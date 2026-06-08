using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using MediatR;
using Sgae.Application.Common.Behaviors;

namespace Sgae.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Registra as dependências internas da camada Sgae.Application no contêiner de DI do .NET 8.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Registra o AutoMapper escaneando os profiles de mapeamento do Assembly
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // Registra todos os validadores do FluentValidation no Assembly atual
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Registra o MediatR escaneando o Assembly atual da aplicação
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

            // Acopla os Behaviors na Pipeline de execução do MediatR
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            config.AddOpenBehavior(typeof(FluentValidationBehavior<,>));
        });

        return services;
    }
}