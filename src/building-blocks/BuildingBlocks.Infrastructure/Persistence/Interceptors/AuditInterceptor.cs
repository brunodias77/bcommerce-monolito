using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Security.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BuildingBlocks.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Interceptor que preenche automaticamente campos de auditoria (CreatedAt, UpdatedAt, CreatedBy, UpdatedBy)
/// em entidades que implementam as interfaces correspondentes
/// </summary>
public sealed class AuditInterceptor : SaveChangesInterceptor
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICurrentUser? _currentUser;

    public AuditInterceptor(
        IDateTimeProvider dateTimeProvider,
        ICurrentUser? currentUser = null)
    {
        _dateTimeProvider = dateTimeProvider;
        _currentUser = currentUser;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            AtualizarCamposDeAuditoria(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            AtualizarCamposDeAuditoria(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    /// <summary>
    /// Atualiza os campos de auditoria nas entidades modificadas
    /// </summary>
    private void AtualizarCamposDeAuditoria(DbContext context)
    {
        var agora = _dateTimeProvider.UtcNow;
        var usuarioAtual = _currentUser?.UserId;

        var entradas = context.ChangeTracker
            .Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entrada in entradas)
        {
            // Preenche campos de data/hora
            PreencherTimestamps(entrada, agora);

            // Preenche campos de usuário se o serviço estiver disponível
            if (usuarioAtual.HasValue)
            {
                PreencherUsuario(entrada, usuarioAtual.Value);
            }
        }
    }

    /// <summary>
    /// Preenche os campos CreatedAt e UpdatedAt
    /// </summary>
    private void PreencherTimestamps(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entrada, DateTime agora)
    {
        if (entrada.State == EntityState.Added)
        {
            // Nova entidade - define CreatedAt
            var propriedadeCreatedAt = entrada.Properties
                .FirstOrDefault(p => p.Metadata.Name == "CreatedAt");

            if (propriedadeCreatedAt != null)
            {
                propriedadeCreatedAt.CurrentValue = agora;
            }
        }

        if (entrada.State == EntityState.Added || entrada.State == EntityState.Modified)
        {
            // Entidade nova ou modificada - atualiza UpdatedAt
            var propriedadeUpdatedAt = entrada.Properties
                .FirstOrDefault(p => p.Metadata.Name == "UpdatedAt");

            if (propriedadeUpdatedAt != null)
            {
                propriedadeUpdatedAt.CurrentValue = agora;
            }
        }
    }

    /// <summary>
    /// Preenche os campos CreatedBy e UpdatedBy
    /// </summary>
    private void PreencherUsuario(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entrada, Guid usuarioId)
    {
        if (entrada.State == EntityState.Added)
        {
            // Nova entidade - define CreatedBy
            var propriedadeCreatedBy = entrada.Properties
                .FirstOrDefault(p => p.Metadata.Name == "CreatedBy");

            if (propriedadeCreatedBy != null)
            {
                propriedadeCreatedBy.CurrentValue = usuarioId;
            }
        }

        if (entrada.State == EntityState.Added || entrada.State == EntityState.Modified)
        {
            // Entidade nova ou modificada - atualiza UpdatedBy
            var propriedadeUpdatedBy = entrada.Properties
                .FirstOrDefault(p => p.Metadata.Name == "UpdatedBy");

            if (propriedadeUpdatedBy != null)
            {
                propriedadeUpdatedBy.CurrentValue = usuarioId;
            }
        }
    }
}
