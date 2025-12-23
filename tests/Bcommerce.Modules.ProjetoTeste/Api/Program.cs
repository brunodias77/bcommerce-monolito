
using Bcommerce.BuildingBlocks.Web.Extensions;
using Bcommerce.Modules.ProjetoTeste.Application;
using Bcommerce.Modules.ProjetoTeste.Infrastructure;

// Note: Additional imports for Security/Observability libraries if they are not transitively included or if we need to add references.
// Assuming Web project has reference to BuildingBlocks.Web which references others or we added them.
// We added direct references to Application/Infra/Web. 
// We might need to add package references for Serilog/Swagger if they are not in BuildingBlocks.Web
// BuildingBlocks.Web usually exposes them.

var builder = WebApplication.CreateBuilder(args);

// 1. Add Building Blocks Services
builder.Services.AddBuildingBlocksWeb(); // Controllers, Filters, Middlewares

// 2. Add Module Services
builder.Services.AddTestProjectApplication();
builder.Services.AddTestProjectInfrastructure(builder.Configuration);

// 3. Add Swagger (Standard ASP.NET Core)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 4. Configure Pipeline using BuildingBlocks
app.UseBuildingBlocksWeb(); // Global Middlewares (Exception, Logging, Correlation, Tenant)

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
