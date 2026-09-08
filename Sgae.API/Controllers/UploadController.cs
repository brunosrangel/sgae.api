using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sgae.Application.Abstractions;
using Sgae.Application.Upload.DTOs;
using Sgae.Domain.Entities;
using Sgae.Domain.Repositories;

namespace Sgae.API.Controllers;

/// <summary>
/// Controller responsável pelo upload seguro de imagens e arquivos para o Vercel Blob Storage,
/// validação estrita de integridade binária (Magic Numbers), limitação de tamanho (máximo 5MB)
/// e persistência dos caminhos/metadados no banco relacional PostgreSQL para posterior resgate.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UploadController : ControllerBase
{
    private const long MaxFileSizeInBytes = 5 * 1024 * 1024; // 5 MB

    private readonly IVercelBlobService _vercelBlobService;
    private readonly IFileSecurityService _fileSecurityService;
    private readonly IArquivoBlobRepository _arquivoBlobRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UploadController> _logger;

    public UploadController(
        IVercelBlobService vercelBlobService,
        IFileSecurityService fileSecurityService,
        IArquivoBlobRepository arquivoBlobRepository,
        IUnitOfWork unitOfWork,
        ILogger<UploadController> logger)
    {
        _vercelBlobService = vercelBlobService ?? throw new ArgumentNullException(nameof(vercelBlobService));
        _fileSecurityService = fileSecurityService ?? throw new ArgumentNullException(nameof(fileSecurityService));
        _arquivoBlobRepository = arquivoBlobRepository ?? throw new ArgumentNullException(nameof(arquivoBlobRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Realiza o upload de um arquivo de imagem via multipart/form-data diretamente para o Vercel Blob
    /// e persiste o caminho/URL no banco de dados para resgate.
    /// </summary>
    /// <param name="file">Arquivo binário da imagem (máximo 5 MB).</param>
    /// <param name="categoria">Categoria opcional da imagem (ex: FotoBuzios, FotoPerfil, Comprovante, Atendimento).</param>
    /// <param name="descricao">Descrição ou legenda opcional da imagem.</param>
    /// <param name="entidadeRelacionadaId">Id opcional da entidade vinculada (Atendimento, Lead, Usuário, etc.).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    [HttpPost("imagem")]
    [HttpPost("blob")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(UploadImagemResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UploadImagemResponseDto>> UploadImagem(
        [FromForm] IFormFile file,
        [FromForm] string? categoria = "Geral",
        [FromForm] string? descricao = null,
        [FromForm] Guid? entidadeRelacionadaId = null,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new
            {
                message = "Nenhum arquivo foi enviado ou o arquivo possui 0 bytes.",
                code = "EMPTY_FILE"
            });
        }

        // Validação de limite estrito de tamanho (5 MB)
        if (file.Length > MaxFileSizeInBytes)
        {
            var tamanhoMb = Math.Round((double)file.Length / (1024 * 1024), 2);
            _logger.LogWarning("Upload rejeitado: arquivo '{FileName}' possui {Size} MB, excedendo o teto de 5 MB.",
                file.FileName, tamanhoMb);

            return BadRequest(new
            {
                message = $"O arquivo excede o limite máximo permitido de 5 MB. Tamanho recebido: {tamanhoMb} MB.",
                code = "FILE_TOO_LARGE",
                maxSizeBytes = MaxFileSizeInBytes,
                receivedSizeBytes = file.Length
            });
        }

        // Leitura dos bytes do arquivo para inspeção rigorosa
        byte[] fileBytes;
        using (var memoryStream = new MemoryStream())
        {
            await file.CopyToAsync(memoryStream, cancellationToken);
            fileBytes = memoryStream.ToArray();
        }

        // Validação de segurança e Magic Numbers (tipo real de imagem)
        var validation = _fileSecurityService.ValidateRawBytes(
            fileBytes,
            file.FileName,
            file.ContentType,
            MaxFileSizeInBytes
        );

        if (!validation.IsValid)
        {
            _logger.LogWarning("Upload rejeitado por falha de validação de arquivo: {Error}", validation.ErrorMessage);
            return UnprocessableEntity(new
            {
                message = validation.ErrorMessage,
                code = "INVALID_FILE_SECURITY"
            });
        }

        var detectedMime = validation.DetectedMimeType ?? file.ContentType ?? "application/octet-stream";
        if (!detectedMime.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Upload rejeitado: arquivo '{FileName}' possui MIME '{Mime}', que não é uma imagem.",
                file.FileName, detectedMime);

            return BadRequest(new
            {
                message = "Apenas arquivos de imagem (JPEG, PNG, WebP, GIF, BMP, TIFF, SVG) são permitidos neste endpoint.",
                code = "INVALID_IMAGE_MIME_TYPE",
                detectedMimeType = detectedMime
            });
        }

        try
        {
            // Upload para o Vercel Blob
            var blobResult = await _vercelBlobService.UploadBytesAsync(
                fileBytes,
                validation.SanitizedFileName,
                detectedMime,
                cancellationToken
            );

            // Identificação do usuário logado se houver token JWT
            Guid? usuarioId = null;
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdClaim, out var parsedGuid))
            {
                usuarioId = parsedGuid;
            }

            // Persistência da URL e metadados no banco relacional PostgreSQL
            var arquivoBlob = new ArquivoBlob(
                nomeOriginal: validation.SanitizedFileName,
                nomeBlob: blobResult.Pathname,
                blobUrl: blobResult.Url,
                downloadUrl: blobResult.DownloadUrl,
                contentType: blobResult.ContentType,
                tamanhoBytes: fileBytes.Length,
                categoria: !string.IsNullOrWhiteSpace(categoria) ? categoria : "Geral",
                descricao: descricao,
                usuarioUploadId: usuarioId,
                entidadeRelacionadaId: entidadeRelacionadaId
            );

            await _arquivoBlobRepository.AddAsync(arquivoBlob, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Upload concluído com sucesso: Arquivo ID '{Id}', URL '{Url}' salvo no banco.",
                arquivoBlob.Id, arquivoBlob.BlobUrl);

            var response = new UploadImagemResponseDto
            {
                Id = arquivoBlob.Id,
                NomeOriginal = arquivoBlob.NomeOriginal,
                NomeBlob = arquivoBlob.NomeBlob,
                BlobUrl = arquivoBlob.BlobUrl,
                DownloadUrl = arquivoBlob.DownloadUrl,
                ContentType = arquivoBlob.ContentType,
                TamanhoBytes = arquivoBlob.TamanhoBytes,
                Categoria = arquivoBlob.Categoria,
                Descricao = arquivoBlob.Descricao,
                CreatedAt = arquivoBlob.CreatedAt,
                Message = "Imagem armazenada com sucesso no Vercel Blob e registrada no banco de dados."
            };

            return CreatedAtAction(nameof(GetById), new { id = arquivoBlob.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno durante upload para o Vercel Blob ou persistência no banco: {Message}", ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "Ocorreu um erro ao processar o upload da imagem no Vercel Blob.",
                code = "VERCEL_BLOB_UPLOAD_ERROR",
                details = ex.Message
            });
        }
    }

    /// <summary>
    /// Realiza o upload de uma imagem enviada em formato Base64 (ex: capturas de canvas, câmera ou digitalizações).
    /// </summary>
    [HttpPost("imagem-base64")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(UploadImagemResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UploadImagemResponseDto>> UploadImagemBase64(
        [FromBody] UploadImagemBase64RequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Base64Data))
        {
            return BadRequest(new
            {
                message = "O conteúdo Base64 da imagem é obrigatório.",
                code = "EMPTY_BASE64_DATA"
            });
        }

        var validation = _fileSecurityService.ValidateBase64File(
            request.Base64Data,
            request.NomeArquivo ?? $"foto_{Guid.NewGuid():N}.jpg",
            null,
            MaxFileSizeInBytes
        );

        if (!validation.IsValid)
        {
            return UnprocessableEntity(new
            {
                message = validation.ErrorMessage,
                code = "INVALID_FILE_SECURITY"
            });
        }

        var detectedMime = validation.DetectedMimeType ?? "application/octet-stream";
        if (!detectedMime.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message = "Apenas arquivos de imagem são permitidos.",
                code = "INVALID_IMAGE_MIME_TYPE",
                detectedMimeType = detectedMime
            });
        }

        try
        {
            var fileBytes = validation.DecodedBytes!;
            var blobResult = await _vercelBlobService.UploadBytesAsync(
                fileBytes,
                validation.SanitizedFileName,
                detectedMime,
                cancellationToken
            );

            Guid? usuarioId = null;
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdClaim, out var parsedGuid))
            {
                usuarioId = parsedGuid;
            }

            var arquivoBlob = new ArquivoBlob(
                nomeOriginal: validation.SanitizedFileName,
                nomeBlob: blobResult.Pathname,
                blobUrl: blobResult.Url,
                downloadUrl: blobResult.DownloadUrl,
                contentType: blobResult.ContentType,
                tamanhoBytes: fileBytes.Length,
                categoria: !string.IsNullOrWhiteSpace(request.Categoria) ? request.Categoria : "Geral",
                descricao: request.Descricao,
                usuarioUploadId: usuarioId,
                entidadeRelacionadaId: request.EntidadeRelacionadaId
            );

            await _arquivoBlobRepository.AddAsync(arquivoBlob, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new UploadImagemResponseDto
            {
                Id = arquivoBlob.Id,
                NomeOriginal = arquivoBlob.NomeOriginal,
                NomeBlob = arquivoBlob.NomeBlob,
                BlobUrl = arquivoBlob.BlobUrl,
                DownloadUrl = arquivoBlob.DownloadUrl,
                ContentType = arquivoBlob.ContentType,
                TamanhoBytes = arquivoBlob.TamanhoBytes,
                Categoria = arquivoBlob.Categoria,
                Descricao = arquivoBlob.Descricao,
                CreatedAt = arquivoBlob.CreatedAt,
                Message = "Imagem Base64 armazenada com sucesso no Vercel Blob e persistida no banco."
            };

            return CreatedAtAction(nameof(GetById), new { id = arquivoBlob.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no upload Base64: {Message}", ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "Erro ao realizar upload do arquivo Base64 no Vercel Blob.",
                code = "VERCEL_BLOB_UPLOAD_ERROR",
                details = ex.Message
            });
        }
    }

    /// <summary>
    /// Resgata as informações e o caminho/URL da imagem persistida no banco a partir de seu ID.
    /// </summary>
    /// <param name="id">Identificador único (UUID) do arquivo salvo no banco.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    [HttpGet("blob/{id:guid}")]
    [ProducesResponseType(typeof(ArquivoBlobDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ArquivoBlobDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var arquivo = await _arquivoBlobRepository.GetByIdAsync(id, cancellationToken);
        if (arquivo == null || arquivo.IsDeleted)
        {
            return NotFound(new
            {
                message = $"Arquivo com ID '{id}' não foi encontrado no banco de dados.",
                code = "FILE_NOT_FOUND"
            });
        }

        return Ok(ArquivoBlobDto.FromEntity(arquivo));
    }

    /// <summary>
    /// Lista os arquivos e imagens armazenados no banco, com suporte a paginação e filtros.
    /// </summary>
    /// <param name="categoria">Filtro opcional por categoria (ex: FotoBuzios, FotoPerfil).</param>
    /// <param name="entidadeRelacionadaId">Filtro opcional por ID da entidade vinculada.</param>
    /// <param name="page">Número da página (padrão 1).</param>
    /// <param name="pageSize">Tamanho da página (padrão 20, máx 100).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    [HttpGet("blob")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> Listar(
        [FromQuery] string? categoria = null,
        [FromQuery] Guid? entidadeRelacionadaId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        IEnumerable<ArquivoBlob> query;

        if (entidadeRelacionadaId.HasValue)
        {
            query = await _arquivoBlobRepository.GetByEntidadeRelacionadaIdAsync(entidadeRelacionadaId.Value, cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(categoria))
        {
            query = await _arquivoBlobRepository.GetByCategoriaAsync(categoria, cancellationToken);
        }
        else
        {
            query = await _arquivoBlobRepository.GetAllAsync(cancellationToken);
        }

        var list = query
            .Where(a => !a.IsDeleted)
            .OrderByDescending(a => a.CreatedAt)
            .ToList();

        var totalItems = list.Count;
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        var items = list
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ArquivoBlobDto.FromEntity)
            .ToList();

        return Ok(new
        {
            page,
            pageSize,
            totalItems,
            totalPages,
            items
        });
    }

    /// <summary>
    /// Remove um arquivo tanto do Vercel Blob quanto do banco de dados relacional (Soft Delete).
    /// </summary>
    /// <param name="id">Identificador único do arquivo.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    [HttpDelete("blob/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var arquivo = await _arquivoBlobRepository.GetByIdAsync(id, cancellationToken);
        if (arquivo == null || arquivo.IsDeleted)
        {
            return NotFound(new
            {
                message = $"Arquivo com ID '{id}' não encontrado para exclusão.",
                code = "FILE_NOT_FOUND"
            });
        }

        try
        {
            // Tenta deletar no Vercel Blob
            if (!string.IsNullOrWhiteSpace(arquivo.BlobUrl))
            {
                await _vercelBlobService.DeleteAsync(arquivo.BlobUrl, cancellationToken);
            }

            // Exclui do banco de dados (Soft delete)
            _arquivoBlobRepository.Delete(arquivo);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Arquivo ID '{Id}' ({BlobUrl}) removido com sucesso.", id, arquivo.BlobUrl);
            return Ok(new
            {
                message = "Arquivo excluído com sucesso do Vercel Blob e do banco de dados.",
                id
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir arquivo ID '{Id}': {Message}", id, ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "Erro ao excluir o arquivo do armazenamento.",
                code = "DELETE_FILE_ERROR"
            });
        }
    }
}
