using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Users.Core.Entities;

namespace Users.Infrastructure.Persistence;

/// <summary>
/// DbContext para o módulo de usuários.
/// Herda de IdentityDbContext para suporte completo ao ASP.NET Identity.
/// Implementa IUnitOfWork via herança de UnitOfWork (se preferir) ou diretamente.
/// </summary>
/// <remarks>
/// OPÇÃO 1: Herdar de UnitOfWork (requer múltipla herança - não é possível em C#)
/// OPÇÃO 2: Implementar IUnitOfWork diretamente (ESCOLHIDA)
/// 
/// Como IdentityDbContext já herda de DbContext, não podemos herdar também de UnitOfWork.
/// Portanto, implementamos IUnitOfWork diretamente e usamos as extensões.
/// </remarks>
public class UsersDbContext : IdentityDbContext<
    User,
    IdentityRole<Guid>,
    Guid,
    IdentityUserClaim<Guid>,
    IdentityUserRole<Guid>,
    IdentityUserLogin<Guid>,
    IdentityRoleClaim<Guid>,
    IdentityUserToken<Guid>>, 
    BuildingBlocks.Domain.Repositories.IUnitOfWork // ⭐ Implementa IUnitOfWork
{
    public UsersDbContext(DbContextOptions<UsersDbContext> options)
        : base(options)
    {
    }

    // ========================================
    // DBSETS - Entidades Customizadas
    // ========================================
    
    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationPreferences> NotificationPreferences => Set<NotificationPreferences>();
    public DbSet<LoginHistory> LoginHistories => Set<LoginHistory>();

    // ========================================
    // CONFIGURAÇÃO DO MODELO
    // ========================================

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Define o schema padrão como 'users'
        builder.HasDefaultSchema("users");

        // Configura as tabelas do Identity com os nomes corretos do schema.sql
        ConfigureIdentityTables(builder);

        // Aplica todas as configurações de entidades do assembly
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    // ========================================
    // IMPLEMENTAÇÃO DE IUnitOfWork
    // ========================================

    /// <summary>
    /// Implementação explícita de IUnitOfWork.SaveChangesAsync.
    /// </summary>
    /// <remarks>
    /// O base.SaveChangesAsync() do EF Core já faz:
    /// 1. Executa Interceptors (Auditable, SoftDelete, OptimisticConcurrency, DomainEvents)
    /// 2. Persiste mudanças no banco
    /// 3. Faz commit da transação
    /// 
    /// Não precisamos adicionar lógica extra aqui.
    /// </remarks>
    async Task<int> BuildingBlocks.Domain.Repositories.IUnitOfWork.SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Implementação de IUnitOfWork.SaveEntitiesAsync.
    /// </summary>
    /// <remarks>
    /// Usa a extensão SaveEntitiesAsync que captura exceções e retorna booleano.
    /// </remarks>
    async Task<bool> BuildingBlocks.Domain.Repositories.IUnitOfWork.SaveEntitiesAsync(
        CancellationToken cancellationToken)
    {
        // Usa a extensão do UnitOfWorkExtensions
        return await this.SaveEntitiesAsync(cancellationToken);
    }

    // ========================================
    // CONFIGURAÇÃO DAS TABELAS DO IDENTITY
    // ========================================

    private void ConfigureIdentityTables(ModelBuilder builder)
    {
        // Configura User (já configurado em UserConfiguration.cs)
        // Apenas garantindo que está no schema correto
        builder.Entity<User>().ToTable("asp_net_users", "users");

        // Roles
        builder.Entity<IdentityRole<Guid>>(entity =>
        {
            entity.ToTable("asp_net_roles", "users");
            entity.Property(r => r.Id).HasColumnName("id");
            entity.Property(r => r.Name).HasColumnName("name").HasMaxLength(256);
            entity.Property(r => r.NormalizedName).HasColumnName("normalized_name").HasMaxLength(256);
            entity.Property(r => r.ConcurrencyStamp).HasColumnName("concurrency_stamp");

            entity.HasIndex(r => r.NormalizedName)
                .HasDatabaseName("role_name_index")
                .IsUnique()
                .HasFilter("normalized_name IS NOT NULL");
        });

        // UserRoles
        builder.Entity<IdentityUserRole<Guid>>(entity =>
        {
            entity.ToTable("asp_net_user_roles", "users");
            entity.Property(ur => ur.UserId).HasColumnName("user_id");
            entity.Property(ur => ur.RoleId).HasColumnName("role_id");

            entity.HasIndex(ur => ur.RoleId)
                .HasDatabaseName("ix_asp_net_user_roles_role_id");
        });

        // UserClaims
        builder.Entity<IdentityUserClaim<Guid>>(entity =>
        {
            entity.ToTable("asp_net_user_claims", "users");
            entity.Property(uc => uc.Id).HasColumnName("id");
            entity.Property(uc => uc.UserId).HasColumnName("user_id");
            entity.Property(uc => uc.ClaimType).HasColumnName("claim_type").HasMaxLength(255);
            entity.Property(uc => uc.ClaimValue).HasColumnName("claim_value");

            entity.HasIndex(uc => uc.UserId)
                .HasDatabaseName("ix_asp_net_user_claims_user_id");
        });

        // RoleClaims
        builder.Entity<IdentityRoleClaim<Guid>>(entity =>
        {
            entity.ToTable("asp_net_role_claims", "users");
            entity.Property(rc => rc.Id).HasColumnName("id");
            entity.Property(rc => rc.RoleId).HasColumnName("role_id");
            entity.Property(rc => rc.ClaimType).HasColumnName("claim_type").HasMaxLength(255);
            entity.Property(rc => rc.ClaimValue).HasColumnName("claim_value");

            entity.HasIndex(rc => rc.RoleId)
                .HasDatabaseName("ix_asp_net_role_claims_role_id");
        });

        // UserLogins
        builder.Entity<IdentityUserLogin<Guid>>(entity =>
        {
            entity.ToTable("asp_net_user_logins", "users");
            entity.Property(ul => ul.LoginProvider).HasColumnName("login_provider").HasMaxLength(128);
            entity.Property(ul => ul.ProviderKey).HasColumnName("provider_key").HasMaxLength(128);
            entity.Property(ul => ul.ProviderDisplayName).HasColumnName("provider_display_name").HasMaxLength(255);
            entity.Property(ul => ul.UserId).HasColumnName("user_id");

            entity.HasIndex(ul => ul.UserId)
                .HasDatabaseName("ix_asp_net_user_logins_user_id");
        });

        // UserTokens
        builder.Entity<IdentityUserToken<Guid>>(entity =>
        {
            entity.ToTable("asp_net_user_tokens", "users");
            entity.Property(ut => ut.UserId).HasColumnName("user_id");
            entity.Property(ut => ut.LoginProvider).HasColumnName("login_provider").HasMaxLength(128);
            entity.Property(ut => ut.Name).HasColumnName("name").HasMaxLength(128);
            entity.Property(ut => ut.Value).HasColumnName("value");
        });
    }
}

