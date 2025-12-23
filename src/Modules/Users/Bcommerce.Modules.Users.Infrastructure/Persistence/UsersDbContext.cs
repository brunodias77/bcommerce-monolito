using System.Reflection;
using Bcommerce.BuildingBlocks.Application.Abstractions.Data;
using Bcommerce.BuildingBlocks.Infrastructure.Data.Interceptors;
using Bcommerce.Modules.Users.Domain.Entities;
using Bcommerce.Modules.Users.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Bcommerce.Modules.Users.Infrastructure.Persistence;

public class UsersDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IUnitOfWork
{
    private readonly AuditableEntityInterceptor _auditableEntityInterceptor;
    private readonly SoftDeleteInterceptor _softDeleteInterceptor;
    private readonly DomainEventInterceptor _domainEventInterceptor;
    private readonly OptimisticLockInterceptor _optimisticLockInterceptor;

    public UsersDbContext(
        DbContextOptions<UsersDbContext> options,
        AuditableEntityInterceptor auditableEntityInterceptor,
        SoftDeleteInterceptor softDeleteInterceptor,
        DomainEventInterceptor domainEventInterceptor,
        OptimisticLockInterceptor optimisticLockInterceptor)
        : base(options)
    {
        _auditableEntityInterceptor = auditableEntityInterceptor;
        _softDeleteInterceptor = softDeleteInterceptor;
        _domainEventInterceptor = domainEventInterceptor;
        _optimisticLockInterceptor = optimisticLockInterceptor;
    }

    public DbSet<Profile> Profiles { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<NotificationPreference> NotificationPreferences { get; set; }
    public DbSet<LoginHistory> LoginHistory { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(
            _auditableEntityInterceptor,
            _softDeleteInterceptor,
            // _domainEventInterceptor,
            _optimisticLockInterceptor);

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("users");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UsersDbContext).Assembly);
        
        // Customizations for Identity tables if needed (rename table names to snake_case etc)
        modelBuilder.Entity<ApplicationUser>(b => b.ToTable("asp_net_users"));
        modelBuilder.Entity<ApplicationRole>(b => b.ToTable("asp_net_roles"));
        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<Guid>>(b => b.ToTable("asp_net_user_claims"));
        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<Guid>>(b => b.ToTable("asp_net_role_claims"));
        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<Guid>>(b => b.ToTable("asp_net_user_logins"));
        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<Guid>>(b => b.ToTable("asp_net_user_tokens"));
        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>>(b => b.ToTable("asp_net_user_roles"));
    }
}
