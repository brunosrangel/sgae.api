using Sgae.Application.Abstractions;
using Sgae.Application.Common.CQRS;
using Sgae.Domain.Repositories;

namespace Sgae.Application.Leads.Commands.UpdateLead;

/// <summary>
/// Manipulador que processa a atualização do Consulente (Lead).
/// </summary>
public class UpdateLeadCommandHandler : ICommandHandler<UpdateLeadCommand>
{
    private readonly ILeadRepository _leadRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateLeadCommandHandler(ILeadRepository leadRepository, IUnitOfWork unitOfWork)
    {
        _leadRepository = leadRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateLeadCommand request, CancellationToken cancellationToken)
    {
        var lead = await _leadRepository.GetByIdAsync(request.Id, cancellationToken);
        if (lead == null)
        {
            throw new ArgumentException($"Consulente de ID '{request.Id}' não localizado.");
        }

        var nome = request.GetNomeEfetivo();
        var cidade = request.GetCidadeEfetiva();
        var estado = request.GetEstadoEfetivo();

        lead.UpdateDadosPessoais(
            nome, 
            request.Email, 
            request.Telefone, 
            request.DataNascimento, 
            request.Profissao, 
            request.Nacionalidade, 
            request.Naturalidade
        );

        lead.UpdateLocalizacao(
            cidade, 
            estado, 
            request.Cep, 
            request.Endereco, 
            request.Numero, 
            request.Complemento, 
            request.Bairro
        );

        lead.UpdateTradicao(
            request.TradicaoTerreiro, 
            request.VinculoTradicoes, 
            request.VinculoCcrias, 
            request.Temporalidade, 
            request.JogouBuziosBabalorisaSidnei, 
            request.OrixasNagoKetu
        );

        lead.UpdateStatusPrioridade(request.Status, request.Prioridade, request.Observacoes);

        if (!string.IsNullOrWhiteSpace(request.ProblemaPrincipal))
        {
            lead.AlterarProblemaPrincipal(request.ProblemaPrincipal);
        }

        if (request.CanalCaptacaoId.HasValue)
        {
            lead.DefinirCanalCaptacao(request.CanalCaptacaoId);
        }

        lead.AdicionarHistorico("Atualização", "Dados do lead atualizados");

        _leadRepository.Update(lead);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
