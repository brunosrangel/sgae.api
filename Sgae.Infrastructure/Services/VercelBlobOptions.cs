namespace Sgae.Infrastructure.Services;

/// <summary>
/// Opções de configuração para autenticação e integração com a Vercel Blob Storage API.
/// </summary>
public class VercelBlobOptions
{
    public const string SectionName = "VercelBlob";

    public string StoreId { get; set; } = "store_AWqyq55xGXLcLDO7";
    public string ReadWriteToken { get; set; } = "vercel_blob_rw_AWqyq55xGXLcLDO7_ph81eY76E7au9Q2BAZvdhPNJedpsiO";
    public string BaseUrl { get; set; } = "https://blob.vercel-storage.com";
    public string ApiVersion { get; set; } = "7";
}
