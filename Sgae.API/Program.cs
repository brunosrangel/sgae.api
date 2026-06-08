using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System;
using Sgae.Application;
using Sgae.Application.Abstractions;
using Sgae.Infrastructure.Persistence;
using Sgae.API.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Adiciona Serviços das Camadas de Arquitetura Clean
builder.Services.AddApplication(); // Registra o MediatR e pipeline CQRS via método de extensão da Application

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "SGAE API - Sistema de Gestão de Atendimento Especializado",
        Description = "API RESTful corporativa para gestão da jornada do consulente, estruturada sobre os princípios de Clean Architecture e DDD.\n\n" +
                      "### 📌 Módulos Funcionais da API:\n" +
                      "- **Módulo 1: Captação (Leads)** - Registro inicial e controle de captação de consulentes.\n" +
                      "- **Módulo 2: Agendamentos** - Gestão do fluxo de datas, status e valores de consultas.\n" +
                      "- **Módulo 3: Perfil do Consulente** - Enriquecimento sócio-demográfico e qualificação personalizada.\n\n" +
                      "*Padrões adotados: CQRS, Mediator Pipeline Behaviors, FluentValidation e Soft-Delete.*",
        Contact = new OpenApiContact
        {
            Name = "Equipe de Arquitetura e Engenharia SGAE",
            Email = "arquitetura@sgae.com.br"
        }
    });

    // Agrupamento lógico e ordenação de rotas por Módulo Funcional para melhor DX (Developer Experience)
    options.TagActionsBy(api =>
    {
        if (api.GroupName != null)
        {
            return new[] { api.GroupName };
        }

        var controllerName = api.ActionDescriptor.RouteValues["controller"];
        if (controllerName != null)
        {
            if (controllerName.Contains("Lead", StringComparison.OrdinalIgnoreCase))
                return new[] { "1. Módulo de Captação (Leads)" };
            if (controllerName.Contains("Agendamento", StringComparison.OrdinalIgnoreCase))
                return new[] { "2. Módulo de Agendamentos" };
            if (controllerName.Contains("Perfil", StringComparison.OrdinalIgnoreCase))
                return new[] { "3. Módulo de Perfil do Consulente" };

            return new[] { controllerName };
        }

        return new[] { "Geral" };
    });
});

// Configuração do DbContext com PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registro das implementações físicas da camada de Infrastructure
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

var app = builder.Build();

// ATIVE O MIDDLEWARE DE EXCEÇÕES GLOBAL (RFC 7807)
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();