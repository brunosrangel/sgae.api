namespace Sgae.Application.Atendimentos.DTOs;

/// <summary>
/// DTO de resposta para metadados de arquivos e fotos anexadas ao atendimento.
/// </summary>
public class AnexoAtendimentoResponseDto
{
    public Guid Id { get; set; }
    public Guid AtendimentoId { get; set; }
    public string NomeArquivo { get; set; } = string.Empty;
    public string TipoArquivo { get; set; } = string.Empty;
    public long TamanhoBytes { get; set; }
    public string? Legenda { get; set; }
    public int RotacaoGraus { get; set; }
    public string? Categoria { get; set; }
    public bool TemConteudoBinario { get; set; }
    public string? UrlDownload { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO contendo o conteúdo binário/base64 de um anexo para download ou visualização em alta resolução.
/// </summary>
public class AnexoConteudoDto
{
    public Guid Id { get; set; }
    public Guid AtendimentoId { get; set; }
    public string NomeArquivo { get; set; } = string.Empty;
    public string TipoArquivo { get; set; } = string.Empty;
    public string? Base64Data { get; set; }
    public byte[]? Bytes { get; set; }
    public long TamanhoBytes { get; set; }
    public string? Legenda { get; set; }
    public int RotacaoGraus { get; set; }
}
