using BuildingBlocks.Domain.Entities;
using Cart.Core.Enums;

namespace Cart.Core.Entities;

/// <summary>
/// Aggregate Root do carrinho de compras.
/// Corresponde à tabela cart.carts no banco de dados.
/// </summary>
public class Cart : AggregateRoot, IAuditableEntity
{
    public Guid? UserId { get; private set; }
    public string? SessionId { get; private set; }

    public Guid? CouponId { get; private set; }
    public string? CouponCode { get; private set; }
    public decimal DiscountAmount { get; private set; }

    public CartStatus Status { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }

    public int Version { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public DateTime? ConvertedAt { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private readonly List<CartItem> _items = new();
    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    private Cart()
    {
    }

    /// <summary>
    /// Cria um carrinho para um usuário autenticado.
    /// </summary>
    public static Cart CreateForUser(Guid userId, string? ipAddress = null, string? userAgent = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        var cart = new Cart
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Status = CartStatus.Active,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            DiscountAmount = 0,
            Version = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return cart;
    }

    /// <summary>
    /// Cria um carrinho para um visitante (sessão anônima).
    /// </summary>
    public static Cart CreateForSession(string sessionId, string? ipAddress = null, string? userAgent = null)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("Session ID cannot be empty.", nameof(sessionId));

        var cart = new Cart
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            Status = CartStatus.Active,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            DiscountAmount = 0,
            Version = 1,
            ExpiresAt = DateTime.UtcNow.AddDays(30), // Carrinhos de sessão expiram em 30 dias
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return cart;
    }

    /// <summary>
    /// Associa um usuário ao carrinho (após login).
    /// </summary>
    public void AssignToUser(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        UserId = userId;
        SessionId = null;
        ExpiresAt = null;
        UpdatedAt = DateTime.UtcNow;
        Version++;
    }

    /// <summary>
    /// Marca o carrinho como convertido em pedido.
    /// </summary>
    public void MarkAsConverted()
    {
        Status = CartStatus.Converted;
        ConvertedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        Version++;
    }

    /// <summary>
    /// Marca o carrinho como abandonado.
    /// </summary>
    public void MarkAsAbandoned()
    {
        Status = CartStatus.Abandoned;
        UpdatedAt = DateTime.UtcNow;
        Version++;
    }

    /// <summary>
    /// Aplica um cupom de desconto.
    /// </summary>
    public void ApplyCoupon(Guid couponId, string couponCode, decimal discountAmount)
    {
        if (discountAmount < 0)
            throw new ArgumentException("Discount amount cannot be negative.", nameof(discountAmount));

        CouponId = couponId;
        CouponCode = couponCode;
        DiscountAmount = discountAmount;
        UpdatedAt = DateTime.UtcNow;
        Version++;
    }

    /// <summary>
    /// Remove o cupom de desconto.
    /// </summary>
    public void RemoveCoupon()
    {
        CouponId = null;
        CouponCode = null;
        DiscountAmount = 0;
        UpdatedAt = DateTime.UtcNow;
        Version++;
    }
}
