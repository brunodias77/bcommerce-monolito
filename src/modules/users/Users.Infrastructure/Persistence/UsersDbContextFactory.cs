using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Users.Infrastructure.Persistence;

/// <summary>
/// Factory para criar instâncias do UsersDbContext em tempo de design.
/// Utilizado pelo EF Core Tools para criar migrations sem precisar executar a aplicação.
/// </summary>
public class UsersDbContextFactory : IDesignTimeDbContextFactory<UsersDbContext>
{
    public UsersDbContext CreateDbContext(string[] args)
    {
        // Determina o caminho base relativo ao diretório da API
        var basePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "..", "api", "Bcommerce.Api");

        // Se não encontrar, tenta o diretório atual (quando executado da raiz)
        if (!Directory.Exists(basePath))
        {
            basePath = Path.Combine(Directory.GetCurrentDirectory(), "src", "api", "Bcommerce.Api");
        }

        // Se ainda não encontrar, usa um caminho padrão
        if (!Directory.Exists(basePath))
        {
            basePath = Directory.GetCurrentDirectory();
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Se não tiver connection string, usa uma padrão para geração de migrations
        if (string.IsNullOrEmpty(connectionString))
        {
            connectionString = "Host=localhost;Port=5432;Database=bcommerce;Username=postgres;Password=postgres";
        }

        var optionsBuilder = new DbContextOptionsBuilder<UsersDbContext>();
        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history", "users");
        });

        return new UsersDbContext(optionsBuilder.Options);
    }
}
