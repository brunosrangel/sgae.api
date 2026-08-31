using System;
using Microsoft.EntityFrameworkCore;
using Sgae.Application.Abstractions;
using Sgae.Application.Common.CQRS;
using Sgae.Domain.Entities;
using Sgae.Domain.Enums;
using Sgae.Domain.Repositories;

namespace Sgae.Application.Agendamentos.Commands.CreateAgendamento;

/// <summary>
/// Manipulador responsável pelo fluxo transacional de criação de um agendamento espiritual.
/// Vincula as entidades relacionais de Sacerdote e ServicoConsulta e mapeia todas as propriedades do payload.
/// </summary>
public class CreateAgendamentoCommandHandler : ICommandHandler<CreateAgendamentoCommand, Guid>
{
    private readonly IAgendamentoRepository _agendamentoRepository;
    private readonly ILeadRepository _leadRepository;
    private readonly IAppDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAgendamentoCommandHandler(
        IAgendamentoRepository agendamentoRepository,
        ILeadRepository leadRepository,
        IAppDbContext context,
        IUnitOfWork unitOfWork)
    {
        _agendamentoRepository = agendamentoRepository;
        _leadRepository = leadRepository;
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateAgendamentoCommand request, CancellationToken cancellationToken)
    {
        // 1. Garante a integridade lógica: Consulente deve existir previamente na base
        var leadExists = await _leadRepository.GetByIdAsync(request.LeadId, cancellationToken);

        if (leadExists == null)
        {
            if (!string.IsNullOrWhiteSpace(request.LeadNome))
            {
                var leadName = request.LeadNome.Trim();
                var leadPhone = !string.IsNullOrWhiteSpace(request.LeadTelefone) && request.LeadTelefone.Trim().Length >= 8 
                    ? request.LeadTelefone.Trim() 
                    : "(11) 99999-9999";
                var cleanEmailName = new string(leadName.Where(char.IsLetterOrDigit).ToArray()).ToLower();
                var email = !string.IsNullOrEmpty(cleanEmailName) ? $"{cleanEmailName}@sgae.com.br" : "consulente@sgae.com.br";

                var novoLead = new Lead(
                    nome: leadName,
                    telefone: leadPhone,
                    email: email,
                    cidade: "São Paulo",
                    estado: "SP",
                    origem: OrigemContato.Outros,
                    problemaPrincipal: "Consulta Espiritual Agendada"
                );

                if (request.LeadId != Guid.Empty)
                {
                    typeof(Sgae.Domain.Common.BaseEntity).GetProperty("Id")?.SetValue(novoLead, request.LeadId);
                }

                await _leadRepository.AddAsync(novoLead, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            else
            {
                throw new ArgumentException($"O consulente de ID '{request.LeadId}' não foi localizado no sistema.");
            }
        }

        // 2. Resolve a data e hora em formato UTC seguro utilizando o helper de domínio
        DateTime dataHoraUtc = !string.IsNullOrWhiteSpace(request.Data)
            ? Sgae.Domain.Common.DateTimeHelper.ParseUtc(request.Data, request.Horario)
            : Sgae.Domain.Common.DateTimeHelper.EnsureUtc(request.DataHora);

        // 3. Resolve a relação com Sacerdote a partir do ID ou string 'sacerdote' do JSON
        Guid? sacerdoteId = request.SacerdoteId;
        if (!sacerdoteId.HasValue && !string.IsNullOrWhiteSpace(request.Sacerdote))
        {
            var sacerdoteNome = request.Sacerdote.Trim();
            
            // Busca exata ou por aproximação no banco de dados
            var allSacerdotes = await _context.Sacerdotes.Where(s => !s.IsDeleted).ToListAsync(cancellationToken);
            
            var sacerdote = allSacerdotes.FirstOrDefault(s => 
                string.Equals(s.Nome, sacerdoteNome, StringComparison.OrdinalIgnoreCase))
                ?? allSacerdotes.FirstOrDefault(s => 
                    s.Nome.Contains("Sidnei", StringComparison.OrdinalIgnoreCase) && sacerdoteNome.Contains("Sidnei", StringComparison.OrdinalIgnoreCase))
                ?? allSacerdotes.FirstOrDefault(s =>
                    s.Nome.IndexOf(sacerdoteNome, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    sacerdoteNome.IndexOf(s.Nome, StringComparison.OrdinalIgnoreCase) >= 0);

            if (sacerdote != null)
            {
                sacerdoteId = sacerdote.Id;
            }
            else
            {
                var novoSacerdote = new Sacerdote(sacerdoteNome, "Sacerdote Responsável", true);
                await _context.Sacerdotes.AddAsync(novoSacerdote, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                sacerdoteId = novoSacerdote.Id;
            }
        }

        // 4. Resolve a relação com ServicoConsulta a partir do ID ou string 'tipoConsulta' do JSON
        Guid? servicoConsultaId = request.ServicoConsultaId;
        if (!servicoConsultaId.HasValue && !string.IsNullOrWhiteSpace(request.TipoConsulta))
        {
            var tipoConsultaNome = request.TipoConsulta.Trim();
            
            var allServicos = await _context.ServicosConsulta.Where(s => !s.IsDeleted).ToListAsync(cancellationToken);
            
            var servico = allServicos.FirstOrDefault(s => 
                string.Equals(s.Nome, tipoConsultaNome, StringComparison.OrdinalIgnoreCase))
                ?? allServicos.FirstOrDefault(s => 
                    (s.Nome.Contains("Búzios", StringComparison.OrdinalIgnoreCase) || s.Nome.Contains("Buzios", StringComparison.OrdinalIgnoreCase)) &&
                    (tipoConsultaNome.Contains("Búzios", StringComparison.OrdinalIgnoreCase) || tipoConsultaNome.Contains("Buzios", StringComparison.OrdinalIgnoreCase)))
                ?? allServicos.FirstOrDefault(s =>
                    s.Nome.IndexOf(tipoConsultaNome, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    tipoConsultaNome.IndexOf(s.Nome, StringComparison.OrdinalIgnoreCase) >= 0);

            if (servico != null)
            {
                servicoConsultaId = servico.Id;
            }
            else
            {
                var novoServico = new ServicoConsulta(tipoConsultaNome, request.Valor >= 0 ? request.Valor : 250m, true);
                await _context.ServicosConsulta.AddAsync(novoServico, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                servicoConsultaId = novoServico.Id;
            }
        }

        // 5. Normaliza o Status se informado
        var status = StatusAgendamento.Pendente;
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (Enum.TryParse<StatusAgendamento>(request.Status, true, out var parsedStatus))
            {
                status = parsedStatus;
            }
        }

        // 6. Instancia a entidade rica executando as regras de estado
        var agendamento = new Agendamento(
            request.LeadId,
            dataHoraUtc,
            request.Modalidade,
            request.Valor,
            sacerdoteId: sacerdoteId,
            servicoConsultaId: servicoConsultaId,
            formaPagamento: request.FormaPagamento,
            pago: request.Pago,
            observacoes: request.Observacoes,
            whatsappConfirmacaoDisparada: request.WhatsappConfirmacaoDisparada,
            configLembrete: request.ConfigLembrete,
            frequenciaLembrete: request.FrequenciaLembrete,
            status: status
        );

        await _agendamentoRepository.AddAsync(agendamento, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return agendamento.Id;
    }
}