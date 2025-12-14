using Bcommerce.Api.Configurations;

var builder = WebApplication.CreateBuilder(args);

// ===== CONFIGURAÇÃO DE SERVIÇOS =====

builder.Services
    .AddApplication(builder.Configuration)   // MediatR, Behaviors, Validators
    .AddInfrastructure(builder.Configuration) // DbContexts, Repositories, Services
    .AddWebApi();                             // Controllers, Swagger, Filters

// ===== CONSTRUÇÃO E CONFIGURAÇÃO DO PIPELINE =====

var app = builder.Build();

app.UseMiddlewarePipeline();

// ===== INICIAR APLICAÇÃO =====

app.Run();