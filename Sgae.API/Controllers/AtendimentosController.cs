using System.Text;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Sgae.Application.Abstractions;
using Sgae.Application.Atendimentos.Commands.CreateAcompanhamento;
using Sgae.Application.Atendimentos.Commands.CreateAtendimento;
using Sgae.Application.Atendimentos.DTOs;
using Sgae.Application.Atendimentos.Queries.GetAcompanhamentosByAtendimentoId;
using Sgae.Domain.Entities;
using Sgae.Domain.Enums;
using Sgae.Domain.Repositories;

namespace Sgae.API.Controllers;

/// <summary>
/// Controller responsável por expor os endpoints de Gestão de Atendimento Espiritual e Jogo de Búzios (Etapa 4),
/// suportando persistência de diagnósticos oraculares, upload e download de fotos e digitalizações de anotações
/// com validação estrita de Magic Numbers (tipo MIME real), exportação otimizada para CSV/Excel,
/// trilha de auditoria (Audit Log) e acompanhamento contínuo de consulentes.
/// </summary>
[Authorize(Roles = "Admin,Sacerdote,PastoralStaff,Secretaria")]
[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("AtendimentosPolicy")]
public class AtendimentosController : ControllerBase
{
    private readonly IAtendimentoRepository _atendimentoRepository;
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ISender _sender;
    private readonly IFileSecurityService _fileSecurityService;

    public AtendimentosController(
        IAtendimentoRepository atendimentoRepository,
        IAppDbContext context,
        IMapper mapper,
        ISender sender,
        IFileSecurityService fileSecurityService)
    {
        _atendimentoRepository = atendimentoRepository;
        _context = context;
        _mapper = mapper;
        _sender = sender;
        _fileSecurityService = fileSecurityService;
    }

