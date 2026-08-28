using System;
using System.Collections.Generic;
using System.Linq;
using Sgae.Domain.Common;
using Sgae.Domain.Enums;

namespace Sgae.Domain.Entities;

/// <summary>
/// Entidade representando o Consulente na etapa de Captação (Lead).
/// Suporta dados pessoais, localização, vínculos e tradições de terreiro, orixás e histórico.
/// </summary>
public class Lead : BaseEntity
{
    // Construtor privado para o EF Core
    private Lead() { }

    public Lead(
        string nome, 
        string telefone, 
        string email, 
        string cidade, 
        string estado, 
        OrigemContato origem = OrigemContato.Indicacao, 
        string? problemaPrincipal = null,
        string? customId = null,
        DateTime? dataNascimento = null,
        string? profissao = null,
        string? nacionalidade = null,
        string? naturalidade = null,
        string? tradicaoTerreiro = null,
        string? vinculoTradicoes = null,
        string? vinculoCcrias = null,
        string? temporalidade = null,
        string? jogouBuziosBabalorisaSidnei = null,
        IEnumerable<string>? orixasNagoKetu = null,
        string? cep = null,
        string? endereco = null,
        string? numero = null,
        string? complemento = null,
        string? bairro = null,
        string? observacoes = null,
        string? status = "Novo",
        string? prioridade = "Média",
        DateTime? dataCadastro = null,
        Guid? canalCaptacaoId = null)
    {
        CustomId = customId?.Trim();
        UpdateDadosPessoais(nome, email, telefone, dataNascimento, profissao, nacionalidade, naturalidade);
        UpdateLocalizacao(cidade, estado, cep, endereco, numero, complemento, bairro);
        UpdateTradicao(tradicaoTerreiro, vinculoTradicoes, vinculoCcrias, temporalidade, jogouBuziosBabalorisaSidnei, orixasNagoKetu);
        
        Origem = origem;
        Observacoes = observacoes?.Trim();
        ProblemaPrincipal = !string.IsNullOrWhiteSpace(problemaPrincipal) 
            ? problemaPrincipal.Trim() 
            : (!string.IsNullOrWhiteSpace(observacoes) ? observacoes.Trim() : "Captação de Lead");

        Status = !string.IsNullOrWhiteSpace(status) ? status.Trim() : "Novo";
        Prioridade = !string.IsNullOrWhiteSpace(prioridade) ? prioridade.Trim() : "Média";
        DataContato = dataCadastro ?? DateTime.UtcNow;
        CanalCaptacaoId = canalCaptacaoId;

        // Adiciona registro inicial no histórico
        AdicionarHistorico("Criação", "Lead cadastrado no sistema", DataContato);
    }

