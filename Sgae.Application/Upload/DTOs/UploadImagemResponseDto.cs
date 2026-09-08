namespace Sgae.Application.Upload.DTOs;

/// <summary>
/// Resposta com dados do upload bem-sucedido e persistido no SGAE.
/// </summary>
public class UploadImagemResponseDto
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
    public DateTime CreatedAt { get; set; }
    public string Message { get; set; } = "Upload realizado com sucesso no Vercel Blob.";
}
