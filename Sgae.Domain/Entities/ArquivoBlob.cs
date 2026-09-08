using Sgae.Domain.Common;

namespace Sgae.Domain.Entities;

/// <summary>
/// Representa um arquivo binário ou imagem armazenada externamente no Vercel Blob Storage,
/// mantendo a rastreabilidade, URLs públicas/download e metadados no banco relacional PostgreSQL.
/// </summary>
public class ArquivoBlob : BaseEntity
{
    private ArquivoBlob() { }

    public ArquivoBlob(
        string nomeOriginal,
        string nomeBlob,
        string blobUrl,
        string? downloadUrl,
        string contentType,
        long tamanhoBytes,
        string? categoria = null,
        string? descricao = null,
        Guid? usuarioUploadId = null,
        Guid? entidadeRelacionadaId = null)
    {
        if (string.IsNullOrWhiteSpace(nomeOriginal))
            throw new ArgumentException("O nome original do arquivo é obrigatório.", nameof(nomeOriginal));

        if (string.IsNullOrWhiteSpace(blobUrl))
            throw new ArgumentException("A URL do Blob é obrigatória.", nameof(blobUrl));

        NomeOriginal = nomeOriginal.Trim();
        NomeBlob = !string.IsNullOrWhiteSpace(nomeBlob) ? nomeBlob.Trim() : nomeOriginal.Trim();
        BlobUrl = blobUrl.Trim();
        DownloadUrl = downloadUrl?.Trim();
        ContentType = !string.IsNullOrWhiteSpace(contentType) ? contentType.Trim() : "application/octet-stream";
        TamanhoBytes = tamanhoBytes >= 0 ? tamanhoBytes : 0;
        Categoria = !string.IsNullOrWhiteSpace(categoria) ? categoria.Trim() : "Geral";
        Descricao = descricao?.Trim();
        UsuarioUploadId = usuarioUploadId;
        EntidadeRelacionadaId = entidadeRelacionadaId;
    }

    public string NomeOriginal { get; private set; } = null!;
    public string NomeBlob { get; private set; } = null!;
    public string BlobUrl { get; private set; } = null!;
    public string? DownloadUrl { get; private set; }
    public string ContentType { get; private set; } = null!;
    public long TamanhoBytes { get; private set; }
    public string? Categoria { get; private set; }
    public string? Descricao { get; private set; }
    public Guid? UsuarioUploadId { get; private set; }
    public Guid? EntidadeRelacionadaId { get; private set; }

    public void UpdateMetadata(string? categoria, string? descricao)
    {
        if (!string.IsNullOrWhiteSpace(categoria))
            Categoria = categoria.Trim();

        Descricao = descricao?.Trim();
        RegisterUpdate();
    }
}
