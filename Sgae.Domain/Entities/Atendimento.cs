using Sgae.Domain.Common;

namespace Sgae.Domain.Entities;

/// <summary>
/// Entidade de domínio rica representando o Atendimento Espiritual e Consulta Oracular (Jogo de Búzios, Ifá, etc.).
/// Centraliza o registro sacerdotal, a pergunta vital do consulente, o diagnóstico/veredicto espiritual e os anexos fotográficos/litúrgicos.
/// </summary>
public class Atendimento : BaseEntity
{
    private readonly List<AnexoAtendimento> _anexos = new();

    private Atendimento() { }

    public Atendimento(
        Guid sacerdoteId,
        Guid consulenteId,
        DateTime dataConsulta,
        string tipoOraculo,
        string perguntaCentral,
        string veredictoEspiritual,
        Guid? agendamentoId = null,
        string? status = "Realizado",
        string? observacoes = null)
    {
        if (sacerdoteId == Guid.Empty)
            throw new ArgumentException("O Sacerdote responsável deve ser informado.");

        if (consulenteId == Guid.Empty)
            throw new ArgumentException("O Consulente deve ser informado.");

        if (string.IsNullOrWhiteSpace(tipoOraculo))
            throw new ArgumentException("O Tipo de Oráculo é obrigatório.");

        if (string.IsNullOrWhiteSpace(perguntaCentral) || perguntaCentral.Trim().Length < 5)
            throw new ArgumentException("A Pergunta Central / Dúvida Vital deve conter no mínimo 5 caracteres.");

        if (string.IsNullOrWhiteSpace(veredictoEspiritual) || veredictoEspiritual.Trim().Length < 10)
            throw new ArgumentException("O Veredicto Espiritual / Causa Raiz deve conter no mínimo 10 caracteres.");

        SacerdoteId = sacerdoteId;
        ConsulenteId = consulenteId;
        DataConsulta = dataConsulta;
        TipoOraculo = tipoOraculo.Trim();
        PerguntaCentral = perguntaCentral.Trim();
        VeredictoEspiritual = veredictoEspiritual.Trim();
        AgendamentoId = agendamentoId;
        Status = !string.IsNullOrWhiteSpace(status) ? status.Trim() : "Realizado";
        Observacoes = observacoes?.Trim() ?? string.Empty;
    }

    public Guid SacerdoteId { get; private set; }
    public virtual Sacerdote? Sacerdote { get; private set; }

    public Guid ConsulenteId { get; private set; }
    public virtual Lead? Consulente { get; private set; }

    public Guid? AgendamentoId { get; private set; }
    public virtual Agendamento? Agendamento { get; private set; }

    public DateTime DataConsulta { get; private set; }
    public string TipoOraculo { get; private set; } = null!;
    public string PerguntaCentral { get; private set; } = null!;
    public string VeredictoEspiritual { get; private set; } = null!;
    public string Status { get; private set; } = "Realizado";
    public string Observacoes { get; private set; } = string.Empty;

    public virtual IReadOnlyCollection<AnexoAtendimento> Anexos => _anexos.AsReadOnly();

    public void AddAnexo(AnexoAtendimento anexo)
    {
        if (anexo == null)
            throw new ArgumentNullException(nameof(anexo));

        _anexos.Add(anexo);
        RegisterUpdate();
    }

    public void RemoveAnexo(Guid anexoId)
    {
        var anexo = _anexos.FirstOrDefault(a => a.Id == anexoId);
        if (anexo != null)
        {
            _anexos.Remove(anexo);
            RegisterUpdate();
        }
    }

    public void UpdateDetalhes(
        string tipoOraculo,
        string perguntaCentral,
        string veredictoEspiritual,
        string status,
        string? observacoes)
    {
        if (string.IsNullOrWhiteSpace(tipoOraculo))
            throw new ArgumentException("O Tipo de Oráculo é obrigatório.");

        if (string.IsNullOrWhiteSpace(perguntaCentral) || perguntaCentral.Trim().Length < 5)
            throw new ArgumentException("A Pergunta Central / Dúvida Vital deve conter no mínimo 5 caracteres.");

        if (string.IsNullOrWhiteSpace(veredictoEspiritual) || veredictoEspiritual.Trim().Length < 10)
            throw new ArgumentException("O Veredicto Espiritual / Causa Raiz deve conter no mínimo 10 caracteres.");

        TipoOraculo = tipoOraculo.Trim();
        PerguntaCentral = perguntaCentral.Trim();
        VeredictoEspiritual = veredictoEspiritual.Trim();
        Status = !string.IsNullOrWhiteSpace(status) ? status.Trim() : Status;
        Observacoes = observacoes?.Trim() ?? string.Empty;
        RegisterUpdate();
    }

    public void UpdateStatus(string status)
    {
        if (!string.IsNullOrWhiteSpace(status))
        {
            Status = status.Trim();
            RegisterUpdate();
        }
    }
}
