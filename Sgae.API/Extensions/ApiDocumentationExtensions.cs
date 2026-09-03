using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Sgae.API.Extensions;

/// <summary>
/// Extensões responsáveis pela configuração do Swagger/OpenAPI,
/// incluindo agrupamento de rotas por módulo funcional, documentação detalhada
/// e integração interativa com autenticação JWT Bearer.
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
                              "- **Módulo de Autenticação e Segurança (Auth/RBAC)** - Autenticação JWT, Refresh Tokens, troca de senha e gestão de usuários.\n" +
                              "- **Módulo 1: Captação (Leads)** - Registro inicial e controle de captação de consulentes.\n" +
                              "- **Módulo 2: Agendamentos** - Gestão do fluxo de datas, status e valores de consultas.\n" +
                              "- **Módulo 3: Perfil do Consulente** - Enriquecimento sócio-demográfico e qualificação personalizada.\n" +
                              "- **Módulo 4: Atendimento Espiritual e Acompanhamento** - Registro de atendimentos e monitoramento continuado de evolução.\n\n" +
                              "### 🔐 Como Autenticar no Swagger:\n" +
                              "1. Realize uma requisição no endpoint `POST /api/auth/login` informando e-mail e senha.\n" +
                              "2. Copie o valor do campo `accessToken` retornado no JSON.\n" +
                              "3. Clique no botão **Authorize 🔓** no topo superior direito desta página.\n" +
                              "4. Cole o token no campo de texto (ex: `Bearer eyJhbGciOi...` ou apenas `eyJhbGciOi...`) e clique em **Authorize**.\n" +
                              "5. Todos os endpoints protegidos com `[Authorize]` enviarão automaticamente o cabeçalho `Authorization: Bearer <token>`.",
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
                    if (controllerName.Contains("Health", StringComparison.OrdinalIgnoreCase))
                        return new[] { "0. Monitoramento e Saúde (Health Checks)" };
                    if (controllerName.Contains("Auth", StringComparison.OrdinalIgnoreCase))
                        return new[] { "0. Autenticação e Segurança (Auth)" };
                    if (controllerName.Contains("Usuario", StringComparison.OrdinalIgnoreCase))
                        return new[] { "0. Administração de Usuários (RBAC)" };
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

            // Configuração do Esquema de Segurança Bearer (JWT) no OpenAPI
            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Autenticação JWT Bearer. Insira 'Bearer' [espaço] e o seu token de acesso JWT gerado no login.",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            };

            options.AddSecurityDefinition("Bearer", securityScheme);

            // Adiciona o requisito de segurança global com fallback para OperationFilter
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

            // OperationFilter para adicionar indicadores visuais de segurança e códigos 401/403
            options.OperationFilter<AuthorizeCheckOperationFilter>();

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

/// <summary>
/// OperationFilter para identificar endpoints que exigem autenticação/autorização,
/// adicionando os status codes 401 Unauthorized e 403 Forbidden e os requisitos de Bearer Token.
/// </summary>
public class AuthorizeCheckOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAnonymous = context.MethodInfo.DeclaringType?.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any() == true
                           || context.MethodInfo.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any();

        if (hasAnonymous)
            return;

        var hasAuthorize = context.MethodInfo.DeclaringType?.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() == true
                           || context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();

        if (hasAuthorize)
        {
            if (!operation.Responses.ContainsKey(StatusCodes.Status401Unauthorized.ToString()))
            {
                operation.Responses.Add(StatusCodes.Status401Unauthorized.ToString(), new OpenApiResponse
                {
                    Description = "Não Autorizado - Token JWT ausente, inválido ou expirado."
                });
            }

            if (!operation.Responses.ContainsKey(StatusCodes.Status403Forbidden.ToString()))
            {
                operation.Responses.Add(StatusCodes.Status403Forbidden.ToString(), new OpenApiResponse
                {
                    Description = "Acesso Negado - Usuário sem perfil/permissão suficiente (RBAC) ou status inativo."
                });
            }

            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
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
                }
            };
        }
    }
}
