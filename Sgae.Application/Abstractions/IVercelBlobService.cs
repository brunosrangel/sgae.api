namespace Sgae.Application.Abstractions;

/// <summary>
/// Informações retornadas pela API do Vercel Blob após o upload bem-sucedido de um arquivo.
/// </summary>
public record VercelBlobUploadResult(
    string Url,
    string? DownloadUrl,
    string Pathname,
    string ContentType,
    string? ContentDisposition
);

/// <summary>
/// Contrato para comunicação e envio de binários ao Vercel Blob Storage via REST API.
/// </summary>
public interface IVercelBlobService
{
    /// <summary>
    /// Realiza o upload de um stream binário para o Vercel Blob.
    /// </summary>
    Task<VercelBlobUploadResult> UploadAsync(
        Stream contentStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Realiza o upload de um array de bytes para o Vercel Blob.
    /// </summary>
    Task<VercelBlobUploadResult> UploadBytesAsync(
        byte[] bytes,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove um arquivo armazenado no Vercel Blob a partir de sua URL.
    /// </summary>
    Task<bool> DeleteAsync(
        string blobUrl,
        CancellationToken cancellationToken = default);
}
