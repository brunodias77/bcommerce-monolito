namespace Catalog.Core.Enums;

/// <summary>
/// Status possíveis de um produto.
/// Corresponde ao enum shared.product_status no PostgreSQL.
/// </summary>
public enum ProductStatus
{
    /// <summary>
    /// Produto em rascunho, não visível para clientes.
    /// </summary>
    Draft = 0,
    
    /// <summary>
    /// Produto ativo e disponível para venda.
    /// </summary>
    Active = 1,
    
    /// <summary>
    /// Produto desativado temporariamente.
    /// </summary>
    Inactive = 2,
    
    /// <summary>
    /// Produto sem estoque disponível.
    /// </summary>
    OutOfStock = 3,
    
    /// <summary>
    /// Produto descontinuado permanentemente.
    /// </summary>
    Discontinued = 4
}
