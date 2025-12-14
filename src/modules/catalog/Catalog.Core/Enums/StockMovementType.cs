namespace Catalog.Core.Enums;

/// <summary>
/// Tipos de movimentação de estoque.
/// Corresponde ao enum shared.stock_movement_type no PostgreSQL.
/// </summary>
public enum StockMovementType
{
    /// <summary>
    /// Entrada de estoque (compra, devolução, etc).
    /// </summary>
    In = 0,
    
    /// <summary>
    /// Saída de estoque (venda, perda, etc).
    /// </summary>
    Out = 1,
    
    /// <summary>
    /// Ajuste de estoque (inventário, correção).
    /// </summary>
    Adjustment = 2,
    
    /// <summary>
    /// Reserva de estoque para pedido.
    /// </summary>
    Reserve = 3,
    
    /// <summary>
    /// Liberação de estoque reservado.
    /// </summary>
    Release = 4
}
