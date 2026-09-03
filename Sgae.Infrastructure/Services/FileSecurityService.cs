using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Sgae.Application.Abstractions;

namespace Sgae.Infrastructure.Services;

/// <summary>
/// Serviço de validação e segurança avançada de arquivos e anexos.
/// Realiza inspeção de cabeçalhos binários reais (Magic Numbers), detecção e correção de MIME types,
/// sanitização rigorosa de nomes de arquivos e bloqueio proativo contra arquivos executáveis e maliciosos.
/// </summary>
public class FileSecurityService : IFileSecurityService
{
    private readonly ILogger<FileSecurityService> _logger;

    // Lista de extensões perigosas estritamente bloqueadas
    private static readonly HashSet<string> BlockedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe", ".dll", ".bat", ".cmd", ".sh", ".vbs", ".js", ".scr", ".ps1", ".psm1",
        ".com", ".pif", ".hta", ".cpl", ".jar", ".msi", ".msp", ".php", ".phtml", ".py",
        ".asp", ".aspx", ".jsp", ".elf", ".so", ".dylib", ".cgi", ".reg", ".wsf", ".action"
    };

    // Definição de assinaturas de Magic Numbers suportadas para fotos de búzios e documentos litúrgicos
    private record MagicNumberRule(byte[] MagicBytes, string MimeType, string DefaultExtension, int Offset = 0);

    private static readonly List<MagicNumberRule> MagicNumberRules = new()
    {
        // JPEG / JPG: FF D8 FF
        new MagicNumberRule(new byte[] { 0xFF, 0xD8, 0xFF }, "image/jpeg", ".jpg"),

        // PNG: 89 50 4E 47 0D 0A 1A 0A
        new MagicNumberRule(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, "image/png", ".png"),

        // GIF87a & GIF89a: 47 49 46 38 37 61 / 47 49 46 38 39 61
        new MagicNumberRule(new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }, "image/gif", ".gif"),
        new MagicNumberRule(new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }, "image/gif", ".gif"),

        // PDF: 25 50 44 46 (%PDF)
        new MagicNumberRule(new byte[] { 0x25, 0x50, 0x44, 0x46 }, "application/pdf", ".pdf"),

        // BMP: 42 4D
        new MagicNumberRule(new byte[] { 0x42, 0x4D }, "image/bmp", ".bmp"),

        // TIFF (Intel / Motorola)
        new MagicNumberRule(new byte[] { 0x49, 0x49, 0x2A, 0x00 }, "image/tiff", ".tiff"),
        new MagicNumberRule(new byte[] { 0x4D, 0x4D, 0x00, 0x2A }, "image/tiff", ".tiff"),

        // Audio WAV: RIFF .... WAVE
        new MagicNumberRule(new byte[] { 0x57, 0x41, 0x56, 0x45 }, "audio/wav", ".wav", 8),

        // Audio MP3 (ID3 tag): 49 44 33
        new MagicNumberRule(new byte[] { 0x49, 0x44, 0x33 }, "audio/mpeg", ".mp3"),

        // Audio OGG: 4F 67 67 53
        new MagicNumberRule(new byte[] { 0x4F, 0x67, 0x67, 0x53 }, "audio/ogg", ".ogg"),
    };

    public FileSecurityService(ILogger<FileSecurityService> logger)
    {
        _logger = logger;
    }

    public FileValidationResult ValidateBase64File(
        string? base64Data,
        string originalFileName,
        string? declaredMimeType = null,
        long maxSizeBytes = 25 * 1024 * 1024)
    {
        var sanitizedName = SanitizeFileName(originalFileName);

        if (string.IsNullOrWhiteSpace(base64Data))
        {
            return FileValidationResult.Failure("O conteúdo do arquivo anexado está vazio.", sanitizedName);
        }

        try
        {
            var rawBase64 = base64Data;
            // Remover prefixo de Data URI Scheme se presente (ex: data:image/jpeg;base64,...)
            if (rawBase64.Contains(","))
            {
                var commaIndex = rawBase64.IndexOf(",");
                var header = rawBase64.Substring(0, commaIndex);
                if (string.IsNullOrWhiteSpace(declaredMimeType) && header.Contains("data:") && header.Contains(";"))
                {
                    declaredMimeType = header.Split(';')[0].Replace("data:", "").Trim();
                }
                rawBase64 = rawBase64.Substring(commaIndex + 1);
            }

            var bytes = Convert.FromBase64String(rawBase64);
            return ValidateRawBytes(bytes, originalFileName, declaredMimeType, maxSizeBytes);
        }
        catch (FormatException ex)
        {
            _logger.LogWarning(ex, "[Security] Falha ao decodificar Base64 do arquivo {FileName}", sanitizedName);
            return FileValidationResult.Failure("Formato Base64 inválido ou corrompido.", sanitizedName);
        }
    }

    public FileValidationResult ValidateRawBytes(
        byte[] bytes,
        string originalFileName,
        string? declaredMimeType = null,
        long maxSizeBytes = 25 * 1024 * 1024)
    {
        var sanitizedName = SanitizeFileName(originalFileName);

        if (bytes == null || bytes.Length == 0)
        {
            return FileValidationResult.Failure("O arquivo não possui dados binários (0 bytes).", sanitizedName);
        }

        if (bytes.Length > maxSizeBytes)
        {
            var maxMb = maxSizeBytes / (1024 * 1024);
            return FileValidationResult.Failure($"O arquivo excede o limite máximo permitido de {maxMb} MB.", sanitizedName);
        }

        // 1. Checar extensão do nome do arquivo
        var extension = Path.GetExtension(sanitizedName).ToLowerInvariant();
        if (BlockedExtensions.Contains(extension))
        {
            _logger.LogWarning("[Security] Tentativa de upload bloqueada por extensão proibida: {FileName} ({Extension})", sanitizedName, extension);
            return FileValidationResult.Failure($"Extensão de arquivo '{extension}' não é permitida por motivos de segurança.", sanitizedName);
        }

        // 2. Detecção de Magic Numbers
        var detectedMime = DetectMimeType(bytes);

        // Se for WEBP (RIFF header + WEBP offset 8)
        if (detectedMime == null && IsWebP(bytes))
        {
            detectedMime = "image/webp";
        }

        // Se nenhum magic number for identificado
        if (detectedMime == null)
        {
            // Se for um arquivo de texto simples (ex: .txt / anotação em texto)
            if (IsPlainText(bytes) && (extension == ".txt" || extension == ".csv" || declaredMimeType == "text/plain"))
            {
                detectedMime = "text/plain";
            }
            else
            {
                _logger.LogWarning("[Security] Assinatura binária (Magic Number) desconhecida ou não suportada para o arquivo {FileName}", sanitizedName);
                return FileValidationResult.Failure("O tipo de arquivo não é suportado ou possui cabeçalho binário inválido.", sanitizedName);
            }
        }

        // 3. Validação de consistência entre tipo declarado e tipo real detectado
        if (!string.IsNullOrWhiteSpace(declaredMimeType) &&
            !declaredMimeType.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase))
        {
            var cleanDeclared = declaredMimeType.ToLowerInvariant().Trim();
            // Se declarou uma imagem mas é outro formato inconsistente
            if (cleanDeclared.StartsWith("image/") && !detectedMime.StartsWith("image/"))
            {
                _logger.LogWarning("[Security] Inconsistência de MIME type: Declarado={Declared}, Real={Real} no arquivo {FileName}",
                    cleanDeclared, detectedMime, sanitizedName);
                return FileValidationResult.Failure($"MIME type declarado ({cleanDeclared}) não coincide com o conteúdo real do arquivo ({detectedMime}).", sanitizedName);
            }
        }

        var resolvedExt = !string.IsNullOrEmpty(extension) ? extension : GetExtensionForMime(detectedMime);

        return FileValidationResult.Success(
            sanitizedName,
            detectedMime,
            resolvedExt,
            bytes.Length,
            bytes
        );
    }

    public string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "anexo_" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + ".dat";

        // Remove caminhos completos ou tentativas de directory traversal
        var cleanName = Path.GetFileName(fileName).Trim();

        // Remove caracteres de controle e caracteres inválidos para sistema de arquivos
        var invalidChars = Path.GetInvalidFileNameChars();
        cleanName = new string(cleanName.Where(ch => !invalidChars.Contains(ch) && ch >= 32).ToArray());

        // Normaliza múltiplos espaços e pontos consecutivos
        cleanName = Regex.Replace(cleanName, @"\s+", "_");
        cleanName = Regex.Replace(cleanName, @"\.{2,}", ".");

        if (string.IsNullOrWhiteSpace(cleanName) || cleanName == ".")
            return "anexo_" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + ".dat";

        return cleanName;
    }

    private static string? DetectMimeType(byte[] bytes)
    {
        foreach (var rule in MagicNumberRules)
        {
            if (bytes.Length >= rule.Offset + rule.MagicBytes.Length)
            {
                var match = true;
                for (var i = 0; i < rule.MagicBytes.Length; i++)
                {
                    if (bytes[rule.Offset + i] != rule.MagicBytes[i])
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    return rule.MimeType;
                }
            }
        }

        return null;
    }

    private static bool IsWebP(byte[] bytes)
    {
        // RIFF (bytes 0..3: 0x52, 0x49, 0x46, 0x46) + WEBP (bytes 8..11: 0x57, 0x45, 0x42, 0x50)
        if (bytes.Length < 12) return false;

        return bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46
            && bytes[8] == 0x57 && bytes[9] == 0x45 && bytes[10] == 0x42 && bytes[11] == 0x50;
    }

    private static bool IsPlainText(byte[] bytes)
    {
        var sampleSize = Math.Min(bytes.Length, 1024);
        for (var i = 0; i < sampleSize; i++)
        {
            var b = bytes[i];
            // Caracteres permitidos: tab, newline, carriage return e caracteres imprimíveis (32..126) ou UTF-8 multi-byte (> 127)
            if (b < 32 && b != 9 && b != 10 && b != 13)
            {
                return false;
            }
        }
        return true;
    }

    private static string GetExtensionForMime(string mimeType)
    {
        return mimeType.ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/gif" => ".gif",
            "image/webp" => ".webp",
            "image/bmp" => ".bmp",
            "image/tiff" => ".tiff",
            "application/pdf" => ".pdf",
            "audio/wav" => ".wav",
            "audio/mpeg" => ".mp3",
            "audio/ogg" => ".ogg",
            "text/plain" => ".txt",
            _ => ".dat"
        };
    }
}
