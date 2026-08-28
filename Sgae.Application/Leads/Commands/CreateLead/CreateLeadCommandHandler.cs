using Sgae.Application.Abstractions;
using Sgae.Application.Common.CQRS;
using Sgae.Domain.Entities;
using Sgae.Domain.Enums;
using Sgae.Domain.Repositories;

namespace Sgae.Application.Leads.Commands.CreateLead;

/// <summary>
/// Manipulador que recebe o comando, cria a entidade rica e persiste via repositório e Unit of Work.
/// </summary>
public class CreateLeadCommandHandler : ICommandHandler<CreateLeadCommand, Guid>
{
    private readonly ILeadRepository _leadRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateLeadCommandHandler(ILeadRepository leadRepository, IUnitOfWork unitOfWork)
    {
        _leadRepository = leadRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateLeadCommand request, CancellationToken cancellationToken)
    {
        var nome = request.GetNomeEfetivo();
        var cidade = request.GetCidadeEfetiva();
        var estado = request.GetEstadoEfetivo();
        var problemaPrincipal = request.GetProblemaPrincipalEfetivo();
        var origem = request.Origem ?? OrigemContato.Indicacao;
        var dataCadastro = request.DataCadastro ?? request.DataContato ?? DateTime.UtcNow;

        var lead = new Lead(
            nome: nome,
            telefone: request.Telefone,
            email: request.Email,
            cidade: cidade,
            estado: estado,
            origem: origem,
            problemaPrincipal: problemaPrincipal,
            customId: request.Id,
            dataNascimento: request.DataNascimento,
            profissao: request.Profissao,
            nacionalidade: request.Nacionalidade,
            naturalidade: request.Naturalidade,
            tradicaoTerreiro: request.TradicaoTerreiro,
            vinculoTradicoes: request.VinculoTradicoes,
            vinculoCcrias: request.VinculoCcrias,
            temporalidade: request.Temporalidade,
            jogouBuziosBabalorisaSidnei: request.JogouBuziosBabalorisaSidnei,
            orixasNagoKetu: request.OrixasNagoKetu,
            cep: request.Cep,
            endereco: request.Endereco,
            numero: request.Numero,
            complemento: request.Complemento,
            bairro: request.Bairro,
            observacoes: request.Observacoes,
            status: request.Status ?? "Novo",
            prioridade: request.Prioridade ?? "Média",
            dataCadastro: dataCadastro,
            canalCaptacaoId: request.CanalCaptacaoId
        );

        // Se veio histórico adicional especificado no comando
        if (request.Historico != null && request.Historico.Count > 0)
        {
            foreach (var h in request.Historico)
            {
                if (!string.IsNullOrWhiteSpace(h.Descricao))
                {
                    lead.AdicionarHistorico(h.Tipo ?? "Criação", h.Descricao, h.Data ?? dataCadastro);
                }
            }
        }

        await _leadRepository.AddAsync(lead, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return lead.Id;
    }
}
