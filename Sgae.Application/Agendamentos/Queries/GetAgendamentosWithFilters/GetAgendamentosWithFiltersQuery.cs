using System;
using System.Collections.Generic;
using Sgae.Application.Common.CQRS;
using Sgae.Application.Agendamentos.DTOs;
using Sgae.Domain.Enums;

namespace Sgae.Application.Agendamentos.Queries.GetAgendamentosWithFilters;

public record GetAgendamentosWithFiltersQuery : IQuery<List<AgendamentoDto>>
{
    public DateTime? DataInicio { get; init; }
    public DateTime? DataFim { get; init; }
    public ModalidadeAtendimento? Modalidade { get; init; }
    public StatusAgendamento? Status { get; init; }
}