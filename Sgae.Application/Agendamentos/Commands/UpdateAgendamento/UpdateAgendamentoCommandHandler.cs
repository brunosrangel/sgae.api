using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sgae.Application.Abstractions;
using Sgae.Application.Common.CQRS;
using Sgae.Domain.Entities;
using Sgae.Domain.Enums;
using Sgae.Domain.Repositories;

namespace Sgae.Application.Agendamentos.Commands.UpdateAgendamento;

public class UpdateAgendamentoCommandHandler : ICommandHandler<UpdateAgendamentoCommand, bool>
{
    private readonly IAgendamentoRepository _agendamentoRepository;
    private readonly ILeadRepository _leadRepository;
    private readonly IAppDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAgendamentoCommandHandler(
        IAgendamentoRepository agendamentoRepository,
        ILeadRepository leadRepository,
        IAppDbContext context,
        IUnitOfWork unitOfWork)
    {
        _agendamentoRepository = agendamentoRepository;
        _leadRepository = leadRepository;
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpdateAgendamentoCommand request, CancellationToken cancellationToken)
    {
        var agendamento = await _context.Agendamentos
            .Include(a => a.Lead)
            .Include(a => a.Sacerdote)
            .Include(a => a.ServicoConsulta)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (agendamento == null)
        {
            throw new ArgumentException($"Agendamento com ID '{request.Id}' não foi localizado no sistema.");
        }

        // 1. Atualização do Lead / Consulente se fornecido
        if (request.LeadId.HasValue && request.LeadId.Value != Guid.Empty && request.LeadId.Value != agendamento.LeadId)
        {
            var leadExists = await _leadRepository.GetByIdAsync(request.LeadId.Value, cancellationToken);
            if (leadExists == null)
            {
                throw new ArgumentException($"O consulente de ID '{request.LeadId.Value}' não foi localizado no sistema.");
            }
            typeof(Agendamento).GetProperty("LeadId")?.SetValue(agendamento, request.LeadId.Value);
        }

        if (agendamento.Lead != null)
        {
            if (!string.IsNullOrWhiteSpace(request.LeadNome))
            {
                typeof(Lead).GetProperty("Nome")?.SetValue(agendamento.Lead, request.LeadNome.Trim());
            }
            if (!string.IsNullOrWhiteSpace(request.LeadTelefone))
            {
                typeof(Lead).GetProperty("Telefone")?.SetValue(agendamento.Lead, request.LeadTelefone.Trim());
            }
        }

        // 2. Atualização de Data e Horário
        if (!string.IsNullOrWhiteSpace(request.Data))
        {
            var novaDataUtc = Sgae.Domain.Common.DateTimeHelper.ParseUtc(request.Data, request.Horario);
            agendamento.Reagendar(novaDataUtc);
        }
        else if (request.DataHora.HasValue && request.DataHora.Value != default)
        {
            var novaDataUtc = Sgae.Domain.Common.DateTimeHelper.EnsureUtc(request.DataHora.Value);
            agendamento.Reagendar(novaDataUtc);
        }

        // 3. Atualização de Modalidade e Valor
        if (request.Modalidade.HasValue)
        {
            typeof(Agendamento).GetProperty("Modalidade")?.SetValue(agendamento, request.Modalidade.Value);
        }

        if (request.Valor.HasValue && request.Valor.Value >= 0)
        {
            typeof(Agendamento).GetProperty("Valor")?.SetValue(agendamento, request.Valor.Value);
        }

        // 4. Atualização de Sacerdote
        if (request.SacerdoteId.HasValue && request.SacerdoteId.Value != Guid.Empty)
        {
            agendamento.DefinirSacerdote(request.SacerdoteId.Value);
        }
        else if (!string.IsNullOrWhiteSpace(request.Sacerdote))
        {
            var sacerdoteNome = request.Sacerdote.Trim();
            var allSacerdotes = await _context.Sacerdotes.Where(s => !s.IsDeleted).ToListAsync(cancellationToken);
            
            var sacerdote = allSacerdotes.FirstOrDefault(s => 
                string.Equals(s.Nome, sacerdoteNome, StringComparison.OrdinalIgnoreCase))
                ?? allSacerdotes.FirstOrDefault(s => 
                    s.Nome.Contains("Sidnei", StringComparison.OrdinalIgnoreCase) && sacerdoteNome.Contains("Sidnei", StringComparison.OrdinalIgnoreCase))
                ?? allSacerdotes.FirstOrDefault(s =>
                    s.Nome.IndexOf(sacerdoteNome, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    sacerdoteNome.IndexOf(s.Nome, StringComparison.OrdinalIgnoreCase) >= 0);

            if (sacerdote != null)
            {
                agendamento.DefinirSacerdote(sacerdote.Id);
            }
            else
            {
                var novoSacerdote = new Sacerdote(sacerdoteNome, "Sacerdote Responsável", true);
                await _context.Sacerdotes.AddAsync(novoSacerdote, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                agendamento.DefinirSacerdote(novoSacerdote.Id);
            }
        }

        // 5. Atualização de ServicoConsulta
        if (request.ServicoConsultaId.HasValue && request.ServicoConsultaId.Value != Guid.Empty)
        {
            agendamento.DefinirServicoConsulta(request.ServicoConsultaId.Value);
        }
        else if (!string.IsNullOrWhiteSpace(request.TipoConsulta))
        {
            var tipoConsultaNome = request.TipoConsulta.Trim();
            var allServicos = await _context.ServicosConsulta.Where(s => !s.IsDeleted).ToListAsync(cancellationToken);
            
            var servico = allServicos.FirstOrDefault(s => 
                string.Equals(s.Nome, tipoConsultaNome, StringComparison.OrdinalIgnoreCase))
                ?? allServicos.FirstOrDefault(s => 
                    (s.Nome.Contains("Búzios", StringComparison.OrdinalIgnoreCase) || s.Nome.Contains("Buzios", StringComparison.OrdinalIgnoreCase)) &&
                    (tipoConsultaNome.Contains("Búzios", StringComparison.OrdinalIgnoreCase) || tipoConsultaNome.Contains("Buzios", StringComparison.OrdinalIgnoreCase)))
                ?? allServicos.FirstOrDefault(s =>
                    s.Nome.IndexOf(tipoConsultaNome, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    tipoConsultaNome.IndexOf(s.Nome, StringComparison.OrdinalIgnoreCase) >= 0);

            if (servico != null)
            {
                agendamento.DefinirServicoConsulta(servico.Id);
            }
            else
            {
                var novoServico = new ServicoConsulta(tipoConsultaNome, request.Valor ?? 250m, true);
                await _context.ServicosConsulta.AddAsync(novoServico, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                agendamento.DefinirServicoConsulta(novoServico.Id);
            }
        }

        // 6. Atualização de Detalhes Operacionais
        var formaPagamento = request.FormaPagamento ?? agendamento.FormaPagamento;
        var pago = request.Pago ?? agendamento.Pago;
        var observacoes = request.Observacoes ?? agendamento.Observacoes;
        var whatsappConfirmacao = request.WhatsappConfirmacaoDisparada ?? agendamento.WhatsappConfirmacaoDisparada;
        var configLembrete = request.ConfigLembrete ?? agendamento.ConfigLembrete;
        var frequenciaLembrete = request.FrequenciaLembrete ?? agendamento.FrequenciaLembrete;

        agendamento.AtualizarDetalhes(
            formaPagamento,
            pago,
            observacoes,
            whatsappConfirmacao,
            configLembrete,
            frequenciaLembrete
        );

        // 7. Atualização de Status
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (Enum.TryParse<StatusAgendamento>(request.Status, true, out var parsedStatus))
            {
                agendamento.DefinirStatus(parsedStatus);
            }
        }

        if (!string.IsNullOrWhiteSpace(request.MotivoCancelamento))
        {
            agendamento.CancelarAgendamento(request.MotivoCancelamento);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
