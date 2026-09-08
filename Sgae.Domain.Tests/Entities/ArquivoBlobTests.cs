using Sgae.Domain.Entities;
using Xunit;

namespace Sgae.Domain.Tests.Entities;

public class ArquivoBlobTests
{
    [Fact]
    public void CriarArquivoBlob_ComParametrosValidos_DeveInstanciarComSucesso()
    {
        // Arrange
        var nomeOriginal = "foto_buzios_consulente.jpg";
        var nomeBlob = "uploads/2026/09/unique-foto_buzios_consulente.jpg";
        var blobUrl = "https://awqyq55xgxlcldo7.public.blob.vercel-storage.com/foto.jpg";
        var downloadUrl = "https://awqyq55xgxlcldo7.public.blob.vercel-storage.com/foto.jpg?download=1";
        var contentType = "image/jpeg";
        var tamanhoBytes = 1024 * 500; // 500 KB
        var categoria = "FotoBuzios";
        var descricao = "Caída de búzios mostrando Oyó e Ogum";
        var usuarioId = Guid.NewGuid();
        var entidadeId = Guid.NewGuid();

        // Act
        var arquivo = new ArquivoBlob(
            nomeOriginal,
            nomeBlob,
            blobUrl,
            downloadUrl,
            contentType,
            tamanhoBytes,
            categoria,
            descricao,
            usuarioId,
            entidadeId);

        // Assert
        Assert.NotEqual(Guid.Empty, arquivo.Id);
        Assert.Equal(nomeOriginal, arquivo.NomeOriginal);
        Assert.Equal(nomeBlob, arquivo.NomeBlob);
        Assert.Equal(blobUrl, arquivo.BlobUrl);
        Assert.Equal(downloadUrl, arquivo.DownloadUrl);
        Assert.Equal(contentType, arquivo.ContentType);
        Assert.Equal(tamanhoBytes, arquivo.TamanhoBytes);
        Assert.Equal(categoria, arquivo.Categoria);
        Assert.Equal(descricao, arquivo.Descricao);
        Assert.Equal(usuarioId, arquivo.UsuarioUploadId);
        Assert.Equal(entidadeId, arquivo.EntidadeRelacionadaId);
        Assert.False(arquivo.IsDeleted);
        Assert.True(arquivo.CreatedAt <= DateTime.UtcNow);
    }

    [Theory]
    [InlineData("", "https://url.com")]
    [InlineData("   ", "https://url.com")]
    [InlineData(null, "https://url.com")]
    public void CriarArquivoBlob_ComNomeInvalido_DeveLancarExcecao(string? nome, string url)
    {
        Assert.Throws<ArgumentException>(() => new ArquivoBlob(
            nome!,
            "blob.jpg",
            url,
            null,
            "image/jpeg",
            1000));
    }

    [Theory]
    [InlineData("foto.jpg", "")]
    [InlineData("foto.jpg", "   ")]
    [InlineData("foto.jpg", null)]
    public void CriarArquivoBlob_ComUrlInvalida_DeveLancarExcecao(string nome, string? url)
    {
        Assert.Throws<ArgumentException>(() => new ArquivoBlob(
            nome,
            "blob.jpg",
            url!,
            null,
            "image/jpeg",
            1000));
    }

    [Fact]
    public void UpdateMetadata_DeveAtualizarCamposERegistrarData()
    {
        // Arrange
        var arquivo = new ArquivoBlob(
            "foto.png",
            "blob.png",
            "https://vercel.blob/foto.png",
            null,
            "image/png",
            2048,
            "FotoPerfil",
            "Foto Antiga");

        // Act
        arquivo.UpdateMetadata("FotoAtualizada", "Nova Foto de Perfil");

        // Assert
        Assert.Equal("FotoAtualizada", arquivo.Categoria);
        Assert.Equal("Nova Foto de Perfil", arquivo.Descricao);
        Assert.NotNull(arquivo.UpdatedAt);
    }
}
