using System;
using Sgae.Domain.Common;

namespace Sgae.Domain.Entities;

/// <summary>
/// Histórico de interações e movimentações do Lead/Consulente.
/// </summary>
public class LeadHistorico : BaseEntity
{
    private LeadHistorico() { }

    public LeadHistorico(Guid leadId, string tipo, string descricao, DateTime? data = null)
    {
        if (leadId == Guid.Empty)
            throw new ArgumentException("LeadId é obrigatório para o histórico.");

        if (string.IsNullOrWhiteSpace(tipo))
            throw new ArgumentException("Tipo de histórico é obrigatório.");

        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("Descrição do histórico é obrigatória.");

        LeadId = leadId;
        Tipo = tipo.Trim();
        Descricao = descricao.Trim();
        Data = data ?? DateTime.UtcNow;
    }

    public Guid LeadId { get; private set; }
    public virtual Lead Lead { get; private set; } = null!;

    public DateTime Data { get; private set; }
    public string Tipo { get; private set; } = null!;
    public string Descricao { get; private set; } = null!;

    public void Update(string tipo, string descricao, DateTime? data = null)
    {
        if (string.IsNullOrWhiteSpace(tipo))
            throw new ArgumentException("Tipo de histórico é obrigatório.");

        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("Descrição do histórico é obrigatória.");

        Tipo = tipo.Trim();
        Descricao = descricao.Trim();
        if (data.HasValue)
        {
            Data = data.Value;
        }
        RegisterUpdate();
    }
}
