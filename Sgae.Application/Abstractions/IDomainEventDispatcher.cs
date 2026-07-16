namespace Sgae.Application.Abstractions;

/// <summary>
/// Provedor de despacho desacoplado de eventos de domínio mapeando-os e distribuindo-os para notificações do MediatR.
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Varre o rastreador de mudanças de estados (DbContext / Entidades Ricas) para distribuir eventos pendentes.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento da transação corporativa.</param>
    Task DispatchEventsAsync(CancellationToken cancellationToken = default);
}
