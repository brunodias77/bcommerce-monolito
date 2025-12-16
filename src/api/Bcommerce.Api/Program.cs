using Bcommerce.Api.Configurations;
using Serilog;

// Cria o WebApplicationBuilder, que inicializa configurações, logging e DI container.
var builder = WebApplication.CreateBuilder(args);

// Configuração do Serilog
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

// ===== CONFIGURAÇÃO DE SERVIÇOS (Dependency Injection) =====
// Aqui registramos todos os serviços, repositórios, contexto de banco, etc.
// A organização é feita por métodos de extensão para manter o Program.cs limpo.

builder.Services
    .AddApplication(builder.Configuration)   // Camada Application: MediatR, Behaviors (Logs, Validação, Transação), Validators
    .AddInfrastructure(builder.Configuration) // Camada Infrastructure: DbContexts, Repositories, Serviços Externos, Cache, Bus
    .AddWebApi();                             // Camada API: Controllers, Swagger, Filters, Configurações Web

// ===== CONSTRUÇÃO E CONFIGURAÇÃO DO PIPELINE =====
// Constrói a instância da aplicação web pronta para receber requisições.

var app = builder.Build();

// Configura o pipeline de Middlewares (ordem é crucial: Exception -> Auth -> Controllers)
app.UseMiddlewarePipeline();

// ===== INICIAR APLICAÇÃO =====
// Inicia o servidor Kestrel e começa a escutar requisições.

app.Run();