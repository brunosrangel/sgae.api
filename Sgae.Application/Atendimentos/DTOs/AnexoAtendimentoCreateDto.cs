namespace Sgae.Application.Atendimentos.DTOs;

/// <summary>
/// DTO de entrada para envio de fotos de búzios, anotações e digitalizações associadas ao atendimento.
/// </summary>
public class AnexoAtendimentoCreateDto
{
    public string NomeArquivo { get; set; } = string.Empty;
    public string TipoArquivo { get; set; } = "image/jpeg";
    public long TamanhoBytes { get; set; }
    public string? Base64Data { get; set; }
    public string? Legenda { get; set; }
    public int RotacaoGraus { get; set; } = 0;
    public string? Categoria { get; set; } = "FotoBuzios";
}