    public string? CustomId { get; private set; }
    public string Nome { get; private set; } = null!;
    public string NomeCompleto => Nome;
    public string Telefone { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public DateTime? DataNascimento { get; private set; }
    public string? Profissao { get; private set; }
    public string? Nacionalidade { get; private set; }
    public string? Naturalidade { get; private set; }

    // Vínculos & Tradições
    public string? TradicaoTerreiro { get; private set; }
    public string? VinculoTradicoes { get; private set; }
    public string? VinculoCcrias { get; private set; }
    public string? Temporalidade { get; private set; }
    public string? JogouBuziosBabalorisaSidnei { get; private set; }
    public List<string> OrixasNagoKetu { get; private set; } = new();

    // Endereço / Localização
    public string? Cep { get; private set; }
    public string? Endereco { get; private set; }
    public string? Numero { get; private set; }
    public string? Complemento { get; private set; }
    public string? Bairro { get; private set; }
    public string Cidade { get; private set; } = null!;
    public string Estado { get; private set; } = null!;
    public string Uf => Estado;

    public DateTime DataContato { get; private set; }
    public DateTime DataCadastro => DataContato;
    public OrigemContato Origem { get; private set; }
    public string ProblemaPrincipal { get; private set; } = null!;
    public string? Observacoes { get; private set; }
    public string Status { get; private set; } = "Novo";
    public string Prioridade { get; private set; } = "Média";

    public Guid? CanalCaptacaoId { get; private set; }
    public virtual CanalCaptacao? CanalCaptacao { get; private set; }

    // Histórico do Lead
    public virtual ICollection<LeadHistorico> Historico { get; private set; } = new List<LeadHistorico>();

    // Relacionamento de Navegação (1-para-N): Um Lead pode ter vários agendamentos no sistema
    public virtual ICollection<Agendamento> Agendamentos { get; private set; } = new List<Agendamento>();

    // Relacionamento de Navegação 1-para-1 com o Perfil do Consulente (Etapa 3 - Perfil)
    public virtual PerfilConsulente? Perfil { get; private set; }

    // Métodos de Negócio (Ricos / DDD)
    public void UpdateDadosPessoais(
        string nome, 
        string email, 
        string telefone,
        DateTime? dataNascimento = null,
        string? profissao = null,
        string? nacionalidade = null,
        string? naturalidade = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome não pode ser vazio.");
        
        if (string.IsNullOrWhiteSpace(telefone))
            throw new ArgumentException("Telefone para contato é obrigatório.");

        Nome = nome.Trim();
        Email = email?.Trim() ?? string.Empty;
        Telefone = telefone.Trim();
        DataNascimento = dataNascimento;
        Profissao = profissao?.Trim();
        Nacionalidade = nacionalidade?.Trim();
        Naturalidade = naturalidade?.Trim();
        RegisterUpdate();
    }

    public void UpdateLocalizacao(
        string cidade, 
        string estado,
        string? cep = null,
        string? endereco = null,
        string? numero = null,
        string? complemento = null,
        string? bairro = null)
    {
        if (string.IsNullOrWhiteSpace(cidade))
            throw new ArgumentException("Cidade é obrigatória.");
        
        if (string.IsNullOrWhiteSpace(estado) || estado.Length != 2)
            throw new ArgumentException("Estado é obrigatório e deve conter exatamente 2 caracteres (UF).");

        Cidade = cidade.Trim();
        Estado = estado.Trim().ToUpper();
        Cep = cep?.Trim();
        Endereco = endereco?.Trim();
        Numero = numero?.Trim();
        Complemento = complemento?.Trim();
        Bairro = bairro?.Trim();
        RegisterUpdate();
    }

    public void UpdateTradicao(
        string? tradicaoTerreiro,
        string? vinculoTradicoes,
        string? vinculoCcrias,
        string? temporalidade,
        string? jogouBuziosBabalorisaSidnei,
        IEnumerable<string>? orixasNagoKetu)
    {
        TradicaoTerreiro = tradicaoTerreiro?.Trim();
        VinculoTradicoes = vinculoTradicoes?.Trim();
        VinculoCcrias = vinculoCcrias?.Trim();
        Temporalidade = temporalidade?.Trim();
        JogouBuziosBabalorisaSidnei = jogouBuziosBabalorisaSidnei?.Trim();
        OrixasNagoKetu = orixasNagoKetu != null ? orixasNagoKetu.Where(o => !string.IsNullOrWhiteSpace(o)).Select(o => o.Trim()).ToList() : new List<string>();
        RegisterUpdate();
    }

    public void UpdateStatusPrioridade(string? status, string? prioridade, string? observacoes)
    {
        if (!string.IsNullOrWhiteSpace(status))
            Status = status.Trim();

        if (!string.IsNullOrWhiteSpace(prioridade))
            Prioridade = prioridade.Trim();

        if (observacoes != null)
            Observacoes = observacoes.Trim();

        RegisterUpdate();
    }

    public void AlterarProblemaPrincipal(string novoProblema)
    {
        if (string.IsNullOrWhiteSpace(novoProblema))
            throw new ArgumentException("O problema principal não pode ser nulo.");
        
        ProblemaPrincipal = novoProblema.Trim();
        RegisterUpdate();
    }

    public void AdicionarHistorico(string tipo, string descricao, DateTime? data = null)
    {
        var historicoItem = new LeadHistorico(Id, tipo, descricao, data);
        Historico.Add(historicoItem);
        RegisterUpdate();
    }

    public void DefinirPerfil(PerfilConsulente perfil)
    {
        Perfil = perfil ?? throw new ArgumentNullException(nameof(perfil));
        RegisterUpdate();
    }

    public void DefinirCanalCaptacao(Guid? canalCaptacaoId)
    {
        CanalCaptacaoId = canalCaptacaoId;
        RegisterUpdate();
    }

    public void DefinirCodigoExterno(string? customId)
    {
        CustomId = customId?.Trim();
        RegisterUpdate();
    }
}
