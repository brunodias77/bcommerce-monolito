using Bcommerce.Api.Configurations;

var builder = WebApplication.CreateBuilder(args);

// ===== CONFIGURAÇÃO DE SERVIÇOS =====

// Configuração da camada de aplicação (MediatR, Behaviors, etc.)
builder.Services.AddApplication(builder.Configuration);

// Configuração da camada de infraestrutura (DbContexts, Repositories, Interceptors, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

// Configuração de Controllers
builder.Services.AddControllers();

// Configuração do Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "BCommerce API",
        Version = "v1",
        Description = "API do BCommerce - Modular Monolith com DDD"
    });

    // Adicionar XML comments se necessário
    // var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    // options.IncludeXmlComments(xmlPath);
});

// ===== CONSTRUÇÃO DA APLICAÇÃO =====

var app = builder.Build();

// ===== CONFIGURAÇÃO DO PIPELINE HTTP =====

// Swagger (apenas em desenvolvimento)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "BCommerce API v1");
        options.RoutePrefix = string.Empty; // Swagger na raiz (http://localhost:5000)
    });

    // Habilita logging detalhado em desenvolvimento
    app.UseDeveloperExceptionPage();
}

// HTTPS Redirection
app.UseHttpsRedirection();

// Authentication & Authorization
// Será configurado quando implementar autenticação
// app.UseAuthentication();
app.UseAuthorization();

// Mapeamento de Controllers
app.MapControllers();

// ===== EXECUTAR MIGRATIONS (OPCIONAL - APENAS DESENVOLVIMENTO) =====
// IMPORTANTE: Em produção, execute migrations via scripts SQL ou CI/CD
// if (app.Environment.IsDevelopment())
// {
//     using var scope = app.Services.CreateScope();
//     var services = scope.ServiceProvider;
//
//     try
//     {
//         // Executar migrations de cada módulo
//         var usersDb = services.GetRequiredService<UsersDbContext>();
//         await usersDb.Database.MigrateAsync();
//
//         var catalogDb = services.GetRequiredService<CatalogDbContext>();
//         await catalogDb.Database.MigrateAsync();
//
//         var ordersDb = services.GetRequiredService<OrdersDbContext>();
//         await ordersDb.Database.MigrateAsync();
//
//         var paymentsDb = services.GetRequiredService<PaymentsDbContext>();
//         await paymentsDb.Database.MigrateAsync();
//
//         var couponsDb = services.GetRequiredService<CouponsDbContext>();
//         await couponsDb.Database.MigrateAsync();
//
//         var cartDb = services.GetRequiredService<CartDbContext>();
//         await cartDb.Database.MigrateAsync();
//     }
//     catch (Exception ex)
//     {
//         var logger = services.GetRequiredService<ILogger<Program>>();
//         logger.LogError(ex, "Erro ao executar migrations");
//     }
// }

// ===== INICIAR APLICAÇÃO =====

app.Run();