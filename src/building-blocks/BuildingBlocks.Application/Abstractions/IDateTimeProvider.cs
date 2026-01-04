namespace BuildingBlocks.Application.Interfaces;
/// <summary>
/// Interface para abstração de data e hora
/// Permite substituir a implementação real por mocks em testes
///
/// Usado em várias operações do sistema baseadas no schema SQL:
/// - Preenchimento automático de created_at, updated_at (todas as tabelas)
/// - Validação de cupons (valid_from, valid_until)
/// - Expiração de carrinhos (expires_at)
/// - Expiração de pagamentos PIX e Boleto (pix_expiration_at, boleto_expiration_at)
/// - Expiração de sessões (expires_at)
/// - Reservas de estoque (expires_at)
/// - Timestamps de eventos de domínio
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Obtém a data e hora atual em UTC
    /// Usado para todos os campos de timestamp no banco (TIMESTAMPTZ)
    /// </summary>
    DateTime UtcNow { get; }

    /// <summary>
    /// Obtém a data e hora atual no fuso horário local
    /// Usado para exibição de informações ao usuário
    /// </summary>
    DateTime Now { get; }

    /// <summary>
    /// Obtém apenas a data atual (sem hora)
    /// Usado para comparações de data (ex: birth_date, issued_at)
    /// </summary>
    DateOnly Today { get; }

    /// <summary>
    /// Obtém apenas a hora atual (sem data)
    /// </summary>
    TimeOnly TimeOfDay { get; }
}
