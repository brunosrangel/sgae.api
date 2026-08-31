using System;
using Sgae.Application.Common.CQRS;

namespace Sgae.Application.Agendamentos.Commands.DeleteAgendamento;

public class DeleteAgendamentoCommand : ICommand<bool>
{
    public Guid Id { get; set; }

    public DeleteAgendamentoCommand(Guid id)
    {
        Id = id;
    }
}
