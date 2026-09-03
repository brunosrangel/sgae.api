namespace Sgae.Application.Abstractions;

/// <summary>
/// Resultado da validação de segurança e inspeção de Magic Numbers de um arquivo binário/anexo.
/// </summary>
public class FileValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public string? DetectedMimeType { get; set; }
    public string? DetectedExtension { get; set; }
    public string SanitizedFileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public byte[]? DecodedBytes { get; set; }

    public static FileValidationResult Success(
        string sanitizedFileName,
        string detectedMimeType,
        string detectedExtension,
        long fileSizeBytes,
        byte[]? decodedBytes = null)
    {
        return new FileValidationResult
        {
            IsValid = true,
            SanitizedFileName = sanitizedFileName,
            DetectedMimeType = detectedMimeType,
            DetectedExtension = detectedExtension,
            FileSizeBytes = fileSizeBytes,
            DecodedBytes = decodedBytes
        };
    }

    public static FileValidationResult Failure(string errorMessage, string? sanitizedFileName = null)
    {
        return new FileValidationResult
        {
            IsValid = false,
            ErrorMessage = errorMessage,
            SanitizedFileName = sanitizedFileName ?? "arquivo_desconhecido"
        };
    }
}

/// <summary>
/// Contrato de serviço para validações estritas de segurança de arquivos (inspeção de Magic Numbers,
/// detecção real de MIME types, sanitização de caminhos e prevenção contra arquivos maliciosos/executáveis).
/// </summary>
public interface IFileSecurityService
{
    /// <summary>
    /// Valida um anexo a partir de sua representação em Base64 ou nome de arquivo.
    /// Realiza decodificação, checagem de Magic Numbers (assinatura binária), checagem de extensões bloqueadas
    /// e validação de tamanho máximo.
    /// </summary>
    FileValidationResult ValidateBase64File(
        string? base64Data,
        string originalFileName,
        string? declaredMimeType = null,
        long maxSizeBytes = 25 * 1024 * 1024); // 25 MB default

    /// <summary>
    /// Valida um anexo a partir de um array de bytes brutos.
    /// </summary>
    FileValidationResult ValidateRawBytes(
        byte[] bytes,
        string originalFileName,
        string? declaredMimeType = null,
        long maxSizeBytes = 25 * 1024 * 1024);

    /// <summary>
    /// Sanitiza o nome de arquivo, removendo caracteres de path traversal e caracteres de controle ilegais.
    /// </summary>
    string SanitizeFileName(string fileName);
}
