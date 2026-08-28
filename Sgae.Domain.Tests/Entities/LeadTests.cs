using System;
using System.Collections.Generic;
using Sgae.Domain.Entities;
using Sgae.Domain.Enums;
using Xunit;

namespace Sgae.Domain.Tests.Entities;

public class LeadTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateLead()
    {
        // Arrange
        var nome = "Bruno Rangel";
        var telefone = "(21) 98888-7777";
        var email = "bruno@email.com";
        var cidade = "Rio de Janeiro";
        var estado = "RJ";
        var origem = OrigemContato.Instagram;
        var problemaPrincipal = "Busca de orientação sobre dilemas existenciais.";

        // Act
        var lead = new Lead(nome, telefone, email, cidade, estado, origem, problemaPrincipal);

        // Assert
        Assert.NotNull(lead);
        Assert.NotEqual(Guid.Empty, lead.Id);
        Assert.Equal(nome, lead.Nome);
        Assert.Equal(nome, lead.NomeCompleto);
        Assert.Equal(telefone, lead.Telefone);
        Assert.Equal(email, lead.Email);
        Assert.Equal(cidade, lead.Cidade);
        Assert.Equal(estado, lead.Estado);
        Assert.Equal(estado, lead.Uf);
        Assert.Equal(origem, lead.Origem);
        Assert.Equal(problemaPrincipal, lead.ProblemaPrincipal);
        Assert.False(lead.IsDeleted);
        Assert.True(lead.CreatedAt <= DateTime.UtcNow);
        Assert.NotEmpty(lead.Historico);
    }

    [Fact]
    public void Constructor_WithCompleteJsonParameters_ShouldPopulateAllFields()
    {
        // Arrange
        var customId = "1724851234567-abc12345";
        var nomeCompleto = "Maria Silva dos Santos";
        var telefone = "11987654321";
        var email = "maria.silva@exemplo.com";
        var dataNasc = new DateTime(1990, 4, 15, 0, 0, 0, DateTimeKind.Utc);
        var profissao = "Professora";
        var nacionalidade = "Brasileira";
        var naturalidade = "São Paulo - SP";
        var tradicao = "Candomblé – Angola, Efon, Ijexá, Jeje, Nagô-Ketu ou Nação Mista.";
        var vinculo = "Iniciado(a) / Feito(a) no Santo";
        var vinculoCcrias = "Sim";
        var temporalidade = "10 anos";
        var jogouBuzios = "Sim";
        var orixas = new List<string> { "Èṣù (Exu)", "Ògún (Ogum)", "Ọ̀ṣọ́ọ̀sì (Oxóssi)" };
        var cep = "01310-100";
        var endereco = "Avenida Paulista";
        var numero = "1000";
        var complemento = "Apto 42";
        var bairro = "Bela Vista";
        var cidade = "São Paulo";
        var uf = "SP";
        var obs = "Consulente encaminhado via indicação.";
        var status = "Novo";
        var prioridade = "Média";

        // Act
        var lead = new Lead(
            nome: nomeCompleto,
            telefone: telefone,
            email: email,
            cidade: cidade,
            estado: uf,
            origem: OrigemContato.Indicacao,
            problemaPrincipal: obs,
            customId: customId,
            dataNascimento: dataNasc,
            profissao: profissao,
            nacionalidade: nacionalidade,
            naturalidade: naturalidade,
            tradicaoTerreiro: tradicao,
            vinculoTradicoes: vinculo,
            vinculoCcrias: vinculoCcrias,
            temporalidade: temporalidade,
            jogouBuziosBabalorisaSidnei: jogouBuzios,
            orixasNagoKetu: orixas,
            cep: cep,
            endereco: endereco,
            numero: numero,
            complemento: complemento,
            bairro: bairro,
            observacoes: obs,
            status: status,
            prioridade: prioridade
        );

        // Assert
        Assert.NotNull(lead);
        Assert.Equal(customId, lead.CustomId);
        Assert.Equal(nomeCompleto, lead.NomeCompleto);
        Assert.Equal(profissao, lead.Profissao);
        Assert.Equal(tradicao, lead.TradicaoTerreiro);
        Assert.Equal(vinculo, lead.VinculoTradicoes);
        Assert.Equal(3, lead.OrixasNagoKetu.Count);
        Assert.Contains("Èṣù (Exu)", lead.OrixasNagoKetu);
        Assert.Equal(cep, lead.Cep);
        Assert.Equal(endereco, lead.Endereco);
        Assert.Equal(status, lead.Status);
        Assert.Equal(prioridade, lead.Prioridade);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithEmptyOrNullNome_ShouldThrowArgumentException(string invalidNome)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Lead(
            invalidNome,
            "(21) 98888-7777",
            "bruno@email.com",
            "Rio de Janeiro",
            "RJ",
            OrigemContato.Instagram,
            "Problema principal"
        ));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithEmptyOrNullTelefone_ShouldThrowArgumentException(string invalidTelefone)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Lead(
            "Bruno Rangel",
            invalidTelefone,
            "bruno@email.com",
            "Rio de Janeiro",
            "RJ",
            OrigemContato.Instagram,
            "Problema principal"
        ));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithEmptyOrNullCidade_ShouldThrowArgumentException(string invalidCidade)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Lead(
            "Bruno Rangel",
            "(21) 98888-7777",
            "bruno@email.com",
            invalidCidade,
            "RJ",
            OrigemContato.Instagram,
            "Problema principal"
        ));
    }

    [Theory]
    [InlineData("")]
    [InlineData("R")]
    [InlineData("RJO")]
    [InlineData(null)]
    public void Constructor_WithInvalidEstadoLength_ShouldThrowArgumentException(string invalidEstado)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Lead(
            "Bruno Rangel",
            "(21) 98888-7777",
            "bruno@email.com",
            "Rio de Janeiro",
            invalidEstado,
            OrigemContato.Instagram,
            "Problema principal"
        ));
    }

    [Fact]
    public void Delete_ShouldMarkAsDeletedAndRegisterUpdate()
    {
        // Arrange
        var lead = new Lead("Bruno", "(21) 98888-7777", "bruno@email.com", "Rio", "RJ", OrigemContato.Instagram, "Problema");

        // Act
        lead.Delete();

        // Assert
        Assert.True(lead.IsDeleted);
        Assert.NotNull(lead.UpdatedAt);
    }
}
