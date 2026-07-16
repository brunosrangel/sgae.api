namespace Sgae.Application.Agendamentos.DTOs;

public class AgendamentoAtendimentoEspiritualDto
{
    public Guid Id { get; set; }
    public Guid AgendamentoId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public int TempoDuracaoMinutos { get; set; }
    public string TemasAbordados { get; set; } = string.Empty;
    public string Observacoes { get; set; } = string.Empty;
}
