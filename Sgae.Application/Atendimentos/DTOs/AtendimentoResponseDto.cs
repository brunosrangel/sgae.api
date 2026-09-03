namespace Sgae.Application.Atendimentos.DTOs;

/// <summary>
/// DTO de resposta detalhado para o Atendimento Oracular (Jogo de Búzios).
/// </summary>
public class AtendimentoResponseDto
{
    public Guid Id { get; set; }

    public Guid SacerdoteId { get; set; }
    public string SacerdoteNome { get; set; } = string.Empty;

    public Guid ConsulenteId { get; set; }
    public string ConsulenteNome { get; set; } = string.Empty;
    public string? ConsulenteTelefone { get; set; }
    public string? ConsulenteEmail { get; set; }

    public Guid? AgendamentoId { get; set; }

    public DateTime DataConsulta { get; set; }
    public string TipoOraculo { get; set; } = string.Empty;
    public string PerguntaCentral { get; set; } = string.Empty;
    public string VeredictoEspiritual { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Observacoes { get; set; } = string.Empty;

    public int QuantidadeAnexos { get; set; }
    public List<AnexoAtendimentoResponseDto> Anexos { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
