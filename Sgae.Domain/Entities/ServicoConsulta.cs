using Sgae.Domain.Common;

namespace Sgae.Domain.Entities;

/// <summary>
/// Serviços de Consulta Espiritual e Tarifas (ex: "Jogo de Búzios Presencial")
/// </summary>
public class ServicoConsulta : BaseEntity
{
    private ServicoConsulta() { }

    public ServicoConsulta(string nome, decimal tarifa, bool ativo = true)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome do serviço de consulta é obrigatório.");

        if (tarifa < 0)
            throw new ArgumentException("A tarifa não pode ser um valor negativo.");

        Nome = nome.Trim();
        Tarifa = tarifa;
        Ativo = ativo;
    }

    public string Nome { get; private set; } = null!;
    public decimal Tarifa { get; private set; }
    public bool Ativo { get; private set; } = true;

    public virtual ICollection<Agendamento> Agendamentos { get; private set; } = new List<Agendamento>();

    public void Update(string nome, decimal tarifa, bool ativo)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome do serviço de consulta é obrigatório.");

        if (tarifa < 0)
            throw new ArgumentException("A tarifa não pode ser um valor negativo.");

        Nome = nome.Trim();
        Tarifa = tarifa;
        Ativo = ativo;
        RegisterUpdate();
    }
}
