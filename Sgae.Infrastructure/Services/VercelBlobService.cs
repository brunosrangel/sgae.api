using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sgae.Application.Abstractions;

namespace Sgae.Infrastructure.Services;

/// <summary>
/// Implementação robusta do cliente HTTP para o Vercel Blob Storage REST API.
/// </summary>
public class VercelBlobService : IVercelBlobService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IOptions<VercelBlobOptions> _options;
    private readonly ILogger<VercelBlobService> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public VercelBlobService(
        HttpClient httpClient,
        IConfiguration configuration,
        IOptions<VercelBlobOptions> options,
        ILogger<VercelBlobService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<VercelBlobUploadResult> UploadAsync(
        Stream contentStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream();
        await contentStream.CopyToAsync(memoryStream, cancellationToken);
        var bytes = memoryStream.ToArray();
        return await UploadBytesAsync(bytes, fileName, contentType, cancellationToken);
    }

    public async Task<VercelBlobUploadResult> UploadBytesAsync(
        byte[] bytes,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var token = GetReadWriteToken();
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("O token 'BLOB_READ_WRITE_TOKEN' do Vercel Blob não está configurado.");
        }

        var sanitizedFileName = SanitizeFileName(fileName);
        var extension = Path.GetExtension(sanitizedFileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = GetDefaultExtensionForMime(contentType);
            sanitizedFileName += extension;
        }

        var blobPath = $"uploads/{DateTime.UtcNow:yyyy/MM}/{Guid.NewGuid():N}-{sanitizedFileName}";
        var baseUrl = (_options.Value.BaseUrl ?? "https://blob.vercel-storage.com").TrimEnd('/');
        var targetUrl = $"{baseUrl}/{Uri.EscapeDataString(blobPath)}?access=public";

        _logger.LogInformation("Vercel Blob: Iniciando envio do arquivo '{FileName}' ({Size} bytes, Mime: {Mime}) para '{BlobPath}'.",
            sanitizedFileName, bytes.Length, contentType, blobPath);

        using var request = new HttpRequestMessage(HttpMethod.Put, targetUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.TryAddWithoutValidation("x-api-version", _options.Value.ApiVersion ?? "7");

        var byteArrayContent = new ByteArrayContent(bytes);
        byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        request.Content = byteArrayContent;

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Vercel Blob: Falha ao fazer upload do arquivo '{FileName}'. HTTP {StatusCode}. Resposta: {ResponseBody}",
                sanitizedFileName, (int)response.StatusCode, responseBody);
            throw new InvalidOperationException($"Falha no upload para o Vercel Blob (HTTP {(int)response.StatusCode}): {responseBody}");
        }

        var parsed = JsonSerializer.Deserialize<VercelBlobApiResponse>(responseBody, JsonOpts);
        if (parsed == null || string.IsNullOrWhiteSpace(parsed.Url))
        {
            _logger.LogError("Vercel Blob: Resposta inválida da API ao enviar '{FileName}'. Conteúdo: {ResponseBody}", sanitizedFileName, responseBody);
            throw new InvalidOperationException("A API do Vercel Blob retornou uma resposta sem a URL pública do arquivo.");
        }

        _logger.LogInformation("Vercel Blob: Arquivo '{FileName}' enviado com sucesso. URL pública gerada: '{BlobUrl}'",
            sanitizedFileName, parsed.Url);

        return new VercelBlobUploadResult(
            Url: parsed.Url,
            DownloadUrl: parsed.DownloadUrl,
            Pathname: parsed.Pathname ?? blobPath,
            ContentType: parsed.ContentType ?? contentType,
            ContentDisposition: parsed.ContentDisposition
        );
    }

    public async Task<bool> DeleteAsync(string blobUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(blobUrl))
            return false;

        var token = GetReadWriteToken();
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("O token 'BLOB_READ_WRITE_TOKEN' do Vercel Blob não está configurado.");
        }

        var baseUrl = (_options.Value.BaseUrl ?? "https://blob.vercel-storage.com").TrimEnd('/');
        var deleteEndpoint = $"{baseUrl}/delete";

        using var request = new HttpRequestMessage(HttpMethod.Post, deleteEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.TryAddWithoutValidation("x-api-version", _options.Value.ApiVersion ?? "7");

        var payload = JsonSerializer.Serialize(new { urls = new[] { blobUrl } }, JsonOpts);
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("Vercel Blob: Arquivo '{BlobUrl}' removido com sucesso.", blobUrl);
            return true;
        }

        var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogWarning("Vercel Blob: Falha ao deletar arquivo '{BlobUrl}'. HTTP {StatusCode}. Resposta: {ErrorBody}",
            blobUrl, (int)response.StatusCode, errorBody);
        return false;
    }

    private string GetReadWriteToken()
    {
        return Environment.GetEnvironmentVariable("BLOB_READ_WRITE_TOKEN")
            ?? _configuration["BLOB_READ_WRITE_TOKEN"]
            ?? _configuration["VercelBlob:ReadWriteToken"]
            ?? _options.Value.ReadWriteToken;
    }

    private static string SanitizeFileName(string fileName)
    {
        var rawName = Path.GetFileName(fileName);
        var clean = new string(rawName.Where(c => char.IsLetterOrDigit(c) || c == '.' || c == '-' || c == '_').ToArray());
        return string.IsNullOrWhiteSpace(clean) ? $"arquivo_{Guid.NewGuid():N}.bin" : clean;
    }

    private static string GetDefaultExtensionForMime(string mimeType)
    {
        return mimeType.ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            "image/gif" => ".gif",
            "image/svg+xml" => ".svg",
            "image/bmp" => ".bmp",
            "image/tiff" => ".tiff",
            _ => ".bin"
        };
    }

    private class VercelBlobApiResponse
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("downloadUrl")]
        public string? DownloadUrl { get; set; }

        [JsonPropertyName("pathname")]
        public string? Pathname { get; set; }

        [JsonPropertyName("contentType")]
        public string? ContentType { get; set; }

        [JsonPropertyName("contentDisposition")]
        public string? ContentDisposition { get; set; }
    }
}
