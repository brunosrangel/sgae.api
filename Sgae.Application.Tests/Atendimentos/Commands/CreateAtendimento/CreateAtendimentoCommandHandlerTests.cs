using FluentAssertions;
using Moq;
using Sgae.Application.Atendimentos.Commands.CreateAtendimento;
using Sgae.Application.Tests.Common;
using Sgae.Domain.Entities;
using Sgae.Domain.Enums;
using Sgae.Domain.Repositories;
using Xunit;

namespace Sgae.Application.Tests.Atendimentos.Commands.CreateAtendimento;

public class CreateAtendimentoCommandHandlerTests : HandlerTestBase
{
    private readonly Mock<IAtendimentoEspiritualRepository> _mockAtendimentoRepo;
    private readonly Mock<IAgendamentoRepository> _mockAgendamentoRepo;
    private readonly CreateAtendimentoCommandHandler _handler;

    public CreateAtendimentoCommandHandlerTests()
    {
        _mockAtendimentoRepo = CreateMock<IAtendimentoEspiritualRepository>();
        _mockAgendamentoRepo = CreateMock<IAgendamentoRepository>();

        _handler = new CreateAtendimentoCommandHandler(
            _mockAtendimentoRepo.Object,
            _mockAgendamentoRepo.Object,
            MockUnitOfWork.Object,
            CreateLogger<CreateAtendimentoCommandHandler>(),
            MockCache.Object
        );
    }

    [Fact]
    public async Task Handle_WithNonExistentAgendamento_ShouldThrowArgumentException()
    {
        // Arrange
        var command = new CreateAtendimentoCommand(
            Guid.NewGuid(),
            TipoAtendimento.AssistenciaFraterna,
            30,
            "Tratamento de desobsessão",
            "Observação pastoral."
        );

        _mockAgendamentoRepo
            .Setup(r => r.GetByIdAsync(command.AgendamentoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Agendamento?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage($"O agendamento de ID '{command.AgendamentoId}' não foi localizado no sistema.");

        _mockAtendimentoRepo.Verify(r => r.AddAsync(It.IsAny<AtendimentoEspiritual>(), It.IsAny<CancellationToken>()), Times.Never);
        MockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithExistingAtendimentoForAgendamento_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var agendamentoId = Guid.NewGuid();
        var leadId = Guid.NewGuid();

        // Configura uma data e hora futura válida para passar na regra de negócios da entidade Agendamento
        var agendamento = new Agendamento(leadId, DateTime.UtcNow.AddDays(2), ModalidadeAtendimento.Presencial, 0);

        var existingAtendimento = new AtendimentoEspiritual(agendamentoId, TipoAtendimento.Doutrinacao, 45, "Temas iniciais", null);

        var command = new CreateAtendimentoCommand(
            agendamentoId,
            TipoAtendimento.PasseEspiritual,
            15,
            "Passe de harmonização",
            "Obs"
        );

        _mockAgendamentoRepo
            .Setup(r => r.GetByIdAsync(command.AgendamentoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(agendamento);

        _mockAtendimentoRepo
            .Setup(r => r.GetByAgendamentoIdAsync(command.AgendamentoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAtendimento);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Já existe um Atendimento Espiritual registrado para este agendamento.");

        _mockAtendimentoRepo.Verify(r => r.AddAsync(It.IsAny<AtendimentoEspiritual>(), It.IsAny<CancellationToken>()), Times.Never);
        MockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithValidCommandAndStatusConfirmado_ShouldCreateAtendimentoAndMarkAgendamentoRealizado()
    {
        // Arrange
        var leadId = Guid.NewGuid();
        var agendamento = new Agendamento(leadId, DateTime.UtcNow.AddDays(1), ModalidadeAtendimento.Presencial, 0);

        // Garante que o status inicial do agendamento esteja em "Confirmado"
        agendamento.ConfirmarAgendamento();
        agendamento.Status.Should().Be(StatusAgendamento.Confirmado);

        var command = new CreateAtendimentoCommand(
            agendamento.Id,
            TipoAtendimento.TratamentoEspiritual,
            40,
            "Passes e aconselhamento",
            "Sessão transcorreu de forma harmônica."
        );

        _mockAgendamentoRepo
            .Setup(r => r.GetByIdAsync(command.AgendamentoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(agendamento);

        _mockAtendimentoRepo
            .Setup(r => r.GetByAgendamentoIdAsync(command.AgendamentoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AtendimentoEspiritual?)null);

        // Act
        var resultId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultId.Should().NotBeEmpty();
        agendamento.Status.Should().Be(StatusAgendamento.Realizado);

        _mockAgendamentoRepo.Verify(r => r.Update(agendamento), Times.Once);
        _mockAtendimentoRepo.Verify(r => r.AddAsync(It.IsAny<AtendimentoEspiritual>(), It.IsAny<CancellationToken>()), Times.Once);
        MockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        // Verifica se houve invalidação apropriada do cache distribuído de listagem de atendimentos
        MockCache.Verify(c => c.RemoveAsync("atendimentos_all", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidCommandAndStatusPendente_ShouldTransitionToConfirmadoThenRealizado()
    {
        // Arrange
        var leadId = Guid.NewGuid();
        var agendamento = new Agendamento(leadId, DateTime.UtcNow.AddDays(15), ModalidadeAtendimento.Online, 100);

        // Status inicial de fábrica em Pendente
        agendamento.Status.Should().Be(StatusAgendamento.Pendente);

        var command = new CreateAtendimentoCommand(
            agendamento.Id,
            TipoAtendimento.Desobsessao,
            50,
            "Percepções mediúnicas intensas",
            "Recomendada nova consulta em 15 dias."
        );

        _mockAgendamentoRepo
            .Setup(r => r.GetByIdAsync(command.AgendamentoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(agendamento);

        _mockAtendimentoRepo
            .Setup(r => r.GetByAgendamentoIdAsync(command.AgendamentoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AtendimentoEspiritual?)null);

        // Act
        var resultId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultId.Should().NotBeEmpty();
        agendamento.Status.Should().Be(StatusAgendamento.Realizado);

        _mockAgendamentoRepo.Verify(r => r.Update(agendamento), Times.Once);
        _mockAtendimentoRepo.Verify(r => r.AddAsync(It.IsAny<AtendimentoEspiritual>(), It.IsAny<CancellationToken>()), Times.Once);
        MockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        MockCache.Verify(c => c.RemoveAsync("atendimentos_all", It.IsAny<CancellationToken>()), Times.Once);
    }
}
