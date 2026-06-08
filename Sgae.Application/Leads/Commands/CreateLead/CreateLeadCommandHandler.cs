using System;
using System.Threading;
using System.Threading.Tasks;
using Sgae.Application.Abstractions;
using Sgae.Application.Common.CQRS;
using Sgae.Domain.Entities;

namespace Sgae.Application.Leads.Commands.CreateLead;

/// <summary>
/// Manipulador que recebe o comando, cria a entidade rica e persiste via Unit of Work.
/// </summary>
public class CreateLeadCommandHandler : ICommandHandler<CreateLeadCommand, Guid>
{
    private readonly IAppDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public CreateLeadCommandHandler(IAppDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateLeadCommand request, CancellationToken cancellationToken)
    {
        // Instanciação rica da entidade de domínio (validará as exigências e invariantes internamente)
        var lead = new Lead(
            request.Nome,
            request.Telefone,
            request.Email,
            request.Cidade,
            request.Estado,
            request.Origem,
            request.ProblemaPrincipal
        );

        await _context.Leads.AddAsync(lead, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return lead.Id;
    }
}