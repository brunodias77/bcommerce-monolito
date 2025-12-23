# Sistema Completo de Commands e Queries - E-commerce & Admin

> **Projeto:** bcommerce-monolito
> **Arquitetura:** CQRS + DDD + Modular Monolith
> **Framework:** .NET 8 + MediatR
> **Data:** 2025-12-23

---

## 📋 Índice

1. [Módulo Users (Autenticação & Usuários)](#1-módulo-users)
2. [Módulo Catalog (Catálogo de Produtos)](#2-módulo-catalog)
3. [Módulo Cart (Carrinho de Compras)](#3-módulo-cart)
4. [Módulo Orders (Pedidos)](#4-módulo-orders)
5. [Módulo Payments (Pagamentos)](#5-módulo-payments)
6. [Módulo Coupons (Cupons de Desconto)](#6-módulo-coupons)
7. [Módulo Notifications (Notificações)](#7-módulo-notifications)
8. [Módulo Analytics (Relatórios & Analytics)](#8-módulo-analytics)
9. [Módulo Admin (Administração)](#9-módulo-admin)
10. [Módulo Reviews (Avaliações)](#10-módulo-reviews)
11. [Módulo Inventory (Estoque)](#11-módulo-inventory)
12. [Módulo Shipping (Envio & Logística)](#12-módulo-shipping)

---

## 1. Módulo Users

### 1.1 Commands - Autenticação

```csharp
// Autenticação básica
public record RegisterUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber
) : ICommand<Guid>;

public record LoginCommand(
    string Email,
    string Password,
    bool RememberMe = false
) : ICommand<LoginResponse>;

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User
);

public record RefreshTokenCommand(
    string RefreshToken
) : ICommand<LoginResponse>;

public record LogoutCommand(
    Guid SessionId
) : ICommand;

public record RevokeAllSessionsCommand(
    Guid UserId
) : ICommand;

// Recuperação de senha
public record RequestPasswordResetCommand(
    string Email
) : ICommand;

public record ResetPasswordCommand(
    string Token,
    string NewPassword
) : ICommand;

public record ChangePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword
) : ICommand;

// Verificação de email
public record SendEmailVerificationCommand(
    Guid UserId
) : ICommand;

public record VerifyEmailCommand(
    string Token
) : ICommand;

// Autenticação em duas etapas
public record EnableTwoFactorAuthenticationCommand(
    Guid UserId
) : ICommand<TwoFactorSetupResponse>;

public record TwoFactorSetupResponse(
    string SecretKey,
    string QrCodeUrl,
    string[] BackupCodes
);

public record DisableTwoFactorAuthenticationCommand(
    Guid UserId,
    string Password
) : ICommand;

public record VerifyTwoFactorCodeCommand(
    Guid UserId,
    string Code
) : ICommand<LoginResponse>;

public record RegenerateTwoFactorBackupCodesCommand(
    Guid UserId
) : ICommand<string[]>;
```

### 1.2 Commands - Perfil do Usuário

```csharp
// Gerenciamento de perfil
public record UpdateUserProfileCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    DateTime? DateOfBirth,
    string? Gender,
    string? AvatarUrl
) : ICommand;

public record UpdateUserPreferencesCommand(
    Guid UserId,
    string Language,
    string Currency,
    bool EmailNotifications,
    bool SmsNotifications,
    bool PushNotifications,
    bool MarketingEmails
) : ICommand;

public record UploadUserAvatarCommand(
    Guid UserId,
    Stream ImageStream,
    string FileName,
    string ContentType
) : ICommand<string>; // Returns avatar URL

public record DeleteUserAvatarCommand(
    Guid UserId
) : ICommand;

// Endereços
public record AddUserAddressCommand(
    Guid UserId,
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State,
    string PostalCode,
    string Country,
    bool IsDefault = false
) : ICommand<Guid>;

public record UpdateUserAddressCommand(
    Guid AddressId,
    Guid UserId,
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State,
    string PostalCode,
    string Country,
    bool IsDefault
) : ICommand;

public record DeleteUserAddressCommand(
    Guid AddressId,
    Guid UserId
) : ICommand;

public record SetDefaultAddressCommand(
    Guid AddressId,
    Guid UserId
) : ICommand;

// Conta do usuário
public record DeactivateUserAccountCommand(
    Guid UserId,
    string Reason
) : ICommand;

public record ReactivateUserAccountCommand(
    Guid UserId
) : ICommand;

public record DeleteUserAccountCommand(
    Guid UserId,
    string Password,
    string Reason
) : ICommand;
```

### 1.3 Commands - Roles & Permissões (Admin)

```csharp
public record AssignRoleToUserCommand(
    Guid UserId,
    string RoleName
) : ICommand;

public record RemoveRoleFromUserCommand(
    Guid UserId,
    string RoleName
) : ICommand;

public record CreateRoleCommand(
    string Name,
    string Description,
    string[] Permissions
) : ICommand<Guid>;

public record UpdateRoleCommand(
    Guid RoleId,
    string Name,
    string Description,
    string[] Permissions
) : ICommand;

public record DeleteRoleCommand(
    Guid RoleId
) : ICommand;

public record AssignPermissionToRoleCommand(
    Guid RoleId,
    string PermissionName
) : ICommand;

public record RemovePermissionFromRoleCommand(
    Guid RoleId,
    string PermissionName
) : ICommand;
```

### 1.4 Queries - Autenticação & Usuários

```csharp
// Usuário
public record GetUserByIdQuery(Guid UserId) : IQuery<UserDto>;

public record GetUserByEmailQuery(string Email) : IQuery<UserDto>;

public record GetCurrentUserQuery(Guid UserId) : IQuery<UserProfileDto>;

public record GetUserSessionsQuery(Guid UserId) : IQuery<List<UserSessionDto>>;

public record ValidateTokenQuery(string Token) : IQuery<TokenValidationResult>;

// Endereços
public record GetUserAddressesQuery(Guid UserId) : IQuery<List<AddressDto>>;

public record GetUserDefaultAddressQuery(Guid UserId) : IQuery<AddressDto>;

public record GetAddressByIdQuery(Guid AddressId) : IQuery<AddressDto>;

// Preferências
public record GetUserPreferencesQuery(Guid UserId) : IQuery<UserPreferencesDto>;

// Admin - Listagem de usuários
public record GetUsersQuery(
    int Page = 1,
    int PageSize = 20,
    string? SearchTerm = null,
    string? Role = null,
    bool? IsActive = null,
    DateTime? RegisteredAfter = null,
    DateTime? RegisteredBefore = null,
    string SortBy = "CreatedAt",
    string SortOrder = "Desc"
) : IQuery<PagedResult<UserDto>>;

public record GetUserRolesQuery(Guid UserId) : IQuery<List<RoleDto>>;

public record GetUserPermissionsQuery(Guid UserId) : IQuery<List<string>>;

public record GetAllRolesQuery() : IQuery<List<RoleDto>>;

public record GetRoleByIdQuery(Guid RoleId) : IQuery<RoleDto>;

public record GetRolePermissionsQuery(Guid RoleId) : IQuery<List<string>>;
```

---

## 2. Módulo Catalog

### 2.1 Commands - Produtos

```csharp
// Criar e gerenciar produtos
public record CreateProductCommand(
    string Name,
    string Description,
    string Sku,
    decimal Price,
    string Currency,
    Guid CategoryId,
    Guid? BrandId,
    int InitialStock,
    ProductDimensionsDto? Dimensions,
    decimal? Weight,
    List<string>? Tags,
    Dictionary<string, string>? Attributes
) : ICommand<Guid>;

public record UpdateProductCommand(
    Guid ProductId,
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    Guid CategoryId,
    Guid? BrandId,
    ProductDimensionsDto? Dimensions,
    decimal? Weight,
    List<string>? Tags,
    Dictionary<string, string>? Attributes
) : ICommand;

public record UpdateProductSkuCommand(
    Guid ProductId,
    string NewSku
) : ICommand;

public record UpdateProductPriceCommand(
    Guid ProductId,
    decimal NewPrice,
    string Currency
) : ICommand;

public record DeleteProductCommand(
    Guid ProductId,
    string Reason
) : ICommand;

public record RestoreProductCommand(
    Guid ProductId
) : ICommand;

// Status do produto
public record PublishProductCommand(
    Guid ProductId
) : ICommand;

public record UnpublishProductCommand(
    Guid ProductId
) : ICommand;

public record ArchiveProductCommand(
    Guid ProductId
) : ICommand;

// Imagens do produto
public record AddProductImageCommand(
    Guid ProductId,
    Stream ImageStream,
    string FileName,
    string ContentType,
    bool IsPrimary = false,
    int DisplayOrder = 0
) : ICommand<Guid>;

public record UpdateProductImageOrderCommand(
    Guid ProductId,
    List<ImageOrderDto> ImageOrders
) : ICommand;

public record ImageOrderDto(Guid ImageId, int DisplayOrder);

public record SetPrimaryProductImageCommand(
    Guid ProductId,
    Guid ImageId
) : ICommand;

public record DeleteProductImageCommand(
    Guid ProductId,
    Guid ImageId
) : ICommand;

// Variantes de produto
public record CreateProductVariantCommand(
    Guid ProductId,
    string Sku,
    string Name,
    decimal? PriceAdjustment,
    Dictionary<string, string> Attributes, // e.g., {"Color": "Red", "Size": "M"}
    int Stock
) : ICommand<Guid>;

public record UpdateProductVariantCommand(
    Guid VariantId,
    string Name,
    decimal? PriceAdjustment,
    Dictionary<string, string> Attributes,
    int Stock
) : ICommand;

public record DeleteProductVariantCommand(
    Guid VariantId
) : ICommand;

// Bulk operations
public record BulkUpdateProductPricesCommand(
    List<ProductPriceUpdateDto> Updates
) : ICommand;

public record ProductPriceUpdateDto(Guid ProductId, decimal NewPrice);

public record BulkUpdateProductStockCommand(
    List<ProductStockUpdateDto> Updates
) : ICommand;

public record ProductStockUpdateDto(Guid ProductId, int Quantity, string Operation); // Add, Set, Subtract
```

### 2.2 Commands - Categorias

```csharp
public record CreateCategoryCommand(
    string Name,
    string Description,
    string Slug,
    Guid? ParentCategoryId,
    int DisplayOrder = 0,
    string? ImageUrl = null,
    string? IconUrl = null,
    Dictionary<string, string>? Metadata = null
) : ICommand<Guid>;

public record UpdateCategoryCommand(
    Guid CategoryId,
    string Name,
    string Description,
    string Slug,
    Guid? ParentCategoryId,
    int DisplayOrder,
    string? ImageUrl,
    string? IconUrl,
    Dictionary<string, string>? Metadata
) : ICommand;

public record DeleteCategoryCommand(
    Guid CategoryId,
    Guid? MoveProductsToCategoryId = null
) : ICommand;

public record ReorderCategoriesCommand(
    List<CategoryOrderDto> CategoryOrders
) : ICommand;

public record CategoryOrderDto(Guid CategoryId, int DisplayOrder, Guid? ParentId);

public record UploadCategoryImageCommand(
    Guid CategoryId,
    Stream ImageStream,
    string FileName,
    string ContentType
) : ICommand<string>;
```

### 2.3 Commands - Marcas (Brands)

```csharp
public record CreateBrandCommand(
    string Name,
    string? Description,
    string Slug,
    string? LogoUrl,
    string? WebsiteUrl,
    Dictionary<string, string>? Metadata
) : ICommand<Guid>;

public record UpdateBrandCommand(
    Guid BrandId,
    string Name,
    string? Description,
    string Slug,
    string? LogoUrl,
    string? WebsiteUrl,
    Dictionary<string, string>? Metadata
) : ICommand;

public record DeleteBrandCommand(
    Guid BrandId
) : ICommand;

public record UploadBrandLogoCommand(
    Guid BrandId,
    Stream ImageStream,
    string FileName,
    string ContentType
) : ICommand<string>;
```

### 2.4 Queries - Produtos

```csharp
// Busca de produtos
public record GetProductByIdQuery(Guid ProductId) : IQuery<ProductDto>;

public record GetProductBySkuQuery(string Sku) : IQuery<ProductDto>;

public record GetProductBySlugQuery(string Slug) : IQuery<ProductDto>;

public record SearchProductsQuery(
    string? SearchTerm = null,
    Guid? CategoryId = null,
    Guid? BrandId = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    List<string>? Tags = null,
    Dictionary<string, List<string>>? Filters = null, // e.g., {"Color": ["Red", "Blue"], "Size": ["M", "L"]}
    ProductStatus? Status = null,
    bool? InStock = null,
    int Page = 1,
    int PageSize = 20,
    string SortBy = "CreatedAt",
    string SortOrder = "Desc"
) : IQuery<PagedResult<ProductListItemDto>>;

public record GetFeaturedProductsQuery(
    int Limit = 10
) : IQuery<List<ProductListItemDto>>;

public record GetNewArrivalsQuery(
    int Days = 30,
    int Limit = 20
) : IQuery<List<ProductListItemDto>>;

public record GetBestSellersQuery(
    int Days = 30,
    int Limit = 20
) : IQuery<List<ProductListItemDto>>;

public record GetRelatedProductsQuery(
    Guid ProductId,
    int Limit = 10
) : IQuery<List<ProductListItemDto>>;

public record GetProductsByIdsQuery(
    List<Guid> ProductIds
) : IQuery<List<ProductDto>>;

// Imagens
public record GetProductImagesQuery(
    Guid ProductId
) : IQuery<List<ProductImageDto>>;

// Variantes
public record GetProductVariantsQuery(
    Guid ProductId
) : IQuery<List<ProductVariantDto>>;

// Disponibilidade
public record CheckProductAvailabilityQuery(
    Guid ProductId,
    int Quantity = 1
) : IQuery<ProductAvailabilityDto>;

public record GetProductStockQuery(
    Guid ProductId
) : IQuery<StockDto>;

// Estatísticas (Admin)
public record GetProductStatisticsQuery(
    Guid ProductId,
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IQuery<ProductStatisticsDto>;

public record GetLowStockProductsQuery(
    int Threshold = 10
) : IQuery<List<ProductStockAlertDto>>;

public record GetOutOfStockProductsQuery() : IQuery<List<ProductListItemDto>>;
```

### 2.5 Queries - Categorias & Marcas

```csharp
// Categorias
public record GetCategoryByIdQuery(Guid CategoryId) : IQuery<CategoryDto>;

public record GetCategoryBySlugQuery(string Slug) : IQuery<CategoryDto>;

public record GetAllCategoriesQuery(
    bool IncludeInactive = false
) : IQuery<List<CategoryDto>>;

public record GetCategoryTreeQuery() : IQuery<List<CategoryTreeDto>>;

public record GetCategoryChildrenQuery(
    Guid? ParentCategoryId = null
) : IQuery<List<CategoryDto>>;

public record GetCategoryBreadcrumbQuery(
    Guid CategoryId
) : IQuery<List<CategoryBreadcrumbDto>>;

public record GetCategoryProductCountQuery(
    Guid CategoryId,
    bool IncludeSubcategories = true
) : IQuery<int>;

// Marcas
public record GetBrandByIdQuery(Guid BrandId) : IQuery<BrandDto>;

public record GetBrandBySlugQuery(string Slug) : IQuery<BrandDto>;

public record GetAllBrandsQuery(
    int Page = 1,
    int PageSize = 50,
    string SortBy = "Name",
    string SortOrder = "Asc"
) : IQuery<PagedResult<BrandDto>>;

public record GetBrandProductCountQuery(
    Guid BrandId
) : IQuery<int>;

public record GetPopularBrandsQuery(
    int Limit = 20
) : IQuery<List<BrandDto>>;
```

---

## 3. Módulo Cart

### 3.1 Commands

```csharp
// Gerenciar carrinho
public record CreateCartCommand(
    Guid UserId
) : ICommand<Guid>;

public record AddItemToCartCommand(
    Guid CartId,
    Guid ProductId,
    Guid? VariantId,
    int Quantity
) : ICommand;

public record UpdateCartItemQuantityCommand(
    Guid CartId,
    Guid CartItemId,
    int Quantity
) : ICommand;

public record RemoveItemFromCartCommand(
    Guid CartId,
    Guid CartItemId
) : ICommand;

public record ClearCartCommand(
    Guid CartId
) : ICommand;

public record MergeCartsCommand(
    Guid AnonymousCartId,
    Guid UserCartId
) : ICommand;

// Cupons no carrinho
public record ApplyCouponToCartCommand(
    Guid CartId,
    string CouponCode
) : ICommand<CartDto>;

public record RemoveCouponFromCartCommand(
    Guid CartId
) : ICommand;

// Salvar para depois
public record MoveItemToWishlistCommand(
    Guid CartId,
    Guid CartItemId
) : ICommand;

public record SaveCartForLaterCommand(
    Guid CartId
) : ICommand;

// Carrinho abandonado
public record SendAbandonedCartReminderCommand(
    Guid CartId
) : ICommand;
```

### 3.2 Queries

```csharp
public record GetCartByIdQuery(Guid CartId) : IQuery<CartDto>;

public record GetCartByUserIdQuery(Guid UserId) : IQuery<CartDto>;

public record GetCartTotalQuery(Guid CartId) : IQuery<CartTotalDto>;

public record ValidateCartQuery(Guid CartId) : IQuery<CartValidationResult>;

public record GetCartItemsCountQuery(Guid CartId) : IQuery<int>;

// Admin - Carrinhos abandonados
public record GetAbandonedCartsQuery(
    int MinutesInactive = 60,
    decimal? MinValue = null,
    int Page = 1,
    int PageSize = 20
) : IQuery<PagedResult<AbandonedCartDto>>;

public record GetAbandonedCartStatisticsQuery(
    DateTime StartDate,
    DateTime EndDate
) : IQuery<AbandonedCartStatisticsDto>;
```

---

## 4. Módulo Orders

### 4.1 Commands - Criação e Gerenciamento

```csharp
// Criar pedido
public record CreateOrderFromCartCommand(
    Guid CartId,
    Guid UserId,
    Guid ShippingAddressId,
    Guid? BillingAddressId,
    ShippingMethod ShippingMethod,
    string? CustomerNotes
) : ICommand<Guid>;

public record CreateManualOrderCommand(
    Guid UserId,
    List<OrderItemDto> Items,
    Guid ShippingAddressId,
    Guid? BillingAddressId,
    ShippingMethod ShippingMethod,
    decimal? DiscountAmount,
    string? AdminNotes
) : ICommand<Guid>;

// Atualizar pedido
public record UpdateOrderShippingAddressCommand(
    Guid OrderId,
    Guid ShippingAddressId
) : ICommand;

public record UpdateOrderBillingAddressCommand(
    Guid OrderId,
    Guid BillingAddressId
) : ICommand;

public record AddOrderNoteCommand(
    Guid OrderId,
    string Note,
    bool IsCustomerVisible = false
) : ICommand<Guid>;

public record UpdateOrderItemQuantityCommand(
    Guid OrderId,
    Guid OrderItemId,
    int NewQuantity
) : ICommand;

public record RemoveOrderItemCommand(
    Guid OrderId,
    Guid OrderItemId
) : ICommand;
```

### 4.2 Commands - Workflow de Status

```csharp
// Transições de status
public record ConfirmOrderCommand(
    Guid OrderId
) : ICommand;

public record MarkOrderAsPaidCommand(
    Guid OrderId,
    Guid PaymentId
) : ICommand;

public record ProcessOrderCommand(
    Guid OrderId
) : ICommand;

public record ShipOrderCommand(
    Guid OrderId,
    string TrackingCode,
    string Carrier,
    DateTime? EstimatedDeliveryDate = null
) : ICommand;

public record MarkOrderAsDeliveredCommand(
    Guid OrderId,
    DateTime DeliveredAt,
    string? ReceivedBy = null
) : ICommand;

public record CancelOrderCommand(
    Guid OrderId,
    string Reason,
    string? Notes = null,
    bool RefundPayment = true
) : ICommand;

public record HoldOrderCommand(
    Guid OrderId,
    string Reason
) : ICommand;

public record ResumeOrderCommand(
    Guid OrderId
) : ICommand;
```

### 4.3 Commands - Devoluções e Reembolsos

```csharp
public record CreateOrderRefundCommand(
    Guid OrderId,
    List<RefundItemDto> Items,
    string Reason,
    decimal? CustomAmount = null,
    bool RestockItems = true
) : ICommand<Guid>;

public record RefundItemDto(
    Guid OrderItemId,
    int Quantity,
    decimal? CustomAmount = null
);

public record ApproveRefundCommand(
    Guid RefundId,
    string? AdminNotes = null
) : ICommand;

public record RejectRefundCommand(
    Guid RefundId,
    string Reason
) : ICommand;

public record ProcessRefundPaymentCommand(
    Guid RefundId
) : ICommand;

// Devoluções (RMA)
public record CreateReturnRequestCommand(
    Guid OrderId,
    List<ReturnItemDto> Items,
    string Reason,
    string? CustomerNotes = null
) : ICommand<Guid>;

public record ReturnItemDto(Guid OrderItemId, int Quantity, string Condition);

public record ApproveReturnRequestCommand(
    Guid ReturnId,
    string? Instructions = null
) : ICommand;

public record RejectReturnRequestCommand(
    Guid ReturnId,
    string Reason
) : ICommand;

public record MarkReturnAsReceivedCommand(
    Guid ReturnId,
    ReturnConditionAssessment Assessment
) : ICommand;

public record ReturnConditionAssessment(
    bool ItemsMatchRequest,
    string Condition,
    string? Notes
);

public record CompleteReturnCommand(
    Guid ReturnId,
    bool IssueRefund = true,
    bool RestockItems = true
) : ICommand;
```

### 4.4 Commands - Faturamento

```csharp
public record GenerateInvoiceCommand(
    Guid OrderId
) : ICommand<Guid>;

public record SendInvoiceToCustomerCommand(
    Guid InvoiceId
) : ICommand;

public record MarkInvoiceAsPaidCommand(
    Guid InvoiceId,
    DateTime PaidAt
) : ICommand;

public record VoidInvoiceCommand(
    Guid InvoiceId,
    string Reason
) : ICommand;

public record GenerateCreditNoteCommand(
    Guid InvoiceId,
    Guid RefundId
) : ICommand<Guid>;
```

### 4.5 Queries - Pedidos

```csharp
// Busca de pedidos
public record GetOrderByIdQuery(Guid OrderId) : IQuery<OrderDto>;

public record GetOrderByNumberQuery(string OrderNumber) : IQuery<OrderDto>;

public record GetUserOrdersQuery(
    Guid UserId,
    OrderStatus? Status = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int Page = 1,
    int PageSize = 20
) : IQuery<PagedResult<OrderListItemDto>>;

public record SearchOrdersQuery(
    string? SearchTerm = null, // Order number, customer name, email
    OrderStatus? Status = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    decimal? MinTotal = null,
    decimal? MaxTotal = null,
    ShippingMethod? ShippingMethod = null,
    int Page = 1,
    int PageSize = 20,
    string SortBy = "CreatedAt",
    string SortOrder = "Desc"
) : IQuery<PagedResult<OrderListItemDto>>;

public record GetOrderItemsQuery(
    Guid OrderId
) : IQuery<List<OrderItemDto>>;

public record GetOrderNotesQuery(
    Guid OrderId
) : IQuery<List<OrderNoteDto>>;

public record GetOrderStatusHistoryQuery(
    Guid OrderId
) : IQuery<List<OrderStatusHistoryDto>>;

public record GetOrderTrackingQuery(
    Guid OrderId
) : IQuery<OrderTrackingDto>;

public record GetOrderTrackingByNumberQuery(
    string OrderNumber
) : IQuery<OrderTrackingDto>;

// Estatísticas
public record GetOrderStatisticsQuery(
    DateTime StartDate,
    DateTime EndDate,
    Guid? UserId = null
) : IQuery<OrderStatisticsDto>;

public record GetOrdersByStatusQuery(
    OrderStatus Status,
    int Page = 1,
    int PageSize = 20
) : IQuery<PagedResult<OrderListItemDto>>;

public record GetPendingOrdersQuery() : IQuery<List<OrderListItemDto>>;

public record GetOrdersRequiringAttentionQuery() : IQuery<List<OrderAlertDto>>;
```

### 4.6 Queries - Devoluções e Faturas

```csharp
// Devoluções
public record GetOrderRefundsQuery(
    Guid OrderId
) : IQuery<List<OrderRefundDto>>;

public record GetRefundByIdQuery(
    Guid RefundId
) : IQuery<OrderRefundDto>;

public record GetPendingRefundsQuery(
    int Page = 1,
    int PageSize = 20
) : IQuery<PagedResult<OrderRefundDto>>;

public record GetReturnRequestsQuery(
    ReturnStatus? Status = null,
    int Page = 1,
    int PageSize = 20
) : IQuery<PagedResult<ReturnRequestDto>>;

public record GetReturnRequestByIdQuery(
    Guid ReturnId
) : IQuery<ReturnRequestDto>;

// Faturas
public record GetOrderInvoiceQuery(
    Guid OrderId
) : IQuery<InvoiceDto>;

public record GetInvoiceByIdQuery(
    Guid InvoiceId
) : IQuery<InvoiceDto>;

public record GetInvoiceByNumberQuery(
    string InvoiceNumber
) : IQuery<InvoiceDto>;

public record GetUnpaidInvoicesQuery(
    int Page = 1,
    int PageSize = 20
) : IQuery<PagedResult<InvoiceDto>>;
```

---

## 5. Módulo Payments

### 5.1 Commands

```csharp
// Processar pagamento
public record CreatePaymentCommand(
    Guid OrderId,
    string PaymentMethodType, // CreditCard, DebitCard, Pix, Boleto
    PaymentDetailsDto PaymentDetails,
    decimal Amount,
    string Currency
) : ICommand<Guid>;

public record PaymentDetailsDto(
    string? CardToken = null,
    string? CardHolderName = null,
    string? CardLastFourDigits = null,
    string? CardBrand = null,
    int? InstallmentCount = null,
    Dictionary<string, string>? AdditionalData = null
);

public record ProcessPaymentCommand(
    Guid PaymentId
) : ICommand<PaymentResultDto>;

public record PaymentResultDto(
    bool IsSuccess,
    string Status,
    string? TransactionId,
    string? ErrorMessage,
    Dictionary<string, string>? AdditionalInfo
);

public record ConfirmPaymentCommand(
    Guid PaymentId,
    string TransactionId
) : ICommand;

public record CancelPaymentCommand(
    Guid PaymentId,
    string Reason
) : ICommand;

public record RefundPaymentCommand(
    Guid PaymentId,
    decimal Amount,
    string Reason
) : ICommand<Guid>;

public record CapturePaymentCommand(
    Guid PaymentId
) : ICommand;

// Métodos de pagamento do usuário
public record SavePaymentMethodCommand(
    Guid UserId,
    string PaymentMethodType,
    string Token,
    string LastFourDigits,
    string? CardBrand,
    string? CardHolderName,
    DateTime? ExpirationDate,
    bool IsDefault = false
) : ICommand<Guid>;

public record DeletePaymentMethodCommand(
    Guid PaymentMethodId,
    Guid UserId
) : ICommand;

public record SetDefaultPaymentMethodCommand(
    Guid PaymentMethodId,
    Guid UserId
) : ICommand;

// Webhooks de gateway
public record ProcessStripeWebhookCommand(
    string EventType,
    string Payload,
    string Signature
) : ICommand;

public record ProcessMercadoPagoWebhookCommand(
    string EventType,
    string Payload
) : ICommand;

// PIX
public record GeneratePixPaymentCommand(
    Guid OrderId,
    decimal Amount
) : ICommand<PixPaymentDto>;

public record PixPaymentDto(
    string QrCode,
    string QrCodeBase64,
    string PixKey,
    DateTime ExpiresAt
);

// Boleto
public record GenerateBoletoPaymentCommand(
    Guid OrderId,
    decimal Amount
) : ICommand<BoletoPaymentDto>;

public record BoletoPaymentDto(
    string BoletoUrl,
    string Barcode,
    DateTime DueDate
);
```

### 5.2 Queries

```csharp
public record GetPaymentByIdQuery(Guid PaymentId) : IQuery<PaymentDto>;

public record GetPaymentByOrderIdQuery(Guid OrderId) : IQuery<PaymentDto>;

public record GetPaymentByTransactionIdQuery(
    string TransactionId
) : IQuery<PaymentDto>;

public record GetUserPaymentMethodsQuery(
    Guid UserId
) : IQuery<List<PaymentMethodDto>>;

public record GetPaymentHistoryQuery(
    Guid UserId,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int Page = 1,
    int PageSize = 20
) : IQuery<PagedResult<PaymentDto>>;

public record GetPendingPaymentsQuery(
    int Page = 1,
    int PageSize = 20
) : IQuery<PagedResult<PaymentDto>>;

public record GetFailedPaymentsQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int Page = 1,
    int PageSize = 20
) : IQuery<PagedResult<PaymentDto>>;

public record GetPaymentStatisticsQuery(
    DateTime StartDate,
    DateTime EndDate
) : IQuery<PaymentStatisticsDto>;
```

---

## 6. Módulo Coupons

### 6.1 Commands

```csharp
public record CreateCouponCommand(
    string Code,
    string Description,
    CouponType Type, // Percentage, FixedAmount, FreeShipping
    decimal DiscountValue,
    string? Currency = null,
    DateTime? ValidFrom = null,
    DateTime? ValidUntil = null,
    int? UsageLimit = null,
    int? UsageLimitPerUser = null,
    decimal? MinimumOrderValue = null,
    List<Guid>? ApplicableProductIds = null,
    List<Guid>? ApplicableCategoryIds = null,
    List<Guid>? ExcludedProductIds = null,
    bool IsActive = true
) : ICommand<Guid>;

public record UpdateCouponCommand(
    Guid CouponId,
    string Description,
    CouponType Type,
    decimal DiscountValue,
    string? Currency,
    DateTime? ValidFrom,
    DateTime? ValidUntil,
    int? UsageLimit,
    int? UsageLimitPerUser,
    decimal? MinimumOrderValue,
    List<Guid>? ApplicableProductIds,
    List<Guid>? ApplicableCategoryIds,
    List<Guid>? ExcludedProductIds,
    bool IsActive
) : ICommand;

public record DeleteCouponCommand(
    Guid CouponId
) : ICommand;

public record ActivateCouponCommand(
    Guid CouponId
) : ICommand;

public record DeactivateCouponCommand(
    Guid CouponId
) : ICommand;

public record RecordCouponUsageCommand(
    Guid CouponId,
    Guid OrderId,
    Guid UserId,
    decimal DiscountAmount
) : ICommand;
```

### 6.2 Queries

```csharp
public record GetCouponByIdQuery(Guid CouponId) : IQuery<CouponDto>;

public record GetCouponByCodeQuery(string Code) : IQuery<CouponDto>;

public record ValidateCouponQuery(
    string Code,
    Guid UserId,
    decimal OrderTotal,
    List<Guid> ProductIds
) : IQuery<CouponValidationResult>;

public record CouponValidationResult(
    bool IsValid,
    string? ErrorMessage,
    decimal DiscountAmount,
    CouponDto? Coupon
);

public record GetActiveCouponsQuery(
    int Page = 1,
    int PageSize = 20
) : IQuery<PagedResult<CouponDto>>;

public record GetCouponUsageHistoryQuery(
    Guid CouponId,
    int Page = 1,
    int PageSize = 20
) : IQuery<PagedResult<CouponUsageDto>>;

public record GetUserCouponUsageQuery(
    Guid UserId,
    int Page = 1,
    int PageSize = 20
) : IQuery<PagedResult<CouponUsageDto>>;

public record GetCouponStatisticsQuery(
    Guid CouponId
) : IQuery<CouponStatisticsDto>;
```

---

## 7. Módulo Notifications

### 7.1 Commands

```csharp
// Enviar notificações
public record SendEmailNotificationCommand(
    Guid UserId,
    string TemplateName,
    Dictionary<string, object> TemplateData,
    string? Subject = null
) : ICommand<Guid>;

public record SendSmsNotificationCommand(
    Guid UserId,
    string Message
) : ICommand<Guid>;

public record SendPushNotificationCommand(
    Guid UserId,
    string Title,
    string Body,
    Dictionary<string, string>? Data = null
) : ICommand<Guid>;

public record SendBulkEmailCommand(
    List<Guid> UserIds,
    string TemplateName,
    Dictionary<string, object> TemplateData,
    string? Subject = null
) : ICommand<Guid>; // Returns batch ID

// Notificações de pedido
public record SendOrderConfirmationEmailCommand(
    Guid OrderId
) : ICommand;

public record SendOrderShippedEmailCommand(
    Guid OrderId
) : ICommand;

public record SendOrderDeliveredEmailCommand(
    Guid OrderId
) : ICommand;

public record SendOrderCancelledEmailCommand(
    Guid OrderId
) : ICommand;

// Notificações de produto
public record SendBackInStockNotificationCommand(
    Guid ProductId
) : ICommand;

public record SendPriceDropNotificationCommand(
    Guid ProductId,
    decimal OldPrice,
    decimal NewPrice
) : ICommand;

// Newsletter
public record SubscribeToNewsletterCommand(
    string Email,
    string? FirstName = null,
    Dictionary<string, string>? Tags = null
) : ICommand<Guid>;

public record UnsubscribeFromNewsletterCommand(
    string Email
) : ICommand;

public record SendNewsletterCommand(
    string Subject,
    string HtmlContent,
    List<string>? SegmentTags = null
) : ICommand<Guid>;

// Gerenciar notificações
public record MarkNotificationAsReadCommand(
    Guid NotificationId,
    Guid UserId
) : ICommand;

public record MarkAllNotificationsAsReadCommand(
    Guid UserId
) : ICommand;

public record DeleteNotificationCommand(
    Guid NotificationId,
    Guid UserId
) : ICommand;

// Alertas de estoque (usuário)
public record CreateStockAlertCommand(
    Guid UserId,
    Guid ProductId,
    Guid? VariantId = null
) : ICommand<Guid>;

public record DeleteStockAlertCommand(
    Guid AlertId,
    Guid UserId
) : ICommand;
```

### 7.2 Queries

```csharp
public record GetUserNotificationsQuery(
    Guid UserId,
    bool UnreadOnly = false,
    int Page = 1,
    int PageSize = 20
) : IQuery<PagedResult<NotificationDto>>;

public record GetUnreadNotificationCountQuery(
    Guid UserId
) : IQuery<int>;

public record GetNotificationByIdQuery(
    Guid NotificationId
) : IQuery<NotificationDto>;

public record GetEmailDeliveryStatusQuery(
    Guid EmailId
) : IQuery<EmailDeliveryStatusDto>;

public record GetNewsletterSubscribersQuery(
    List<string>? Tags = null,
    int Page = 1,
    int PageSize = 100
) : IQuery<PagedResult<NewsletterSubscriberDto>>;

public record GetUserStockAlertsQuery(
    Guid UserId
) : IQuery<List<StockAlertDto>>;
```

---

## 8. Módulo Analytics

### 8.1 Queries - Dashboard

```csharp
public record GetDashboardOverviewQuery(
    DateTime StartDate,
    DateTime EndDate
) : IQuery<DashboardOverviewDto>;

public record DashboardOverviewDto(
    decimal TotalRevenue,
    int TotalOrders,
    int NewCustomers,
    decimal AverageOrderValue,
    decimal ConversionRate,
    TrendData RevenueTrend,
    TrendData OrdersTrend
);

public record TrendData(
    decimal CurrentValue,
    decimal PreviousValue,
    decimal PercentageChange,
    string Direction // "up", "down", "stable"
);

public record GetRevenueAnalyticsQuery(
    DateTime StartDate,
    DateTime EndDate,
    string GroupBy = "day" // day, week, month
) : IQuery<List<RevenueDataPoint>>;

public record RevenueDataPoint(
    DateTime Date,
    decimal Revenue,
    int OrderCount,
    decimal AverageOrderValue
);

public record GetSalesAnalyticsQuery(
    DateTime StartDate,
    DateTime EndDate,
    string GroupBy = "day"
) : IQuery<SalesAnalyticsDto>;

public record GetTopSellingProductsQuery(
    DateTime StartDate,
    DateTime EndDate,
    int Limit = 10
) : IQuery<List<TopProductDto>>;

public record TopProductDto(
    Guid ProductId,
    string ProductName,
    int UnitsSold,
    decimal Revenue,
    string ImageUrl
);

public record GetTopCategoriesQuery(
    DateTime StartDate,
    DateTime EndDate,
    int Limit = 10
) : IQuery<List<TopCategoryDto>>;
```

### 8.2 Queries - Relatórios

```csharp
public record GetCustomerAnalyticsQuery(
    DateTime StartDate,
    DateTime EndDate
) : IQuery<CustomerAnalyticsDto>;

public record CustomerAnalyticsDto(
    int NewCustomers,
    int ReturningCustomers,
    int TotalActiveCustomers,
    decimal CustomerLifetimeValue,
    decimal CustomerRetentionRate,
    List<CustomerSegmentDto> Segments
);

public record GetAbandonmentAnalyticsQuery(
    DateTime StartDate,
    DateTime EndDate
) : IQuery<AbandonmentAnalyticsDto>;

public record AbandonmentAnalyticsDto(
    int AbandonedCarts,
    decimal AbandonedCartValue,
    decimal AbandonmentRate,
    int RecoveredCarts,
    decimal RecoveryRate
);

public record GetProductPerformanceQuery(
    Guid ProductId,
    DateTime StartDate,
    DateTime EndDate
) : IQuery<ProductPerformanceDto>;

public record GetInventoryReportQuery(
    bool LowStockOnly = false,
    int? WarehouseId = null
) : IQuery<InventoryReportDto>;

public record GetFinancialReportQuery(
    DateTime StartDate,
    DateTime EndDate
) : IQuery<FinancialReportDto>;

public record FinancialReportDto(
    decimal TotalRevenue,
    decimal TotalCost,
    decimal GrossProfit,
    decimal NetProfit,
    decimal TaxAmount,
    decimal ShippingRevenue,
    decimal Refunds,
    List<RevenueByPaymentMethodDto> RevenueByPaymentMethod
);

// Exportação de relatórios
public record ExportSalesReportCommand(
    DateTime StartDate,
    DateTime EndDate,
    string Format // "csv", "xlsx", "pdf"
) : ICommand<string>; // Returns file URL

public record ExportProductReportCommand(
    DateTime StartDate,
    DateTime EndDate,
    string Format
) : ICommand<string>;

public record ExportCustomerReportCommand(
    DateTime StartDate,
    DateTime EndDate,
    string Format
) : ICommand<string>;
```

### 8.3 Queries - Métricas em Tempo Real

```csharp
public record GetRealTimeStatisticsQuery() : IQuery<RealTimeStatisticsDto>;

public record RealTimeStatisticsDto(
    int ActiveUsers,
    int OngoingCheckouts,
    int OrdersToday,
    decimal RevenueToday,
    List<RecentOrderDto> RecentOrders
);

public record GetTrafficAnalyticsQuery(
    DateTime StartDate,
    DateTime EndDate
) : IQuery<TrafficAnalyticsDto>;

public record TrafficAnalyticsDto(
    int TotalVisits,
    int UniqueVisitors,
    decimal BounceRate,
    TimeSpan AverageSessionDuration,
    List<TrafficSourceDto> TrafficSources,
    List<PopularPageDto> PopularPages
);
```

---

## 9. Módulo Admin

### 9.1 Commands - Configurações

```csharp
public record UpdateStoreSettingsCommand(
    string StoreName,
    string? StoreDescription,
    string? LogoUrl,
    string? FaviconUrl,
    string DefaultCurrency,
    string DefaultLanguage,
    string TimeZone,
    string ContactEmail,
    string? ContactPhone,
    Dictionary<string, string>? SocialMediaLinks
) : ICommand;

public record UpdateCheckoutSettingsCommand(
    bool AllowGuestCheckout,
    bool RequireAccountVerification,
    int CartExpirationMinutes,
    bool EnableCoupons,
    bool EnableGiftCards,
    string[] AllowedPaymentMethods,
    string[] AllowedShippingMethods
) : ICommand;

public record UpdateEmailSettingsCommand(
    string SmtpHost,
    int SmtpPort,
    string SmtpUsername,
    string SmtpPassword,
    bool UseSsl,
    string FromEmail,
    string FromName
) : ICommand;

public record UpdateTaxSettingsCommand(
    bool EnableTax,
    decimal DefaultTaxRate,
    bool PricesIncludeTax,
    List<TaxRuleDto> TaxRules
) : ICommand;

public record TaxRuleDto(
    string Country,
    string? State,
    decimal TaxRate,
    string TaxName
);

public record UpdateShippingSettingsCommand(
    List<ShippingZoneDto> ShippingZones,
    bool EnableFreeShippingThreshold,
    decimal? FreeShippingThreshold
) : ICommand;

public record ShippingZoneDto(
    string Name,
    List<string> Countries,
    List<ShippingRateDto> Rates
);

public record ShippingRateDto(
    string Name,
    decimal BaseRate,
    decimal? PerKgRate,
    int? EstimatedDays
);
```

### 9.2 Commands - Gestão de Conteúdo

```csharp
// Páginas estáticas
public record CreatePageCommand(
    string Title,
    string Slug,
    string Content,
    string? MetaDescription,
    string? MetaKeywords,
    bool IsPublished = false
) : ICommand<Guid>;

public record UpdatePageCommand(
    Guid PageId,
    string Title,
    string Slug,
    string Content,
    string? MetaDescription,
    string? MetaKeywords,
    bool IsPublished
) : ICommand;

public record DeletePageCommand(Guid PageId) : ICommand;

// Banners
public record CreateBannerCommand(
    string Title,
    string? Description,
    string ImageUrl,
    string? LinkUrl,
    string Position, // "home-hero", "home-secondary", "sidebar"
    DateTime? ActiveFrom,
    DateTime? ActiveUntil,
    int DisplayOrder = 0
) : ICommand<Guid>;

public record UpdateBannerCommand(
    Guid BannerId,
    string Title,
    string? Description,
    string ImageUrl,
    string? LinkUrl,
    string Position,
    DateTime? ActiveFrom,
    DateTime? ActiveUntil,
    int DisplayOrder
) : ICommand;

public record DeleteBannerCommand(Guid BannerId) : ICommand;

// FAQ
public record CreateFaqCommand(
    string Question,
    string Answer,
    Guid? CategoryId,
    int DisplayOrder = 0
) : ICommand<Guid>;

public record UpdateFaqCommand(
    Guid FaqId,
    string Question,
    string Answer,
    Guid? CategoryId,
    int DisplayOrder
) : ICommand;

public record DeleteFaqCommand(Guid FaqId) : ICommand;
```

### 9.3 Commands - Importação/Exportação

```csharp
public record ImportProductsFromCsvCommand(
    Stream FileStream,
    bool UpdateExisting = false
) : ICommand<ImportResultDto>;

public record ImportResultDto(
    int TotalRows,
    int SuccessCount,
    int FailureCount,
    List<ImportErrorDto> Errors
);

public record ImportErrorDto(int Row, string Error);

public record ExportProductsToCsvCommand(
    List<Guid>? ProductIds = null
) : ICommand<string>; // Returns file URL

public record ImportCustomersFromCsvCommand(
    Stream FileStream
) : ICommand<ImportResultDto>;

public record ExportCustomersToCsvCommand() : ICommand<string>;

public record ImportOrdersFromCsvCommand(
    Stream FileStream
) : ICommand<ImportResultDto>;
```

### 9.4 Queries - Configurações e Logs

```csharp
public record GetStoreSettingsQuery() : IQuery<StoreSettingsDto>;

public record GetCheckoutSettingsQuery() : IQuery<CheckoutSettingsDto>;

public record GetEmailSettingsQuery() : IQuery<EmailSettingsDto>;

public record GetTaxSettingsQuery() : IQuery<TaxSettingsDto>;

public record GetShippingSettingsQuery() : IQuery<ShippingSettingsDto>;

// Logs de sistema
public record GetSystemLogsQuery(
    string? Level = null, // Debug, Info, Warning, Error
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    string? SearchTerm = null,
    int Page = 1,
    int PageSize = 50
) : IQuery<PagedResult<SystemLogDto>>;

public record GetAuditLogsQuery(
    Guid? UserId = null,
    string? EntityType = null,
    Guid? EntityId = null,
    string? Action = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int Page = 1,
    int PageSize = 50
) : IQuery<PagedResult<AuditLogDto>>;

// Páginas
public record GetPageByIdQuery(Guid PageId) : IQuery<PageDto>;

public record GetPageBySlugQuery(string Slug) : IQuery<PageDto>;

public record GetAllPagesQuery() : IQuery<List<PageDto>>;

// Banners
public record GetActiveBannersQuery(
    string? Position = null
) : IQuery<List<BannerDto>>;

public record GetAllBannersQuery() : IQuery<List<BannerDto>>;

// FAQ
public record GetAllFaqsQuery() : IQuery<List<FaqDto>>;

public record GetFaqsByCategoryQuery(
    Guid CategoryId
) : IQuery<List<FaqDto>>;
```

---

## 10. Módulo Reviews

### 10.1 Commands

```csharp
public record CreateProductReviewCommand(
    Guid ProductId,
    Guid UserId,
    int Rating, // 1-5
    string? Title,
    string Comment,
    List<string>? ImageUrls = null
) : ICommand<Guid>;

public record UpdateProductReviewCommand(
    Guid ReviewId,
    Guid UserId,
    int Rating,
    string? Title,
    string Comment
) : ICommand;

public record DeleteProductReviewCommand(
    Guid ReviewId,
    Guid UserId
) : ICommand;

public record ApproveReviewCommand(
    Guid ReviewId
) : ICommand;

public record RejectReviewCommand(
    Guid ReviewId,
    string Reason
) : ICommand;

public record FlagReviewCommand(
    Guid ReviewId,
    Guid UserId,
    string Reason
) : ICommand;

public record RespondToReviewCommand(
    Guid ReviewId,
    string Response,
    Guid AdminUserId
) : ICommand;

public record MarkReviewAsHelpfulCommand(
    Guid ReviewId,
    Guid UserId
) : ICommand;

public record MarkReviewAsNotHelpfulCommand(
    Guid ReviewId,
    Guid UserId
) : ICommand;
```

### 10.2 Queries

```csharp
public record GetProductReviewsQuery(
    Guid ProductId,
    int? MinRating = null,
    int? MaxRating = null,
    bool VerifiedPurchaseOnly = false,
    string SortBy = "CreatedAt", // CreatedAt, Rating, Helpful
    string SortOrder = "Desc",
    int Page = 1,
    int PageSize = 20
) : IQuery<PagedResult<ProductReviewDto>>;

public record GetReviewByIdQuery(
    Guid ReviewId
) : IQuery<ProductReviewDto>;

public record GetUserReviewsQuery(
    Guid UserId,
    int Page = 1,
    int PageSize = 20
) : IQuery<PagedResult<ProductReviewDto>>;

public record GetProductRatingSummaryQuery(
    Guid ProductId
) : IQuery<RatingSummaryDto>;

public record RatingSummaryDto(
    decimal AverageRating,
    int TotalReviews,
    Dictionary<int, int> RatingDistribution, // {5: 120, 4: 45, 3: 10, 2: 3, 1: 2}
    int VerifiedPurchaseCount
);

public record GetPendingReviewsQuery(
    int Page = 1,
    int PageSize = 20
) : IQuery<PagedResult<ProductReviewDto>>;

public record GetFlaggedReviewsQuery(
    int Page = 1,
    int PageSize = 20
) : IQuery<PagedResult<ProductReviewDto>>;

public record CanUserReviewProductQuery(
    Guid UserId,
    Guid ProductId
) : IQuery<ReviewEligibilityDto>;

public record ReviewEligibilityDto(
    bool CanReview,
    string? Reason,
    bool HasPurchased,
    bool HasExistingReview
);
```

---

## 11. Módulo Inventory

### 11.1 Commands

```csharp
// Gerenciamento de estoque
public record AddStockCommand(
    Guid ProductId,
    int Quantity,
    string Reason,
    string? Reference = null
) : ICommand;

public record RemoveStockCommand(
    Guid ProductId,
    int Quantity,
    string Reason,
    string? Reference = null
) : ICommand;

public record SetStockLevelCommand(
    Guid ProductId,
    int NewQuantity,
    string Reason
) : ICommand;

public record ReserveStockCommand(
    Guid ProductId,
    int Quantity,
    Guid OrderId,
    TimeSpan? ReservationDuration = null
) : ICommand<Guid>;

public record ReleaseStockReservationCommand(
    Guid ReservationId
) : ICommand;

public record CommitStockReservationCommand(
    Guid ReservationId
) : ICommand;

// Transferência entre depósitos (multi-warehouse)
public record TransferStockCommand(
    Guid ProductId,
    int FromWarehouseId,
    int ToWarehouseId,
    int Quantity,
    string Reason
) : ICommand<Guid>;

// Inventário físico
public record CreateStockAdjustmentCommand(
    Guid ProductId,
    int PhysicalCount,
    string Reason,
    Guid? PerformedBy = null
) : ICommand<Guid>;

public record CreateBulkStockAdjustmentCommand(
    List<StockAdjustmentItemDto> Items,
    string Reason,
    Guid? PerformedBy = null
) : ICommand<Guid>;

public record StockAdjustmentItemDto(
    Guid ProductId,
    int PhysicalCount
);

// Alertas de estoque
public record SetLowStockThresholdCommand(
    Guid ProductId,
    int Threshold
) : ICommand;

public record SetOutOfStockNotificationCommand(
    Guid ProductId,
    bool EnableNotification
) : ICommand;
```

### 11.2 Queries

```csharp
public record GetProductStockQuery(
    Guid ProductId
) : IQuery<StockDto>;

public record StockDto(
    Guid ProductId,
    int AvailableQuantity,
    int ReservedQuantity,
    int TotalQuantity,
    int LowStockThreshold,
    bool IsLowStock,
    bool IsOutOfStock
);

public record GetStockHistoryQuery(
    Guid ProductId,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int Page = 1,
    int PageSize = 50
) : IQuery<PagedResult<StockMovementDto>>;

public record GetStockReservationsQuery(
    Guid? ProductId = null,
    Guid? OrderId = null,
    bool ActiveOnly = true,
    int Page = 1,
    int PageSize = 20
) : IQuery<PagedResult<StockReservationDto>>;

public record GetLowStockProductsQuery(
    int? CustomThreshold = null
) : IQuery<List<LowStockAlertDto>>;

public record GetOutOfStockProductsQuery() : IQuery<List<ProductStockDto>>;

public record GetStockValueQuery(
    Guid? CategoryId = null
) : IQuery<StockValueDto>;

public record StockValueDto(
    decimal TotalValue,
    int TotalProducts,
    Dictionary<Guid, decimal> ValueByCategory
);

public record GetStockMovementReportQuery(
    DateTime StartDate,
    DateTime EndDate,
    Guid? ProductId = null,
    string? MovementType = null // "Addition", "Removal", "Reservation", "Transfer"
) : IQuery<StockMovementReportDto>;
```

---

## 12. Módulo Shipping

### 12.1 Commands

```csharp
// Cálculo de frete
public record CalculateShippingRatesCommand(
    string DestinationPostalCode,
    List<ShippingItemDto> Items,
    decimal? OrderValue = null
) : ICommand<List<ShippingRateDto>>;

public record ShippingItemDto(
    Guid ProductId,
    int Quantity,
    decimal Weight,
    decimal? Height,
    decimal? Width,
    decimal? Length
);

public record ShippingRateDto(
    string CarrierName,
    string ServiceName,
    decimal Cost,
    int EstimatedDays,
    string? AdditionalInfo
);

// Rastreamento
public record CreateShipmentCommand(
    Guid OrderId,
    string Carrier,
    string TrackingCode,
    List<Guid> OrderItemIds,
    decimal? Weight = null,
    DateTime? EstimatedDeliveryDate = null
) : ICommand<Guid>;

public record UpdateShipmentTrackingCommand(
    Guid ShipmentId,
    string TrackingCode
) : ICommand;

public record AddTrackingEventCommand(
    Guid ShipmentId,
    string Status,
    string Description,
    string? Location = null,
    DateTime? EventDate = null
) : ICommand;

public record MarkShipmentAsDeliveredCommand(
    Guid ShipmentId,
    DateTime DeliveredAt,
    string? ReceivedBy = null,
    string? Signature = null
) : ICommand;

// Etiquetas de envio
public record GenerateShippingLabelCommand(
    Guid OrderId,
    string Carrier,
    string ServiceType
) : ICommand<ShippingLabelDto>;

public record ShippingLabelDto(
    string LabelUrl,
    string TrackingCode,
    decimal Cost
);

public record PrintShippingLabelCommand(
    Guid ShipmentId
) : ICommand<string>; // Returns PDF URL

// Manifesto de envio
public record CreateShippingManifestCommand(
    List<Guid> ShipmentIds,
    DateTime ShipDate
) : ICommand<Guid>;

public record CloseShippingManifestCommand(
    Guid ManifestId
) : ICommand;
```

### 12.2 Queries

```csharp
public record GetShipmentByIdQuery(
    Guid ShipmentId
) : IQuery<ShipmentDto>;

public record GetOrderShipmentsQuery(
    Guid OrderId
) : IQuery<List<ShipmentDto>>;

public record GetShipmentTrackingQuery(
    Guid ShipmentId
) : IQuery<ShipmentTrackingDto>;

public record GetShipmentTrackingByCodeQuery(
    string TrackingCode
) : IQuery<ShipmentTrackingDto>;

public record GetPendingShipmentsQuery(
    int Page = 1,
    int PageSize = 20
) : IQuery<PagedResult<ShipmentDto>>;

public record GetShipmentsToManifestQuery(
    string? Carrier = null
) : IQuery<List<ShipmentDto>>;

public record GetShippingManifestQuery(
    Guid ManifestId
) : IQuery<ShippingManifestDto>;

public record GetShippingStatisticsQuery(
    DateTime StartDate,
    DateTime EndDate
) : IQuery<ShippingStatisticsDto>;

public record ValidatePostalCodeQuery(
    string PostalCode
) : IQuery<PostalCodeValidationDto>;

public record PostalCodeValidationDto(
    bool IsValid,
    string? City,
    string? State,
    string? Neighborhood
);
```

---

## 📊 Resumo Geral

### Total de Commands e Queries por Módulo

| Módulo | Commands | Queries | Total |
|--------|----------|---------|-------|
| Users | 35 | 15 | 50 |
| Catalog | 32 | 28 | 60 |
| Cart | 10 | 8 | 18 |
| Orders | 36 | 24 | 60 |
| Payments | 18 | 9 | 27 |
| Coupons | 7 | 8 | 15 |
| Notifications | 17 | 7 | 24 |
| Analytics | 5 | 13 | 18 |
| Admin | 19 | 16 | 35 |
| Reviews | 9 | 9 | 18 |
| Inventory | 11 | 9 | 20 |
| Shipping | 11 | 11 | 22 |
| **TOTAL** | **210** | **157** | **367** |

---

## 🎯 Convenções de Nomenclatura

### Commands
- Formato: `{Ação}{Entidade}Command`
- Exemplos: `CreateProductCommand`, `UpdateOrderStatusCommand`, `DeleteUserCommand`
- Sempre retornam `ICommand` ou `ICommand<TResponse>`

### Queries
- Formato: `Get{Entidade}Query` ou `{Ação}{Entidade}Query`
- Exemplos: `GetProductByIdQuery`, `SearchProductsQuery`, `ValidateCouponQuery`
- Sempre retornam `IQuery<TResponse>`

### Handlers
- Commands: `{CommandName}Handler`
- Queries: `{QueryName}Handler`

### DTOs
- Formato: `{Entidade}Dto`
- Exemplos: `ProductDto`, `OrderDto`, `UserProfileDto`

---

## 🚀 Prioridades de Implementação

### Fase 1 - MVP (Essencial)
1. **Users**: Registro, Login, Perfil básico
2. **Catalog**: CRUD de produtos, categorias
3. **Cart**: Adicionar/remover items
4. **Orders**: Criar pedido, gerenciar status
5. **Payments**: Integração básica (PIX/Cartão)

### Fase 2 - Core E-commerce
6. **Inventory**: Controle de estoque
7. **Shipping**: Cálculo de frete, rastreamento
8. **Coupons**: Sistema de cupons
9. **Reviews**: Avaliações de produtos
10. **Notifications**: Emails transacionais

### Fase 3 - Admin & Analytics
11. **Admin**: Configurações, gestão de conteúdo
12. **Analytics**: Dashboard, relatórios
13. **Advanced Features**: Wishlist, recomendações, etc.

---

**Gerado em:** 2025-12-23
**Versão:** 1.0
**Status:** Planejamento completo
