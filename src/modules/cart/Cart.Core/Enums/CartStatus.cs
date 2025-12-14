namespace Cart.Core.Enums;

/// <summary>
/// Status do carrinho de compras.
/// </summary>
public enum CartStatus
{
    /// <summary>
    /// Carrinho ativo, disponível para adicionar itens.
    /// </summary>
    ACTIVE,

    /// <summary>
    /// Carrinho abandonado (expirado).
    /// </summary>
    ABANDONED,

    /// <summary>
    /// Carrinho convertido em pedido.
    /// </summary>
    CONVERTED,

    /// <summary>
    /// Carrinho mesclado com outro.
    /// </summary>
    MERGED
}
