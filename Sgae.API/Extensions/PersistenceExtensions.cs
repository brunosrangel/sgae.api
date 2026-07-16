using Microsoft.EntityFrameworkCore;
using Sgae.Application.Abstractions;
using Sgae.Domain.Repositories;
using Sgae.Infrastructure.Persistence;
using Sgae.Infrastructure.Persistence.Repositories;

namespace Sgae.API.Extensions;

/// <summary>
/// Extensões responsáveis pelo registro da camada de Infrastructure:
/// DbContext, repositórios, Unit of Work e cache distribuído (Redis com fallback em memória).
/// </summary>
public static class PersistenceExtensions
{
    public static IServiceCollection AddSgaePersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
                   .AddInterceptors(new DateTimeUtcInterceptor()));

        services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
        services.AddScoped<DatabaseSeeder>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ILeadRepository, LeadRepository>();
        services.AddScoped<IPerfilConsulenteRepository, PerfilConsulenteRepository>();
        services.AddScoped<IAgendamentoRepository, AgendamentoRepository>();
        services.AddScoped<IAtendimentoEspiritualRepository, AtendimentoEspiritualRepository>();
        services.AddScoped<IAcompanhamentoRepository, AcompanhamentoRepository>();
        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        return services;
    }

    /// <summary>
    /// Configura o cache distribuído com Redis. Caso a connection string não esteja
    /// definida (ex.: ambientes de dev/test sem infraestrutura Redis), usa cache em memória.
    /// </summary>
    public static IServiceCollection AddSgaeCaching(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = configuration["REDIS_CONNECTION_STRING"]
            ?? Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")
            ?? configuration.GetConnectionString("Redis");

        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "Sgae_";
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        return services;
    }
}
