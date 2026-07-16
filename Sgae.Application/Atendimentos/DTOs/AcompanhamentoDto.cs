namespace Sgae.Application.Atendimentos.DTOs;

/// <summary>
/// Objeto de transferência de dados (DTO) para Acompanhamento.
/// </summary>
public class AcompanhamentoDto
{
    public Guid Id { get; set; }
    public Guid AtendimentoEspiritualId { get; set; }
    public DateTime DataAcompanhamento { get; set; }
    public string SintomasMelhora { get; set; } = null!;
    public string Recomendacoes { get; set; } = null!;
    public string Observacoes { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
