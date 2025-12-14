namespace Cart.Core.Enums;

/// <summary>
/// Status do carrinho de compras.
/// </summary>
public enum CartStatus
{
    /// <summary>
    /// Carrinho ativo, disponível para adicionar itens.
    /// </summary>
    Active,

    /// <summary>
    /// Carrinho abandonado (expirado).
    /// </summary>
    Abandoned,

    /// <summary>
    /// Carrinho convertido em pedido.
    /// </summary>
    Converted,

    /// <summary>
    /// Carrinho mesclado com outro.
    /// </summary>
    Merged
}
