using Sgae.Application.Abstractions;
using Sgae.Infrastructure.Persistence;

namespace Sgae.API.Extensions;

/// <summary>
/// Extensão responsável pela inicialização e seeding automático do banco de dados relacional e serviços de identidade.
/// </summary>
public static class DatabaseSeedingExtensions
{
    public static async Task SeedSgaeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var seeder = services.GetRequiredService<DatabaseSeeder>();
            await seeder.InitializeAndSeedAsync();

            var identitySeeder = services.GetRequiredService<IIdentitySeeder>();
            await identitySeeder.SeedIdentityAsync();
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<DatabaseSeeder>>();
            logger.LogCritical(ex, "SGAE Core: Falha estrutural crítica ao executar a inicialização e o seeding automático do banco de dados.");
        }
    }
}
