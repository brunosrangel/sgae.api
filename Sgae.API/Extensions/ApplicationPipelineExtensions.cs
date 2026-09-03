using Serilog;
using Sgae.API.Middlewares;

namespace Sgae.API.Extensions;

/// <summary>
/// Extensão responsável por orquestrar o pipeline de middlewares HTTP,
/// preservando a ordem de execução exigida pela aplicação.
/// </summary>
public static class ApplicationPipelineExtensions
{
    public static WebApplication UseSgaeRequestPipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} respondeu {StatusCode} em {Elapsed:0.0000} ms";
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("QueryString", httpContext.Request.QueryString.Value ?? string.Empty);

                var clientIp = httpContext.Connection.RemoteIpAddress?.ToString();
                if (!string.IsNullOrEmpty(clientIp))
                {
                    diagnosticContext.Set("ClientIp", clientIp);
                }

                var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
                if (!string.IsNullOrEmpty(userAgent))
                {
                    diagnosticContext.Set("UserAgent", userAgent);
                }
            };
        });

        // CORS antes dos demais middlewares, para tratar corretamente requisições OPTIONS (pre-flight)
        app.UseCors("SgaeCorsPolicy");

        // Compressão de respostas HTTP para melhor performance de payload
        app.UseResponseCompression();

        // Correlation ID de forma prioritária, para rastreio ponta a ponta
        app.UseMiddleware<CorrelationIdMiddleware>();

        // Tratamento global de exceções (RFC 7807)
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        // Swagger / OpenAPI na raiz da WebAPI com suporte a persistência de Bearer Token
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "SGAE API v1");
            c.DocumentTitle = "SGAE API - Sistema de Gestão de Atendimento Especializado";
            c.RoutePrefix = string.Empty;
            c.EnablePersistAuthorization(); // Mantém o Bearer Token salvo na sessão do Swagger UI
            c.DisplayRequestDuration();
        });

        // Rate limiting com políticas de segurança por IP do cliente
        app.UseRateLimiter();

        // Autenticação JWT Bearer
        app.UseAuthentication();

        // Validação centralizada de status ativo do usuário autenticado em todas as requisições
        app.UseMiddleware<UserActiveValidationMiddleware>();

        // Autorização corporativa e RBAC nas rotas
        app.UseAuthorization();

        return app;
    }
}