/// <summary>
/// Exemplo de uso do UsersDbContext como Unit of Work.
/// </summary>
public static class UsersDbContextUsageExample
{
    /*
    // ========================================
    // 1. INJEÇÃO NO REPOSITÓRIO
    // ========================================
    
    internal class UserRepository : IUserRepository
    {
        private readonly UsersDbContext _context;

        public UserRepository(UsersDbContext context)
        {
            _context = context;
        }

        // IUnitOfWork vem do próprio DbContext
        public IUnitOfWork UnitOfWork => _context;

        public async Task AddAsync(User user, CancellationToken ct = default)
        {
            await _context.Users.AddAsync(user, ct);
        }
    }

    // ========================================
    // 2. USO EM COMMAND HANDLER
    // ========================================
    
    public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, Guid>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork; // ⭐ Injetado do DI

        public async Task<Result<Guid>> Handle(
            CreateUserCommand command,
            CancellationToken ct)
        {
            // Criar usuário
            var user = User.Create(command.Email, command.UserName);
            user.CreateProfile(command.FirstName, command.LastName);
            
            // Adicionar ao repositório (ChangeTracker rastreia)
            await _userRepository.AddAsync(user, ct);
            
            // Salvar via Unit of Work
            // Interceptors são executados automaticamente:
            // - AuditableEntityInterceptor
            // - PublishDomainEventsInterceptor (salva eventos no Outbox)
            await _unitOfWork.SaveChangesAsync(ct);
            
            return Result.Ok(user.Id);
        }
    }

    // ========================================
    // 3. COM VALIDAÇÃO DE SUCESSO
    // ========================================
    
    public class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public async Task<Result> Handle(
            UpdateUserCommand command,
            CancellationToken ct)
        {
            var user = await _userRepository.GetByIdAsync(command.UserId, ct);
            
            if (user == null)
                return Result.Fail(Error.NotFound("USER_NOT_FOUND", "User not found"));
            
            user.UpdateEmail(command.NewEmail);
            
            // Tenta salvar e retorna false se falhar (ex: concorrência, constraint)
            var success = await _unitOfWork.SaveEntitiesAsync(ct);
            
            if (!success)
                return Result.Fail(Error.Conflict("SAVE_FAILED", "Failed to save changes"));
            
            return Result.Ok();
        }
    }

    // ========================================
    // 4. CONFIGURAÇÃO NO DI (DependencyInjection.cs)
    // ========================================
    
    public static IServiceCollection AddUsersInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext
        services.AddDbContext<UsersDbContext>((serviceProvider, options) =>
        {
            // ... configuração ...
        });

        // ⭐ IMPORTANTE: Registrar DbContext como IUnitOfWork
        services.AddScoped<IUnitOfWork>(provider => 
            provider.GetRequiredService<UsersDbContext>());

        // Repositórios
        services.AddScoped<IUserRepository, UserRepository>();
        // ... outros repositórios

        return services;
    }

    // ========================================
    // 5. TRATAMENTO DE ERROS DE CONCORRÊNCIA
    // ========================================
    
    public class SafeUpdateHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;

        public async Task<Result> UpdateWithRetry(
            Guid userId,
            string newEmail,
            CancellationToken ct)
        {
            const int maxRetries = 3;
            var retryCount = 0;

            while (retryCount < maxRetries)
            {
                try
                {
                    var user = await _userRepository.GetByIdAsync(userId, ct);
                    
                    if (user == null)
                        return Result.Fail(Error.NotFound("USER_NOT_FOUND", "User not found"));
                    
                    user.UpdateEmail(newEmail);
                    
                    await _unitOfWork.SaveChangesAsync(ct);
                    
                    return Result.Ok();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    retryCount++;
                    
                    if (retryCount >= maxRetries)
                    {
                        _logger.LogWarning(ex, 
                            "Concurrency conflict after {RetryCount} retries", retryCount);
                        
                        return Result.Fail(Error.Conflict(
                            "CONCURRENCY_CONFLICT",
                            "The record was modified by another user. Please try again."
                        ));
                    }
                    
                    // Aguardar antes de tentar novamente
                    await Task.Delay(TimeSpan.FromMilliseconds(100 * retryCount), ct);
                }
            }

            return Result.Fail(Error.Conflict(
                "MAX_RETRIES_EXCEEDED",
                "Failed to save after multiple attempts."
            ));
        }
    }

    // ========================================
    // 6. TRANSAÇÃO EXPLÍCITA (Cenário avançado)
    // ========================================
    
    public class TransactionalOperationHandler
    {
        private readonly UsersDbContext _context;

        public async Task<Result> ComplexOperation(CancellationToken ct)
        {
            // Usar transação explícita para múltiplas operações
            await using var transaction = await _context.BeginTransactionAsync(ct);
            
            try
            {
                // Operação 1: Criar usuário
                var user = User.Create("test@example.com", "testuser");
                _context.Users.Add(user);
                await _context.SaveChangesAsync(ct);
                
                // Operação 2: Criar perfil
                var profile = new Profile(user.Id, "John", "Doe");
                _context.Profiles.Add(profile);
                await _context.SaveChangesAsync(ct);
                
                // Operação 3: Adicionar endereço
                var address = new Address(
                    user.Id,
                    "123 Main St",
                    "São Paulo",
                    "SP",
                    "01234-567"
                );
                _context.Addresses.Add(address);
                await _context.SaveChangesAsync(ct);
                
                // Commit da transação
                await transaction.CommitAsync(ct);
                
                return Result.Ok();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                return Result.Fail(Error.Failure("TRANSACTION_FAILED", ex.Message));
            }
        }
    }
    */
}