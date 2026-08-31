using System;
using Sgae.Application.Agendamentos.DTOs;
using Sgae.Application.Common.CQRS;

namespace Sgae.Application.Agendamentos.Queries.GetAgendamentoById;

public record GetAgendamentoByIdQuery(Guid Id) : IQuery<AgendamentoDto?>;