    /// <summary>
    /// Registra um novo Atendimento Oracular / Jogo de Búzios, validando a segurança dos anexos por Magic Numbers
    /// e persistindo o veredicto espiritual e metadados no banco.
    /// </summary>
    /// <param name="dto">Payload completo com dados do atendimento, sacerdote, consulente e lista de anexos em base64.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Dados do Atendimento criado e metadados dos arquivos anexados.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<AtendimentoResponseDto>> CreateAtendimento(
        [FromBody] AtendimentoCreateDto dto,
        CancellationToken cancellationToken)
    {
        if (dto == null)
            return BadRequest(new { message = "Os dados do atendimento são obrigatórios." });

        if (!ModelState.IsValid)
            return UnprocessableEntity(ModelState);

        // Validar existência do Sacerdote
        var sacerdoteExists = await _context.Sacerdotes.AnyAsync(s => s.Id == dto.SacerdoteId, cancellationToken);
        if (!sacerdoteExists)
            return BadRequest(new { message = $"Sacerdote de ID '{dto.SacerdoteId}' não localizado no sistema." });

        // Validar existência do Consulente (Lead)
        var consulente = await _context.Leads.FirstOrDefaultAsync(l => l.Id == dto.ConsulenteId, cancellationToken);
        if (consulente == null)
            return BadRequest(new { message = $"Consulente de ID '{dto.ConsulenteId}' não localizado no sistema." });

        // Validação adicional de segurança e Magic Numbers dos arquivos anexados
        if (dto.Anexos != null && dto.Anexos.Any())
        {
            foreach (var anexoDto in dto.Anexos)
            {
                if (string.IsNullOrWhiteSpace(anexoDto.NomeArquivo) && string.IsNullOrWhiteSpace(anexoDto.Base64Data))
                    continue;

                var validationResult = _fileSecurityService.ValidateBase64File(
                    anexoDto.Base64Data,
                    anexoDto.NomeArquivo,
                    anexoDto.TipoArquivo
                );

                if (!validationResult.IsValid)
                {
                    return BadRequest(new
                    {
                        message = $"Falha de segurança no arquivo '{anexoDto.NomeArquivo}': {validationResult.ErrorMessage}",
                        arquivo = anexoDto.NomeArquivo
                    });
                }

                // Aplicar dados sanitizados e validados com Magic Numbers
                anexoDto.NomeArquivo = validationResult.SanitizedFileName;
                anexoDto.TipoArquivo = validationResult.DetectedMimeType ?? anexoDto.TipoArquivo;
                anexoDto.TamanhoBytes = validationResult.FileSizeBytes;
            }
        }

        // Criar entidade de domínio rica
        var dataConsultaUtc = DateTime.SpecifyKind(dto.DataConsulta, DateTimeKind.Utc);
        var atendimento = new Atendimento(
            dto.SacerdoteId,
            dto.ConsulenteId,
            dataConsultaUtc,
            dto.TipoOraculo,
            dto.PerguntaCentral,
            dto.VeredictoEspiritual,
            dto.AgendamentoId,
            dto.Status ?? "Realizado",
            dto.Observacoes
        );

        // Processar anexos fotográficos e anotações manuscritas
        if (dto.Anexos != null && dto.Anexos.Any())
        {
            foreach (var anexoDto in dto.Anexos)
            {
                if (string.IsNullOrWhiteSpace(anexoDto.NomeArquivo))
                    continue;

                var anexo = new AnexoAtendimento(
                    atendimento.Id,
                    anexoDto.NomeArquivo,
                    anexoDto.TipoArquivo,
                    anexoDto.TamanhoBytes,
                    anexoDto.Base64Data,
                    anexoDto.Legenda,
                    anexoDto.RotacaoGraus,
                    anexoDto.Categoria
                );

                atendimento.AddAnexo(anexo);
            }
        }

        // Se houver agendamento associado, sincronizar seu status para Realizado
        if (dto.AgendamentoId.HasValue && dto.AgendamentoId.Value != Guid.Empty)
        {
            var agendamento = await _context.Agendamentos.FirstOrDefaultAsync(a => a.Id == dto.AgendamentoId.Value, cancellationToken);
            if (agendamento != null)
            {
                agendamento.DefinirStatus(StatusAgendamento.Realizado);
            }
        }

        await _atendimentoRepository.AddAsync(atendimento, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Recarregar com todas as navegações preenchidas
        var savedAtendimento = await _atendimentoRepository.GetByIdWithAnexosAsync(atendimento.Id, cancellationToken);
        var response = _mapper.Map<AtendimentoResponseDto>(savedAtendimento ?? atendimento);

        return CreatedAtAction(nameof(GetAtendimentoById), new { id = atendimento.Id }, response);
    }

    /// <summary>
    /// Lista os atendimentos oraculares registrados, suportando filtros por status, sacerdote, consulente e intervalo de datas.
    /// Utiliza índices de alta performance no banco de dados.
    /// </summary>
    /// <param name="status">Filtro por status (ex: Realizado, Pendente, Cancelado).</param>
    /// <param name="sacerdoteId">Filtro por ID do Sacerdote responsável.</param>
    /// <param name="consulenteId">Filtro por ID do Consulente.</param>
    /// <param name="dataInicio">Filtro por data inicial da consulta.</param>
    /// <param name="dataFim">Filtro por data final da consulta.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lista de atendimentos correspondentes com metadados.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AtendimentoResponseDto>>> GetAtendimentos(
        [FromQuery] string? status = null,
        [FromQuery] Guid? sacerdoteId = null,
        [FromQuery] Guid? consulenteId = null,
        [FromQuery] DateTime? dataInicio = null,
        [FromQuery] DateTime? dataFim = null,
        CancellationToken cancellationToken = default)
    {
        var atendimentos = await _atendimentoRepository.GetWithFiltersAsync(
            status,
            sacerdoteId,
            consulenteId,
            dataInicio,
            dataFim,
            cancellationToken
        );

        var response = _mapper.Map<List<AtendimentoResponseDto>>(atendimentos);
        return Ok(response);
    }

    /// <summary>
    /// Exporta os registros de atendimentos oraculares filtrados em formato CSV estruturado (compatível com Excel),
    /// otimizado com projeções leves (sem carregar payloads binários) para altíssimo desempenho em grandes volumes de dados.
    /// </summary>
    /// <param name="status">Filtro por status.</param>
    /// <param name="sacerdoteId">Filtro por Sacerdote.</param>
    /// <param name="consulenteId">Filtro por Consulente.</param>
    /// <param name="dataInicio">Filtro por data inicial.</param>
    /// <param name="dataFim">Filtro por data final.</param>
    /// <param name="formato">Formato de saída (padrão: "csv").</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Arquivo CSV estruturado com cabeçalho UTF-8 BOM e pontuação compatível com Excel.</returns>
    [HttpGet("export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces("text/csv")]
    public async Task<IActionResult> ExportAtendimentos(
        [FromQuery] string? status = null,
        [FromQuery] Guid? sacerdoteId = null,
        [FromQuery] Guid? consulenteId = null,
        [FromQuery] DateTime? dataInicio = null,
        [FromQuery] DateTime? dataFim = null,
        [FromQuery] string? formato = "csv",
        CancellationToken cancellationToken = default)
    {
        var query = _context.Atendimentos.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            var cleanStatus = status.Trim();
            query = query.Where(a => a.Status == cleanStatus);
        }

        if (sacerdoteId.HasValue && sacerdoteId.Value != Guid.Empty)
        {
            query = query.Where(a => a.SacerdoteId == sacerdoteId.Value);
        }

        if (consulenteId.HasValue && consulenteId.Value != Guid.Empty)
        {
            query = query.Where(a => a.ConsulenteId == consulenteId.Value);
        }

        if (dataInicio.HasValue)
        {
            var dataInicioUtc = DateTime.SpecifyKind(dataInicio.Value, DateTimeKind.Utc);
            query = query.Where(a => a.DataConsulta >= dataInicioUtc);
        }

        if (dataFim.HasValue)
        {
            var dataFimUtc = DateTime.SpecifyKind(dataFim.Value, DateTimeKind.Utc);
            query = query.Where(a => a.DataConsulta <= dataFimUtc);
        }

        // Projeção otimizada para grandes volumes: não carrega Base64Data em memória
        var atendimentosExport = await query
            .OrderByDescending(a => a.DataConsulta)
            .Select(a => new
            {
                a.Id,
                a.DataConsulta,
                SacerdoteNome = a.Sacerdote != null ? a.Sacerdote.Nome : "Não informado",
                ConsulenteNome = a.Consulente != null ? a.Consulente.Nome : "Não informado",
                ConsulenteTelefone = a.Consulente != null ? a.Consulente.Telefone : string.Empty,
                ConsulenteEmail = a.Consulente != null ? a.Consulente.Email : string.Empty,
                a.TipoOraculo,
                a.PerguntaCentral,
                a.VeredictoEspiritual,
                a.Status,
                QtdAnexos = a.Anexos.Count,
                a.Observacoes,
                a.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var csvBuilder = new StringBuilder();

        // Cabeçalhos em português brasileiro com separador ponto-e-vírgula padrão Excel BR
        csvBuilder.AppendLine("ID;Data Consulta;Sacerdote;Consulente;Telefone;E-mail;Tipo Oráculo;Pergunta Central;Veredicto Espiritual / Causa Raiz;Status;Qtd Anexos;Observações;Data Registro");

        foreach (var item in atendimentosExport)
        {
            var linha = string.Join(";", new[]
            {
                EscapeCsv(item.Id.ToString()),
                EscapeCsv(item.DataConsulta.ToString("dd/MM/yyyy HH:mm")),
                EscapeCsv(item.SacerdoteNome),
                EscapeCsv(item.ConsulenteNome),
                EscapeCsv(item.ConsulenteTelefone),
                EscapeCsv(item.ConsulenteEmail),
                EscapeCsv(item.TipoOraculo),
                EscapeCsv(item.PerguntaCentral),
                EscapeCsv(item.VeredictoEspiritual),
                EscapeCsv(item.Status),
                item.QtdAnexos.ToString(),
                EscapeCsv(item.Observacoes ?? string.Empty),
                EscapeCsv(item.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss"))
            });

            csvBuilder.AppendLine(linha);
        }

        // Adiciona UTF-8 BOM (Byte Order Mark) para que caracteres acentuados (ã, é, ç, ó) abram perfeitamente no Excel
        var utf8Bom = new byte[] { 0xEF, 0xBB, 0xBF };
        var csvBytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
        var finalBytes = new byte[utf8Bom.Length + csvBytes.Length];
        Buffer.BlockCopy(utf8Bom, 0, finalBytes, 0, utf8Bom.Length);
        Buffer.BlockCopy(csvBytes, 0, finalBytes, utf8Bom.Length, csvBytes.Length);

        var fileName = $"atendimentos_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
        return File(finalBytes, "text/csv; charset=utf-8", fileName);
    }

    /// <summary>
    /// Obtém os detalhes completos de um atendimento oracular específico pelo seu ID, incluindo metadados dos anexos.
    /// </summary>
    /// <param name="id">ID do Atendimento.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Detalhes do atendimento oracular.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AtendimentoResponseDto>> GetAtendimentoById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var atendimento = await _atendimentoRepository.GetByIdWithAnexosAsync(id, cancellationToken);
        if (atendimento == null)
        {
            return NotFound(new { message = $"Atendimento de ID '{id}' não localizado." });
        }

        var response = _mapper.Map<AtendimentoResponseDto>(atendimento);
        return Ok(response);
    }

    /// <summary>
    /// Atualiza os dados de um atendimento existente (tipo de oráculo, perguntas, veredicto espiritual e status).
    /// As modificações são auditadas automaticamente pelo interceptor do EF Core (Audit Log).
    /// </summary>
    /// <param name="id">ID do Atendimento.</param>
    /// <param name="dto">Dados atualizados.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Atendimento atualizado com sucesso.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AtendimentoResponseDto>> UpdateAtendimento(
        Guid id,
        [FromBody] AtendimentoUpdateDto dto,
        CancellationToken cancellationToken)
    {
        if (dto == null)
            return BadRequest(new { message = "Os dados de atualização são obrigatórios." });

        if (!ModelState.IsValid)
            return UnprocessableEntity(ModelState);

        var atendimento = await _context.Atendimentos
            .Include(a => a.Sacerdote)
            .Include(a => a.Consulente)
            .Include(a => a.Anexos)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (atendimento == null)
        {
            return NotFound(new { message = $"Atendimento de ID '{id}' não localizado." });
        }

        try
        {
            atendimento.UpdateDetalhes(
                dto.TipoOraculo,
                dto.PerguntaCentral,
                dto.VeredictoEspiritual,
                dto.Status ?? "Realizado",
                dto.Observacoes
            );

            await _context.SaveChangesAsync(cancellationToken);

            var response = _mapper.Map<AtendimentoResponseDto>(atendimento);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Remove logicamente um Atendimento e seus Anexos (Soft Delete).
    /// A operação é registrada na trilha de auditoria do sistema.
    /// </summary>
    /// <param name="id">ID do Atendimento a ser removido.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Confirmação de exclusão.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAtendimento(
        Guid id,
        CancellationToken cancellationToken)
    {
        var atendimento = await _context.Atendimentos
            .Include(a => a.Anexos)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (atendimento == null)
        {
            return NotFound(new { message = $"Atendimento de ID '{id}' não localizado." });
        }

        atendimento.Delete();
        foreach (var anexo in atendimento.Anexos)
        {
            anexo.Delete();
        }

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Consulta o histórico de auditoria (Audit Log) com todas as alterações efetuadas no Atendimento e seus anexos.
    /// </summary>
    /// <param name="id">ID do Atendimento.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Trilha de alterações e usuários responsáveis.</returns>
    [HttpGet("{id:guid}/auditoria")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AuditLogDto>>> GetAuditLogsByAtendimentoId(
        Guid id,
        CancellationToken cancellationToken)
    {
        var idStr = id.ToString();
        var logs = await _context.AuditLogs
            .AsNoTracking()
            .Where(al => (al.EntityName == nameof(Atendimento) && al.EntityId == idStr) ||
                         (al.EntityName == nameof(AnexoAtendimento) && al.EntityId == idStr))
            .OrderByDescending(al => al.Timestamp)
            .ToListAsync(cancellationToken);

        var response = _mapper.Map<List<AuditLogDto>>(logs);
        return Ok(response);
    }

    /// <summary>
    /// Lista os metadados de todos os arquivos anexados (fotos de búzios, digitalizações) de um atendimento.
    /// </summary>
    /// <param name="id">ID do Atendimento.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lista de metadados dos arquivos anexados.</returns>
    [HttpGet("{id:guid}/anexos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AnexoAtendimentoResponseDto>>> GetAnexos(
        Guid id,
        CancellationToken cancellationToken)
    {
        var anexos = await _atendimentoRepository.GetAnexosByAtendimentoIdAsync(id, cancellationToken);
        var response = _mapper.Map<List<AnexoAtendimentoResponseDto>>(anexos);
        return Ok(response);
    }

    /// <summary>
    /// Adiciona um novo anexo com validação estrita de Magic Numbers a um atendimento existente.
    /// </summary>
    /// <param name="id">ID do Atendimento.</param>
    /// <param name="anexoDto">Dados e base64 do anexo.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Metadados do anexo persistido com segurança.</returns>
    [HttpPost("{id:guid}/anexos")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AnexoAtendimentoResponseDto>> AddAnexo(
        Guid id,
        [FromBody] AnexoAtendimentoCreateDto anexoDto,
        CancellationToken cancellationToken)
    {
        if (anexoDto == null || string.IsNullOrWhiteSpace(anexoDto.NomeArquivo) || string.IsNullOrWhiteSpace(anexoDto.Base64Data))
        {
            return BadRequest(new { message = "Nome do arquivo e dados em Base64 são obrigatórios." });
        }

        var atendimento = await _atendimentoRepository.GetByIdWithAnexosAsync(id, cancellationToken);
        if (atendimento == null)
        {
            return NotFound(new { message = $"Atendimento de ID '{id}' não localizado." });
        }

        // Validação estrita de Magic Numbers
        var validationResult = _fileSecurityService.ValidateBase64File(
            anexoDto.Base64Data,
            anexoDto.NomeArquivo,
            anexoDto.TipoArquivo
        );

        if (!validationResult.IsValid)
        {
            return BadRequest(new { message = $"Falha na validação do anexo: {validationResult.ErrorMessage}" });
        }

        var anexo = new AnexoAtendimento(
            atendimento.Id,
            validationResult.SanitizedFileName,
            validationResult.DetectedMimeType ?? anexoDto.TipoArquivo,
            validationResult.FileSizeBytes,
            anexoDto.Base64Data,
            anexoDto.Legenda,
            anexoDto.RotacaoGraus,
            anexoDto.Categoria
        );

        atendimento.AddAnexo(anexo);
        await _context.SaveChangesAsync(cancellationToken);

        var response = _mapper.Map<AnexoAtendimentoResponseDto>(anexo);
        return CreatedAtAction(nameof(GetAnexoById), new { id = atendimento.Id, anexoId = anexo.Id }, response);
    }

    /// <summary>
    /// Obtém os metadados de um anexo específico de um atendimento.
    /// </summary>
    /// <param name="id">ID do Atendimento.</param>
    /// <param name="anexoId">ID do Anexo.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Metadados do anexo solicitado.</returns>
    [HttpGet("{id:guid}/anexos/{anexoId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AnexoAtendimentoResponseDto>> GetAnexoById(
        Guid id,
        Guid anexoId,
        CancellationToken cancellationToken)
    {
        var anexo = await _atendimentoRepository.GetAnexoByIdAsync(id, anexoId, cancellationToken);
        if (anexo == null)
        {
            return NotFound(new { message = $"Anexo de ID '{anexoId}' não localizado no atendimento informado." });
        }

        var response = _mapper.Map<AnexoAtendimentoResponseDto>(anexo);
        return Ok(response);
    }

    /// <summary>
    /// Realiza o download ou visualização do arquivo binário anexado (foto de búzios em alta resolução, digitalização de anotações litúrgicas).
    /// </summary>
    /// <param name="id">ID do Atendimento.</param>
    /// <param name="anexoId">ID do Anexo.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Arquivo binário com Content-Type adequado.</returns>
    [HttpGet("{id:guid}/anexos/{anexoId:guid}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadAnexo(
        Guid id,
        Guid anexoId,
        CancellationToken cancellationToken)
    {
        var anexo = await _atendimentoRepository.GetAnexoByIdAsync(id, anexoId, cancellationToken);
        if (anexo == null)
        {
            return NotFound(new { message = $"Anexo de ID '{anexoId}' não localizado." });
        }

        if (string.IsNullOrWhiteSpace(anexo.Base64Data))
        {
            return NotFound(new { message = "O anexo solicitado não possui conteúdo binário armazenado." });
        }

        try
        {
            var base64String = anexo.Base64Data;
            // Remover prefixos comuns no padrão data URI: data:image/png;base64,...
            if (base64String.Contains(","))
            {
                base64String = base64String.Substring(base64String.IndexOf(",") + 1);
            }

            var bytes = Convert.FromBase64String(base64String);
            var contentType = !string.IsNullOrWhiteSpace(anexo.TipoArquivo) ? anexo.TipoArquivo : "application/octet-stream";
            return File(bytes, contentType, anexo.NomeArquivo);
        }
        catch (FormatException)
        {
            return BadRequest(new { message = "Os dados do arquivo anexado estão corrompidos ou em formato base64 inválido." });
        }
    }

    /// <summary>
    /// Registra uma nova sessão de acompanhamento (monitoramento de evolução) para um atendimento espiritual ativo.
    /// </summary>
    /// <param name="id">ID do Atendimento Espiritual (âncora).</param>
    /// <param name="input">Informações do progresso e novas orientações do consulente.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>O ID do acompanhamento registrado.</returns>
    [HttpPost("{id:guid}/acompanhamentos")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateAcompanhamento(
        Guid id,
        [FromBody] CreateAcompanhamentoInputModel input,
        CancellationToken cancellationToken)
    {
        var command = new CreateAcompanhamentoCommand(
            id,
            input.DataAcompanhamento,
            input.SintomasMelhora,
            input.Recomendacoes,
            input.Observacoes
        );

        var acompanhamentoId = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(
            nameof(GetAcompanhamentosByAtendimentoId),
            new { id },
            acompanhamentoId
        );
    }

    /// <summary>
    /// Obtém todas as sessões de acompanhamento/monitoramento registradas para um Atendimento Espiritual.
    /// </summary>
    /// <param name="id">ID do Atendimento Espiritual.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lista de acompanhamentos.</returns>
    [HttpGet("{id:guid}/acompanhamentos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AcompanhamentoDto>>> GetAcompanhamentosByAtendimentoId(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetAcompanhamentosByAtendimentoIdQuery(id), cancellationToken);
        return Ok(result);
    }

    private static string EscapeCsv(string? field)
    {
        if (string.IsNullOrEmpty(field)) return "\"\"";

        var sanitized = field.Replace("\"", "\"\"");

        // Prevenção de CSV Formula Injection (=, +, -, @)
        if (sanitized.StartsWith("=") || sanitized.StartsWith("+") || sanitized.StartsWith("-") || sanitized.StartsWith("@"))
        {
            sanitized = "'" + sanitized;
        }

        return $"\"{sanitized}\"";
    }
}

/// <summary>
/// Modelo de entrada simplificado para criação de Acompanhamento ancorado na rota de Atendimento.
/// </summary>
public class CreateAcompanhamentoInputModel
{
    public DateTime DataAcompanhamento { get; set; }
    public string SintomasMelhora { get; set; } = null!;
    public string Recomendacoes { get; set; } = null!;
    public string Observacoes { get; set; } = null!;
}
