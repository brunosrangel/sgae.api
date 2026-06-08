using System;
using System.Collections.Generic;
using Sgae.Domain.Common;
using Sgae.Domain.Enums;

namespace Sgae.Domain.Entities;

/// <summary>
/// Entidade representando o Consulente na etapa de Captação (Lead).
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
        OrigemContato origem, 
        string problemaPrincipal)
    {
        UpdateDadosPessoais(nome, email, telefone);
        UpdateLocalizacao(cidade, estado);
        Origem = origem;
        ProblemaPrincipal = problemaPrincipal ?? throw new ArgumentException("O problema principal deve ser especificado para captação.");
        DataContato = DateTime.UtcNow;
    }

    public string Nome { get; private set; } = null!;
    public string Telefone { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string Cidade { get; private set; } = null!;
    public string Estado { get; private set; } = null!;
    public DateTime DataContato { get; private set; }
    public OrigemContato Origem { get; private set; }
    public string ProblemaPrincipal { get; private set; } = null!;

    // Relacionamento de Navegação (1-para-N): Um Lead pode ter vários agendamentos no sistema
    public virtual ICollection<Agendamento> Agendamentos { get; private set; } = new List<Agendamento>();

    // Relacionamento de Navegação 1-para-1 com o Perfil do Consulente (Etapa 3 - Perfil)
    public virtual PerfilConsulente? Perfil { get; private set; }

    // Métodos de Negócio (Ricos / DDD)
    public void UpdateDadosPessoais(string nome, string email, string telefone)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome não pode ser vazio.");
        
        if (string.IsNullOrWhiteSpace(telefone))
            throw new ArgumentException("Telefone para contato é obrigatório.");

        Nome = nome.Trim();
        Email = email?.Trim() ?? string.Empty;
        Telefone = telefone.Trim();
        RegisterUpdate();
    }

    public void UpdateLocalizacao(string cidade, string estado)
    {
        if (string.IsNullOrWhiteSpace(cidade))
            throw new ArgumentException("Cidade é obrigatória.");
        
        if (string.IsNullOrWhiteSpace(estado) || estado.Length != 2)
            throw new ArgumentException("Estado é obrigatório e deve conter exatamente 2 caracteres (UF).");

        Cidade = cidade.Trim();
        Estado = estado.Trim().ToUpper();
        RegisterUpdate();
    }

    public void AlterarProblemaPrincipal(string novoProblema)
    {
        if (string.IsNullOrWhiteSpace(novoProblema))
            throw new ArgumentException("O problema principal não pode ser nulo.");
        
        ProblemaPrincipal = novoProblema.Trim();
        RegisterUpdate();
    }

    public void DefinirPerfil(PerfilConsulente perfil)
    {
        Perfil = perfil ?? throw new ArgumentNullException(nameof(perfil));
        RegisterUpdate();
    }
}