using BuildingBlocks.Application;
using BuildingBlocks.Infrastructure;
using BuildingBlocks.Messaging;
using BuildingBlocks.Security;
using BuildingBlocks.Web;
using BuildingBlocks.Web.Extensions;
using Serilog;
using Web.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configuração do Serilog
builder.AddSerilogLogging();

// Add services to the container.

// Configuração dos Building Blocks
builder.Services.AddApplicationServices(typeof(Program).Assembly);
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddWebServices();
builder.Services.AddMessagingServices(builder.Configuration);
builder.Services.AddSecurityServices(builder.Configuration);

// Configuração do Swagger
builder.Services.AddECommerceSwagger();

var app = builder.Build();

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerDocumentation();
}

app.UseHttpsRedirection();

// Configuração dos Building Blocks Web (ExceptionHandler, CORS, Auth, AuthZ)
app.UseWebServices();

// Logging de requisições
app.UseSerilogRequestLogging();

app.MapControllers();

app.EnsureLogsAreFlushed();

app.Run();