using Sgae.Domain.Entities;

namespace Sgae.Application.Upload.DTOs;

/// <summary>
/// DTO com os dados do arquivo persistido no banco e hospedado no Vercel Blob.
/// </summary>
public class ArquivoBlobDto
{
    public Guid Id { get; set; }
    public string NomeOriginal { get; set; } = null!;
    public string NomeBlob { get; set; } = null!;
    public string BlobUrl { get; set; } = null!;
    public string? DownloadUrl { get; set; }
    public string ContentType { get; set; } = null!;
    public long TamanhoBytes { get; set; }
    public string? Categoria { get; set; }
    public string? Descricao { get; set; }
    public Guid? UsuarioUploadId { get; set; }
    public Guid? EntidadeRelacionadaId { get; set; }
    public DateTime CreatedAt { get; set; }

    public static ArquivoBlobDto FromEntity(ArquivoBlob entity)
    {
        return new ArquivoBlobDto
        {
            Id = entity.Id,
            NomeOriginal = entity.NomeOriginal,
            NomeBlob = entity.NomeBlob,
            BlobUrl = entity.BlobUrl,
            DownloadUrl = entity.DownloadUrl,
            ContentType = entity.ContentType,
            TamanhoBytes = entity.TamanhoBytes,
            Categoria = entity.Categoria,
            Descricao = entity.Descricao,
            UsuarioUploadId = entity.UsuarioUploadId,
            EntidadeRelacionadaId = entity.EntidadeRelacionadaId,
            CreatedAt = entity.CreatedAt
        };
    }
}
