using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Sgae.API.Extensions;

/// <summary>
/// Extensões responsáveis pela configuração do Swagger/OpenAPI,
/// incluindo agrupamento de rotas por módulo funcional e autenticação Bearer.
/// </summary>
public static class ApiDocumentationExtensions
{
    public static IServiceCollection AddSgaeSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "SGAE API - Sistema de Gestão de Atendimento Especializado",
                Description = "API RESTful corporativa para gestão da jornada do consulente, estruturada sobre os princípios de Clean Architecture e DDD.\n\n" +
                              "### 📌 Módulos Funcionais da API:\n" +
                              "- **Módulo 1: Captação (Leads)** - Registro inicial e controle de captação de consulentes.\n" +
                              "- **Módulo 2: Agendamentos** - Gestão do fluxo de datas, status e valores de consultas.\n" +
                              "- **Módulo 3: Perfil do Consulente** - Enriquecimento sócio-demográfico e qualificação personalizada.\n" +
                              "- **Módulo 4: Atendimento Espiritual e Acompanhamento** - Registro de atendimentos e monitoramento continuado de evolução.\n\n" +
                              "*Padrões adotados: CQRS, Mediator Pipeline Behaviors, FluentValidation e Soft-Delete.*",
                Contact = new OpenApiContact
                {
                    Name = "Equipe de Arquitetura e Engenharia SGAE",
                    Email = "arquitetura@sgae.com.br"
                }
            });

            // Agrupamento lógico e ordenação de rotas por Módulo Funcional para melhor DX
            options.TagActionsBy(api =>
            {
                if (api.GroupName != null)
                {
                    return new[] { api.GroupName };
                }

                var controllerName = api.ActionDescriptor.RouteValues["controller"];
                if (controllerName != null)
                {
                    if (controllerName.Contains("Lead", StringComparison.OrdinalIgnoreCase) || controllerName.Contains("Patient", StringComparison.OrdinalIgnoreCase))
                        return new[] { "1. Módulo de Captação (Leads)" };
                    if (controllerName.Contains("Agendamento", StringComparison.OrdinalIgnoreCase))
                        return new[] { "2. Módulo de Agendamentos" };
                    if (controllerName.Contains("Perfil", StringComparison.OrdinalIgnoreCase))
                        return new[] { "3. Módulo de Perfil do Consulente" };
                    if (controllerName.Contains("Atendimento", StringComparison.OrdinalIgnoreCase) ||
                        controllerName.Contains("Acompanhamento", StringComparison.OrdinalIgnoreCase) ||
                        controllerName.Contains("Spiritual", StringComparison.OrdinalIgnoreCase))
                        return new[] { "4. Módulo de Atendimento Espiritual e Acompanhamento" };

                    return new[] { controllerName };
                }

                return new[] { "Geral" };
            });

            // Configuração de autenticação Bearer para o Swagger UI
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Insira o token JWT de autorização Bearer no formato: Bearer {seu_token_aqui}"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Carrega dinamicamente os XMLs de documentação das DTOs e das ações dos controllers
            AddXmlCommentsIfExists(options, $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml");
            AddXmlCommentsIfExists(options, "Sgae.Application.xml");
        });

        return services;
    }

    private static void AddXmlCommentsIfExists(SwaggerGenOptions options, string xmlFileName)
    {
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFileName);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    }
}
