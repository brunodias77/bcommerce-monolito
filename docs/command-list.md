# Lista Completa de Commands, Queries, Events e Integração - E-commerce Modular Monolith

## Índice

- [Módulo Users](#módulo-users)
- [Módulo Catalog](#módulo-catalog)
- [Módulo Cart](#módulo-cart)
- [Módulo Orders](#módulo-orders)
- [Módulo Payments](#módulo-payments)
- [Módulo Coupons](#módulo-coupons)
- [Eventos de Integração (Cross-Module)](#eventos-de-integração-cross-module)
- [Regras de Comunicação](#regras-de-comunicação)

---

## Módulo Users

### Commands

#### RegisterUser
- **Arquivo**: `Ecommerce.Modules.Users.Application/Commands/RegisterUser/RegisterUserCommand.cs`
- **Handler**: `RegisterUserCommandHandler.cs`
- **Validator**: `RegisterUserCommandValidator.cs`
- **Descrição**: Registra um novo usuário no sistema
- **Retorno**: `Result<UserDto>`

#### UpdateProfile
- **Arquivo**: `Ecommerce.Modules.Users.Application/Commands/UpdateProfile/UpdateProfileCommand.cs`
- **Handler**: `UpdateProfileCommandHandler.cs`
- **Validator**: `UpdateProfileCommandValidator.cs`
- **Descrição**: Atualiza perfil do usuário
- **Retorno**: `Result<ProfileDto>`

#### AddAddress
- **Arquivo**: `Ecommerce.Modules.Users.Application/Commands/AddAddress/AddAddressCommand.cs`
- **Handler**: `AddAddressCommandHandler.cs`
- **Validator**: `AddAddressCommandValidator.cs`
- **Descrição**: Adiciona novo endereço ao usuário
- **Retorno**: `Result<AddressDto>`

### Queries (API Pública)

#### GetUserById
- **Arquivo**: `Ecommerce.Modules.Users.Application/Queries/GetUserById/GetUserByIdQuery.cs`
- **Handler**: `GetUserByIdQueryHandler.cs`
- **Descrição**: Busca usuário por ID
- **Retorno**: `UserDto`
- **Visibilidade**: ✅ Disponível para outros módulos via Contracts

#### GetUserProfile
- **Arquivo**: `Ecommerce.Modules.Users.Application/Queries/GetUserProfile/GetUserProfileQuery.cs`
- **Handler**: `GetUserProfileQueryHandler.cs`
- **Descrição**: Busca perfil completo do usuário
- **Retorno**: `ProfileDto`
- **Visibilidade**: ✅ Disponível para outros módulos via Contracts

#### GetUserAddresses
- **Arquivo**: `Ecommerce.Modules.Users.Application/Queries/GetUserAddresses/GetUserAddressesQuery.cs`
- **Handler**: `GetUserAddressesQueryHandler.cs`
- **Descrição**: Lista endereços do usuário
- **Retorno**: `List<AddressDto>`
- **Visibilidade**: ✅ Disponível para outros módulos via Contracts

#### GetUserAddress (Query Específica)
- **Descrição**: Busca endereço específico do usuário (usado por Orders)
- **Params**: `userId: Guid, addressId: Guid`
- **Retorno**: `AddressDto`
- **Visibilidade**: ✅ Disponível para outros módulos via Contracts

### Domain Events (Internos)

#### UserRegisteredEvent
- **Arquivo**: `Ecommerce.Modules.Users.Core/Domain/Events/UserRegisteredEvent.cs`
- **Handler**: `UserRegisteredEventHandler.cs`
- **Quando**: Após registro de novo usuário
- **Ação**: Envia email de boas-vindas, cria preferências padrão

#### UserLoggedInEvent
- **Arquivo**: `Ecommerce.Modules.Users.Core/Domain/Events/UserLoggedInEvent.cs`
- **Quando**: Após login bem-sucedido
- **Ação**: Registra histórico de login, atualiza sessão

#### ProfileUpdatedEvent
- **Arquivo**: `Ecommerce.Modules.Users.Core/Domain/Events/ProfileUpdatedEvent.cs`
- **Handler**: `ProfileUpdatedEventHandler.cs`
- **Quando**: Após atualização de perfil
- **Ação**: Invalida cache, atualiza índices

#### AddressAddedEvent
- **Arquivo**: `Ecommerce.Modules.Users.Core/Domain/Events/AddressAddedEvent.cs`
- **Quando**: Após adicionar novo endereço
- **Ação**: Valida CEP, geocoding

### Integration Events (Públicos)

#### UserRegisteredIntegrationEvent
- **Arquivo**: `Ecommerce.Modules.Users.Contracts/Events/UserRegisteredIntegrationEvent.cs`
- **Publicado**: Quando novo usuário se registra
- **Consumido por**: Outros módulos que precisam reagir a novo usuário
- **Payload**:
  - `UserId: Guid`
  - `Email: string`
  - `FullName: string`
  - `RegisteredAt: DateTime`

#### ProfileUpdatedIntegrationEvent
- **Arquivo**: `Ecommerce.Modules.Users.Contracts/Events/ProfileUpdatedIntegrationEvent.cs`
- **Publicado**: Quando perfil é atualizado
- **Consumido por**: Módulos que mantêm cache de dados do usuário
- **Payload**:
  - `UserId: Guid`
  - `UpdatedFields: Dictionary<string, object>`
  - `UpdatedAt: DateTime`

---

## Módulo Catalog

### Commands

#### CreateProduct
- **Arquivo**: `Ecommerce.Modules.Catalog.Application/Commands/CreateProduct/CreateProductCommand.cs`
- **Handler**: `CreateProductCommandHandler.cs`
- **Validator**: `CreateProductCommandValidator.cs`
- **Descrição**: Cria novo produto no catálogo
- **Retorno**: `Result<ProductDto>`

#### UpdateProduct
- **Arquivo**: `Ecommerce.Modules.Catalog.Application/Commands/UpdateProduct/UpdateProductCommand.cs`
- **Handler**: `UpdateProductCommandHandler.cs`
- **Validator**: `UpdateProductCommandValidator.cs`
- **Descrição**: Atualiza informações do produto
- **Retorno**: `Result<ProductDto>`

#### ReserveStock
- **Arquivo**: `Ecommerce.Modules.Catalog.Application/Commands/ReserveStock/ReserveStockCommand.cs`
- **Handler**: `ReserveStockCommandHandler.cs`
- **Validator**: `ReserveStockCommandValidator.cs`
- **Descrição**: Reserva estoque para checkout
- **Params**:
  - `ProductId: Guid`
  - `Quantity: int`
  - `ReferenceType: string` (Cart, Order)
  - `ReferenceId: Guid`
  - `ExpiresAt: DateTime`
- **Retorno**: `Result`
- **Visibilidade**: ✅ Disponível via Contracts para Cart e Orders

#### ReleaseStock
- **Arquivo**: `Ecommerce.Modules.Catalog.Application/Commands/ReleaseStock/ReleaseStockCommand.cs`
- **Handler**: `ReleaseStockCommandHandler.cs`
- **Validator**: `ReleaseStockCommandValidator.cs`
- **Descrição**: Libera reserva de estoque
- **Params**:
  - `ProductId: Guid`
  - `Quantity: int`
  - `ReferenceId: Guid`
- **Retorno**: `Result`
- **Visibilidade**: ✅ Disponível via Contracts para Cart e Orders

### Queries (API Pública)

#### GetProductById
- **Arquivo**: `Ecommerce.Modules.Catalog.Application/Queries/GetProductById/GetProductByIdQuery.cs`
- **Handler**: `GetProductByIdQueryHandler.cs`
- **Descrição**: Busca produto por ID com informações públicas
- **Params**: `ProductId: Guid`
- **Retorno**: `ProductPublicDto`
- **DTOs Públicos**:
  ```csharp
  public record ProductPublicDto(
      Guid Id,
      string Name,
      string Sku,
      decimal Price,
      int Stock,
      bool IsActive
  );
  ```
- **Visibilidade**: ✅ Disponível via Contracts para Cart, Orders, Coupons

#### SearchProducts
- **Arquivo**: `Ecommerce.Modules.Catalog.Application/Queries/SearchProducts/SearchProductsQuery.cs`
- **Handler**: `SearchProductsQueryHandler.cs`
- **Descrição**: Busca produtos com filtros
- **Retorno**: `PagedList<ProductSearchResultDto>`
- **Visibilidade**: ✅ Disponível via Contracts

#### GetCategories
- **Arquivo**: `Ecommerce.Modules.Catalog.Application/Queries/GetCategories/GetCategoriesQuery.cs`
- **Handler**: `GetCategoriesQueryHandler.cs`
- **Descrição**: Lista categorias hierárquicas
- **Retorno**: `List<CategoryDto>`

#### GetProductReviews
- **Arquivo**: `Ecommerce.Modules.Catalog.Application/Queries/GetProductReviews/GetProductReviewsQuery.cs`
- **Handler**: `GetProductReviewsQueryHandler.cs`
- **Descrição**: Lista avaliações de um produto
- **Retorno**: `PagedList<ReviewDto>`

### Domain Events (Internos)

#### ProductCreatedEvent
- **Arquivo**: `Ecommerce.Modules.Catalog.Core/Domain/Events/ProductCreatedEvent.cs`
- **Quando**: Após criar novo produto
- **Ação**: Indexar no ElasticSearch, atualizar cache

#### ProductPriceChangedEvent
- **Arquivo**: `Ecommerce.Modules.Catalog.Core/Domain/Events/ProductPriceChangedEvent.cs`
- **Quando**: Quando preço do produto muda
- **Ação**: Atualizar índices, invalidar cache, notificar sistemas externos

#### StockReservedEvent
- **Arquivo**: `Ecommerce.Modules.Catalog.Core/Domain/Events/StockReservedEvent.cs`
- **Quando**: Quando estoque é reservado
- **Ação**: Atualizar contadores de estoque disponível

#### ReviewApprovedEvent
- **Arquivo**: `Ecommerce.Modules.Catalog.Core/Domain/Events/ReviewApprovedEvent.cs`
- **Quando**: Quando avaliação é aprovada
- **Ação**: Recalcular rating médio do produto

### Integration Events (Públicos)

#### ProductCreatedIntegrationEvent
- **Arquivo**: `Ecommerce.Modules.Catalog.Contracts/Events/ProductCreatedIntegrationEvent.cs`
- **Publicado**: Quando novo produto é criado
- **Consumido por**: Módulos que precisam reagir a novos produtos
- **Payload**:
  - `ProductId: Guid`
  - `Name: string`
  - `Sku: string`
  - `Price: decimal`
  - `CategoryId: Guid`
  - `CreatedAt: DateTime`

#### StockChangedIntegrationEvent
- **Arquivo**: `Ecommerce.Modules.Catalog.Contracts/Events/StockChangedIntegrationEvent.cs`
- **Publicado**: Quando estoque do produto muda
- **Consumido por**: Cart (validar disponibilidade), Orders (verificar ruptura)
- **Payload**:
  - `ProductId: Guid`
  - `PreviousStock: int`
  - `NewStock: int`
  - `AvailableStock: int`
  - `Reason: string`
  - `ChangedAt: DateTime`

---

## Módulo Cart

### Commands

#### AddItemToCart
- **Arquivo**: `Ecommerce.Modules.Cart.Application/Commands/AddItemToCart/AddItemToCartCommand.cs`
- **Handler**: `AddItemToCartCommandHandler.cs`
- **Validator**: `AddItemToCartCommandValidator.cs`
- **Descrição**: Adiciona item ao carrinho
- **Dependências**: Consulta `GetProductByIdQuery` via Mediator
- **Retorno**: `Result<CartDto>`

#### RemoveItemFromCart
- **Arquivo**: `Ecommerce.Modules.Cart.Application/Commands/RemoveItemFromCart/RemoveItemFromCartCommand.cs`
- **Handler**: `RemoveItemFromCartCommandHandler.cs`
- **Validator**: `RemoveItemFromCartCommandValidator.cs`
- **Descrição**: Remove item do carrinho
- **Retorno**: `Result<CartDto>`

#### UpdateItemQuantity
- **Arquivo**: `Ecommerce.Modules.Cart.Application/Commands/UpdateItemQuantity/UpdateItemQuantityCommand.cs`
- **Handler**: `UpdateItemQuantityCommandHandler.cs`
- **Validator**: `UpdateItemQuantityCommandValidator.cs`
- **Descrição**: Atualiza quantidade de um item
- **Retorno**: `Result<CartDto>`

#### ApplyCoupon
- **Arquivo**: `Ecommerce.Modules.Cart.Application/Commands/ApplyCoupon/ApplyCouponCommand.cs`
- **Handler**: `ApplyCouponCommandHandler.cs`
- **Validator**: `ApplyCouponCommandValidator.cs`
- **Descrição**: Aplica cupom de desconto
- **Dependências**: `ValidateCouponCommand` via Mediator (Coupons module)
- **Retorno**: `Result<CartDto>`

#### MergeCart
- **Arquivo**: `Ecommerce.Modules.Cart.Application/Commands/MergeCart/MergeCartCommand.cs`
- **Handler**: `MergeCartCommandHandler.cs`
- **Validator**: `MergeCartCommandValidator.cs`
- **Descrição**: Mescla carrinho anônimo com carrinho do usuário logado
- **Retorno**: `Result<CartDto>`

### Queries

#### GetCartById
- **Arquivo**: `Ecommerce.Modules.Cart.Application/Queries/GetCartById/GetCartByIdQuery.cs`
- **Handler**: `GetCartByIdQueryHandler.cs`
- **Descrição**: Busca carrinho por ID
- **Retorno**: `CartDto`

#### GetAbandonedCarts
- **Arquivo**: `Ecommerce.Modules.Cart.Application/Queries/GetAbandonedCarts/GetAbandonedCartsQuery.cs`
- **Handler**: `GetAbandonedCartsQueryHandler.cs`
- **Descrição**: Lista carrinhos abandonados para campanhas
- **Retorno**: `PagedList<AbandonedCartDto>`

### Domain Events (Internos)

#### ItemAddedToCartEvent
- **Arquivo**: `Ecommerce.Modules.Cart.Core/Domain/Events/ItemAddedToCartEvent.cs`
- **Quando**: Quando item é adicionado ao carrinho
- **Ação**: Log para analytics, verificar disponibilidade

#### CartAbandonedEvent
- **Arquivo**: `Ecommerce.Modules.Cart.Core/Domain/Events/CartAbandonedEvent.cs`
- **Quando**: Quando carrinho fica inativo por período configurado
- **Ação**: Criar campanha de remarketing

#### CartConvertedEvent
- **Arquivo**: `Ecommerce.Modules.Cart.Core/Domain/Events/CartConvertedEvent.cs`
- **Quando**: Quando carrinho é convertido em pedido
- **Ação**: Liberar reservas temporárias, marcar como convertido

### Integration Events (Públicos)

#### CartConvertedIntegrationEvent
- **Arquivo**: `Ecommerce.Modules.Cart.Contracts/Events/CartConvertedIntegrationEvent.cs`
- **Publicado**: Quando carrinho é convertido em pedido
- **Consumido por**: Catalog (confirmar reservas), Analytics
- **Payload**:
  - `CartId: Guid`
  - `OrderId: Guid`
  - `UserId: Guid`
  - `TotalAmount: decimal`
  - `ItemsCount: int`
  - `ConvertedAt: DateTime`

---

## Módulo Orders

### Commands

#### CreateOrder
- **Arquivo**: `Ecommerce.Modules.Orders.Application/Commands/CreateOrder/CreateOrderCommand.cs`
- **Handler**: `CreateOrderCommandHandler.cs`
- **Validator**: `CreateOrderCommandValidator.cs`
- **Descrição**: Cria novo pedido a partir do carrinho
- **Dependências**:
  - `GetUserAddressQuery` via Mediator (Users module)
  - `GetProductByIdQuery` via Mediator (Catalog module)
  - `ReserveStockCommand` via Mediator (Catalog module)
- **Retorno**: `Result<OrderDto>`
- **Snapshot**: Salva endereço e dados do produto

#### CancelOrder
- **Arquivo**: `Ecommerce.Modules.Orders.Application/Commands/CancelOrder/CancelOrderCommand.cs`
- **Handler**: `CancelOrderCommandHandler.cs`
- **Validator**: `CancelOrderCommandValidator.cs`
- **Descrição**: Cancela pedido
- **Retorno**: `Result`

#### UpdateOrderStatus
- **Arquivo**: `Ecommerce.Modules.Orders.Application/Commands/UpdateOrderStatus/UpdateOrderStatusCommand.cs`
- **Handler**: `UpdateOrderStatusCommandHandler.cs`
- **Validator**: `UpdateOrderStatusCommandValidator.cs`
- **Descrição**: Atualiza status do pedido
- **Retorno**: `Result<OrderDto>`

#### ProcessRefund
- **Arquivo**: `Ecommerce.Modules.Orders.Application/Commands/ProcessRefund/ProcessRefundCommand.cs`
- **Handler**: `ProcessRefundCommandHandler.cs`
- **Validator**: `ProcessRefundCommandValidator.cs`
- **Descrição**: Processa reembolso
- **Retorno**: `Result<RefundDto>`

### Queries

#### GetOrderById
- **Arquivo**: `Ecommerce.Modules.Orders.Application/Queries/GetOrderById/GetOrderByIdQuery.cs`
- **Handler**: `GetOrderByIdQueryHandler.cs`
- **Descrição**: Busca pedido por ID
- **Retorno**: `OrderDto`

#### GetUserOrders
- **Arquivo**: `Ecommerce.Modules.Orders.Application/Queries/GetUserOrders/GetUserOrdersQuery.cs`
- **Handler**: `GetUserOrdersQueryHandler.cs`
- **Descrição**: Lista pedidos do usuário
- **Retorno**: `PagedList<OrderDto>`

#### GetOrderTracking
- **Arquivo**: `Ecommerce.Modules.Orders.Application/Queries/GetOrderTracking/GetOrderTrackingQuery.cs`
- **Handler**: `GetOrderTrackingQueryHandler.cs`
- **Descrição**: Rastreamento do pedido
- **Retorno**: `TrackingDto`

### Domain Events (Internos)

#### OrderCreatedEvent
- **Arquivo**: `Ecommerce.Modules.Orders.Core/Domain/Events/OrderCreatedEvent.cs`
- **Quando**: Após criar novo pedido
- **Ação**: Enviar confirmação por email

#### OrderPaidEvent
- **Arquivo**: `Ecommerce.Modules.Orders.Core/Domain/Events/OrderPaidEvent.cs`
- **Quando**: Quando pagamento é confirmado
- **Ação**: Atualizar status para PAID, iniciar preparação

#### OrderShippedEvent
- **Arquivo**: `Ecommerce.Modules.Orders.Core/Domain/Events/OrderShippedEvent.cs`
- **Quando**: Quando pedido é enviado
- **Ação**: Enviar código de rastreamento, notificar usuário

#### OrderCancelledEvent
- **Arquivo**: `Ecommerce.Modules.Orders.Core/Domain/Events/OrderCancelledEvent.cs`
- **Quando**: Quando pedido é cancelado
- **Ação**: Liberar estoque, processar reembolso se pago

### Integration Events (Públicos)

#### OrderCreatedIntegrationEvent
- **Arquivo**: `Ecommerce.Modules.Orders.Contracts/Events/OrderCreatedIntegrationEvent.cs`
- **Publicado**: Quando novo pedido é criado
- **Consumido por**: Payments (criar cobrança)
- **Payload**:
  - `OrderId: Guid`
  - `OrderNumber: string`
  - `UserId: Guid`
  - `TotalAmount: decimal`
  - `Items: List<OrderItemDto>`
  - `CreatedAt: DateTime`

#### OrderPaidIntegrationEvent
- **Arquivo**: `Ecommerce.Modules.Orders.Contracts/Events/OrderPaidIntegrationEvent.cs`
- **Publicado**: Quando pedido é marcado como pago
- **Consumido por**: Catalog (confirmar saída de estoque), Analytics
- **Payload**:
  - `OrderId: Guid`
  - `OrderNumber: string`
  - `PaidAmount: decimal`
  - `PaymentMethod: string`
  - `PaidAt: DateTime`

### Integration Event Handlers (Consumidores)

#### PaymentCapturedIntegrationEventHandler
- **Arquivo**: `Ecommerce.Modules.Orders.Application/IntegrationEventHandlers/PaymentCapturedIntegrationEventHandler.cs`
- **Consome**: `PaymentCapturedIntegrationEvent` (do módulo Payments)
- **Ação**: Marca pedido como pago, atualiza status
- **Idempotência**: Verifica se pedido já foi marcado como pago

---

## Módulo Payments

### Commands

#### ProcessPayment
- **Arquivo**: `Ecommerce.Modules.Payments.Application/Commands/ProcessPayment/ProcessPaymentCommand.cs`
- **Handler**: `ProcessPaymentCommandHandler.cs`
- **Validator**: `ProcessPaymentCommandValidator.cs`
- **Descrição**: Processa pagamento
- **Params**:
  - `OrderId: Guid`
  - `PaymentMethodType: PaymentMethodType`
  - `Amount: decimal`
  - `PaymentDetails: PaymentDetailsDto`
- **Retorno**: `Result<PaymentDto>`

#### CapturePayment
- **Arquivo**: `Ecommerce.Modules.Payments.Application/Commands/CapturePayment/CapturePaymentCommand.cs`
- **Handler**: `CapturePaymentCommandHandler.cs`
- **Validator**: `CapturePaymentCommandValidator.cs`
- **Descrição**: Captura pagamento autorizado
- **Retorno**: `Result<PaymentDto>`

#### RefundPayment
- **Arquivo**: `Ecommerce.Modules.Payments.Application/Commands/RefundPayment/RefundPaymentCommand.cs`
- **Handler**: `RefundPaymentCommandHandler.cs`
- **Validator**: `RefundPaymentCommandValidator.cs`
- **Descrição**: Processa reembolso
- **Retorno**: `Result<RefundDto>`

#### SavePaymentMethod
- **Arquivo**: `Ecommerce.Modules.Payments.Application/Commands/SavePaymentMethod/SavePaymentMethodCommand.cs`
- **Handler**: `SavePaymentMethodCommandHandler.cs`
- **Validator**: `SavePaymentMethodCommandValidator.cs`
- **Descrição**: Salva método de pagamento tokenizado
- **Retorno**: `Result<PaymentMethodDto>`

### Queries

#### GetPaymentById
- **Arquivo**: `Ecommerce.Modules.Payments.Application/Queries/GetPaymentById/GetPaymentByIdQuery.cs`
- **Handler**: `GetPaymentByIdQueryHandler.cs`
- **Descrição**: Busca pagamento por ID
- **Retorno**: `PaymentDto`

#### GetUserPaymentMethods
- **Arquivo**: `Ecommerce.Modules.Payments.Application/Queries/GetUserPaymentMethods/GetUserPaymentMethodsQuery.cs`
- **Handler**: `GetUserPaymentMethodsQueryHandler.cs`
- **Descrição**: Lista métodos de pagamento salvos do usuário
- **Retorno**: `List<PaymentMethodDto>`

### Domain Events (Internos)

#### PaymentAuthorizedEvent
- **Arquivo**: `Ecommerce.Modules.Payments.Core/Domain/Events/PaymentAuthorizedEvent.cs`
- **Quando**: Quando pagamento é autorizado
- **Ação**: Criar transação, preparar para captura

#### PaymentCapturedEvent
- **Arquivo**: `Ecommerce.Modules.Payments.Core/Domain/Events/PaymentCapturedEvent.cs`
- **Quando**: Quando pagamento é capturado
- **Ação**: Publicar evento de integração para Orders

#### PaymentFailedEvent
- **Arquivo**: `Ecommerce.Modules.Payments.Core/Domain/Events/PaymentFailedEvent.cs`
- **Quando**: Quando pagamento falha
- **Ação**: Notificar usuário, log de erro

#### RefundProcessedEvent
- **Arquivo**: `Ecommerce.Modules.Payments.Core/Domain/Events/RefundProcessedEvent.cs`
- **Quando**: Quando reembolso é processado
- **Ação**: Atualizar status, notificar usuário

### Integration Events (Públicos)

#### PaymentAuthorizedIntegrationEvent
- **Arquivo**: `Ecommerce.Modules.Payments.Contracts/Events/PaymentAuthorizedIntegrationEvent.cs`
- **Publicado**: Quando pagamento é autorizado
- **Consumido por**: Orders (atualizar status para PAYMENT_PROCESSING)
- **Payload**:
  - `PaymentId: Guid`
  - `OrderId: Guid`
  - `Amount: decimal`
  - `PaymentMethod: string`
  - `AuthorizedAt: DateTime`

#### PaymentCapturedIntegrationEvent
- **Arquivo**: `Ecommerce.Modules.Payments.Contracts/Events/PaymentCapturedIntegrationEvent.cs`
- **Publicado**: Quando pagamento é capturado com sucesso
- **Consumido por**: Orders (marcar como pago)
- **Payload**:
  - `PaymentId: Guid`
  - `OrderId: Guid`
  - `Amount: decimal`
  - `PaymentMethod: string`
  - `CapturedAt: DateTime`

### Integration Event Handlers (Consumidores)

#### OrderCreatedIntegrationEventHandler
- **Arquivo**: `Ecommerce.Modules.Payments.Application/IntegrationEventHandlers/OrderCreatedIntegrationEventHandler.cs`
- **Consome**: `OrderCreatedIntegrationEvent` (do módulo Orders)
- **Ação**: Criar registro de pagamento pendente, aguardar processamento

---

## Módulo Coupons

### Commands

#### CreateCoupon
- **Descrição**: Cria novo cupom de desconto
- **Retorno**: `Result<CouponDto>`

#### UpdateCoupon
- **Descrição**: Atualiza cupom existente
- **Retorno**: `Result<CouponDto>`

#### ValidateCoupon
- **Arquivo**: `Ecommerce.Modules.Coupons.Application/Commands/ValidateCoupon/ValidateCouponCommand.cs`
- **Handler**: `ValidateCouponCommandHandler.cs`
- **Descrição**: Valida cupom para aplicação no carrinho
- **Params**:
  - `CouponCode: string`
  - `UserId: Guid?`
  - `CartSubtotal: decimal`
  - `Items: List<CartItemDto>`
- **Retorno**: `CouponValidationResult`
- **DTOs Públicos**:
  ```csharp
  public record CouponValidationResult(
      bool IsValid,
      string ErrorCode,
      string ErrorMessage,
      decimal DiscountAmount,
      Guid? CouponId
  );

  public record CartItemDto(
      Guid ProductId,
      Guid CategoryId,
      int Quantity,
      decimal UnitPrice
  );
  ```
- **Validações**:
  - Status ativo
  - Período de validade
  - Limite global de uso
  - Limite por usuário
  - Valor mínimo de compra
  - Elegibilidade (produtos/categorias/usuários)
- **Visibilidade**: ✅ Disponível via Contracts para Cart

#### DeactivateCoupon
- **Descrição**: Desativa cupom
- **Retorno**: `Result`

### Queries

#### GetCouponById
- **Descrição**: Busca cupom por ID
- **Retorno**: `CouponDto`

#### GetActiveCoupons
- **Descrição**: Lista cupons ativos e disponíveis
- **Retorno**: `PagedList<CouponDto>`

#### GetCouponMetrics
- **Descrição**: Métricas de uso do cupom
- **Retorno**: `CouponMetricsDto`

### Domain Events (Internos)

#### CouponCreatedEvent
- **Quando**: Quando cupom é criado
- **Ação**: Agendar ativação se necessário

#### CouponUsedEvent
- **Quando**: Quando cupom é usado em uma compra
- **Ação**: Incrementar contador de uso, verificar limite

#### CouponExpiredEvent
- **Quando**: Quando cupom expira
- **Ação**: Atualizar status, remover de caches

---

## Eventos de Integração (Cross-Module)

### Fluxo: Checkout Completo

```
1. Cart → Catalog
   - Command: ReserveStockCommand
   - Ação: Reservar estoque temporariamente

2. Cart → Coupons
   - Command: ValidateCouponCommand
   - Ação: Validar e aplicar cupom

3. Cart → Orders
   - Trigger: Usuário confirma checkout
   - Event: CartConvertedIntegrationEvent
   - Ação: Criar pedido

4. Orders → Users
   - Query: GetUserAddressQuery
   - Ação: Obter snapshot do endereço

5. Orders → Catalog
   - Query: GetProductByIdQuery
   - Ação: Obter snapshot do produto

6. Orders → Payments
   - Event: OrderCreatedIntegrationEvent
   - Ação: Criar registro de pagamento

7. Payments → Gateway Externo
   - Ação: Processar pagamento

8. Gateway → Payments (Webhook)
   - Ação: Confirmar captura

9. Payments → Orders
   - Event: PaymentCapturedIntegrationEvent
   - Ação: Marcar pedido como pago

10. Orders → Catalog
    - Event: OrderPaidIntegrationEvent
    - Ação: Confirmar saída de estoque

11. Orders → Users
    - Ação: Enviar notificação de pedido confirmado
```

### Fluxo: Cancelamento de Pedido

```
1. Orders → Payments
   - Command: RefundPaymentCommand (se pago)
   - Ação: Processar reembolso

2. Orders → Catalog
   - Command: ReleaseStockCommand
   - Ação: Devolver estoque

3. Orders → Users
   - Ação: Notificar cancelamento

4. Payments → Orders
   - Event: RefundProcessedIntegrationEvent
   - Ação: Confirmar reembolso
```

### Fluxo: Carrinho Abandonado

```
1. Cart (Background Job)
   - Detecta carrinhos inativos > 1 hora
   - Event: CartAbandonedEvent

2. Cart → Users
   - Ação: Enviar email de remarketing

3. Cart → Catalog
   - Command: ReleaseStockCommand
   - Ação: Liberar reservas temporárias
```

---

## Regras de Comunicação

### ✅ Permitido

1. **Consulta via Mediator**
   - Módulos podem consultar dados via queries públicas
   - Exemplo: `Orders` consulta `GetProductByIdQuery` do `Catalog`

2. **Comandos via Mediator**
   - Módulos podem executar ações via commands públicos
   - Exemplo: `Cart` executa `ReserveStockCommand` do `Catalog`

3. **Eventos de Integração**
   - Módulos se comunicam via eventos assíncronos
   - Exemplo: `Payments` publica `PaymentCapturedIntegrationEvent`
   - `Orders` consome e reage ao evento

4. **Contratos Públicos**
   - Módulos dependem apenas de `.Contracts` de outros módulos
   - DTOs e interfaces públicas bem definidas

### ❌ Proibido

1. **Acoplamento Direto**
   - ❌ `Orders.Application` referenciar `Catalog.Core`
   - ❌ `Orders.Application` referenciar `Catalog.Infrastructure`
   - ❌ Acessar `IProductRepository` diretamente de outro módulo

2. **Transações Distribuídas**
   - ❌ Iniciar transação no `Orders` e commitar no `Catalog`
   - ✅ Cada módulo gerencia sua própria transação

3. **Referências a Entidades**
   - ❌ `Orders` referenciar classe `Product` do `Catalog`
   - ✅ Usar snapshots (cópias em JSONB)

### Padrões de Snapshot

Quando módulos precisam manter dados históricos que podem mudar:

```csharp
// ❌ ERRADO - Referência direta
public class OrderItem
{
    public Product Product { get; set; } // Muda se produto for atualizado
}

// ✅ CORRETO - Snapshot em JSONB
public class OrderItem
{
    public Guid ProductId { get; set; }
    public JsonDocument ProductSnapshot { get; set; } // Dados históricos
}
```

**Quando usar Snapshot:**
- Orders salvando dados de Product
- Orders salvando dados de Address
- Cart salvando dados de Product

### Event Bus (Outbox Pattern)

Todos os eventos de integração passam pelo Event Bus:

```csharp
// Publicação
await _eventBus.PublishAsync(new PaymentCapturedIntegrationEvent(...));

// Event Bus salva no Outbox
// Background Job processa outbox
// Handlers registrados são chamados via MediatR
```

**Vantagens:**
- Confiabilidade (eventos não se perdem)
- Auditoria (log de todos os eventos)
- Retry automático
- Idempotência (Inbox Pattern previne duplicação)

---

## Resumo por Tipo

### Commands (Write Operations)

| Módulo   | Total | Públicos (via Contracts) |
|----------|-------|--------------------------|
| Users    | 3     | 0                        |
| Catalog  | 4     | 2 (ReserveStock, ReleaseStock) |
| Cart     | 5     | 0                        |
| Orders   | 4     | 0                        |
| Payments | 4     | 0                        |
| Coupons  | 4     | 1 (ValidateCoupon)       |
| **Total**| **24**| **3**                    |

### Queries (Read Operations)

| Módulo   | Total | Públicos (via Contracts) |
|----------|-------|--------------------------|
| Users    | 4     | 4 (todos)                |
| Catalog  | 4     | 4 (todos)                |
| Cart     | 2     | 0                        |
| Orders   | 3     | 0                        |
| Payments | 2     | 0                        |
| Coupons  | 3     | 0                        |
| **Total**| **18**| **8**                    |

### Domain Events (Internos)

| Módulo   | Total |
|----------|-------|
| Users    | 4     |
| Catalog  | 4     |
| Cart     | 3     |
| Orders   | 4     |
| Payments | 4     |
| Coupons  | 3     |
| **Total**| **22**|

### Integration Events (Cross-Module)

| Módulo   | Publicados | Consumidos |
|----------|------------|------------|
| Users    | 2          | 0          |
| Catalog  | 2          | 1          |
| Cart     | 1          | 0          |
| Orders   | 2          | 1          |
| Payments | 2          | 1          |
| Coupons  | 0          | 0          |
| **Total**| **9**      | **3**      |

---

**Última atualização**: 2025-12-13
**Versão**: 1.0
