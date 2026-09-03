using Sgae.Application.Common.CQRS;

namespace Sgae.Application.Sacerdotes.DTOs;

public class SacerdoteDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Especialidade { get; set; }
    public bool Ativo { get; set; }
    public DateTime CreatedAt { get; set; }
}
