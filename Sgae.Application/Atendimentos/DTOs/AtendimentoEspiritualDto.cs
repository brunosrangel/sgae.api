using Sgae.Domain.Enums;

namespace Sgae.Application.Atendimentos.DTOs;

/// <summary>
/// Objeto de transferência de dados (DTO) para AtendimentoEspiritual.
/// </summary>
public class AtendimentoEspiritualDto
{
    public Guid Id { get; set; }
    public Guid AgendamentoId { get; set; }
    public TipoAtendimento Tipo { get; set; }
    public string TipoDescricao { get; set; } = null!;
    public int TempoDuracaoMinutos { get; set; }
    public string TemasAbordados { get; set; } = null!;
    public string Observacoes { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
