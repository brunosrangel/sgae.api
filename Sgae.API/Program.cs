using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Sgae.Application;
using Sgae.Application.Abstractions;
using Sgae.Infrastructure.Persistence;
using Sgae.API.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Adiciona Serviços das Camadas de Arquitetura Clean
builder.Services.AddApplication(); // Registra o MediatR e pipeline CQRS via método de extensão da Application

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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