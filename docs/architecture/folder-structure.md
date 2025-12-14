# 🏗️ Estrutura Completa - BCommerce Monolito Modular

## 📋 Visão Geral

Sistema de e-commerce modular monolith em .NET 8 com CQRS, DDD, EF Core e PostgreSQL.

---

## 📂 Estrutura de Pastas

```
bcommerce/
│
├── docs/                                          # Documentação
│   ├── architecture/
│   │   ├── adr/                                  # Architecture Decision Records
│   │   │   ├── 001-monolito-modular.md
│   │   │   ├── 002-cqrs-mediatr.md
│   │   │   ├── 003-event-driven.md
│   │   │   └── 004-postgresql-schema.md
│   │   ├── diagrams/
│   │   │   ├── system-context.md
│   │   │   ├── module-dependencies.md
│   │   │   ├── database-erd.md
│   │   │   └── event-flow.md
│   │   └── README.md
│   ├── api/
│   │   ├── openapi.yaml
│   │   ├── postman-collection.json
│   │   └── endpoints.md
│   └── db/
│       ├── schema.sql                            # ✅ JÁ EXISTE
│       └── migrations-guide.md
│
├── src/
│   │
│   ├── building-blocks/                          # 🧱 Building Blocks Compartilhados
│   │   │
│   │   ├── BuildingBlocks.Domain/                # ✅ IMPLEMENTADO
│   │   │   ├── Entities/
│   │   │   │   ├── Entity.cs
│   │   │   │   ├── AggregateRoot.cs
│   │   │   │   ├── IAuditableEntity.cs
│   │   │   │   └── ISoftDeletable.cs
│   │   │   ├── Events/
│   │   │   │   ├── IDomainEvent.cs
│   │   │   │   ├── DomainEvent.cs
│   │   │   │   ├── IIntegrationEvent.cs
│   │   │   │   └── IntegrationEvent.cs
│   │   │   ├── Models/
│   │   │   │   ├── ValueObject.cs
│   │   │   │   └── Enumeration.cs
│   │   │   ├── Repositories/
│   │   │   │   ├── IRepository.cs
│   │   │   │   └── IUnitOfWork.cs
│   │   │   ├── Exceptions/
│   │   │   │   └── DomainException.cs
│   │   │   ├── README.md
│   │   │   ├── examples.md
│   │   │   └── BuildingBlocks.Domain.csproj
│   │   │
│   │   ├── BuildingBlocks.Application/           # ✅ IMPLEMENTADO
│   │   │   ├── Abstractions/
│   │   │   │   ├── ICommand.cs
│   │   │   │   ├── ICommandHandler.cs
│   │   │   │   ├── IQuery.cs
│   │   │   │   └── IQueryHandler.cs
│   │   │   ├── Behaviors/
│   │   │   │   ├── ValidationBehavior.cs
│   │   │   │   ├── LoggingBehavior.cs
│   │   │   │   └── TransactionBehavior.cs
│   │   │   ├── Pagination/
│   │   │   │   ├── PagedResult.cs
│   │   │   │   └── PaginationParams.cs
│   │   │   ├── Results/
│   │   │   │   ├── Result.cs
│   │   │   │   └── Error.cs
│   │   │   ├── README.md
│   │   │   ├── examples.md
│   │   │   └── BuildingBlocks.Application.csproj
│   │   │
│   │   ├── BuildingBlocks.Infrastructure/        # ✅ IMPLEMENTADO
│   │   │   ├── Persistence/
│   │   │   │   ├── Configurations/
│   │   │   │   │   └── BaseEntityConfiguration.cs
│   │   │   │   ├── Interceptors/
│   │   │   │   │   ├── AuditableEntityInterceptor.cs
│   │   │   │   │   ├── SoftDeleteInterceptor.cs
│   │   │   │   │   ├── PublishDomainEventsInterceptor.cs
│   │   │   │   │   └── OptimisticConcurrencyInterceptor.cs
│   │   │   │   ├── UnitOfWork.cs
│   │   │   │   └── UnitOfWorkExtensions.cs
│   │   │   ├── EventBus/
│   │   │   │   ├── IEventBus.cs
│   │   │   │   ├── InMemoryEventBus.cs
│   │   │   │   └── OutboxEventBus.cs
│   │   │   ├── BackgroundJobs/
│   │   │   │   ├── ProcessOutboxMessagesJob.cs
│   │   │   │   └── CleanupExpiredSessionsJob.cs
│   │   │   ├── Caching/
│   │   │   │   ├── ICacheService.cs
│   │   │   │   └── MemoryCacheService.cs
│   │   │   ├── README.md
│   │   │   └── BuildingBlocks.Infrastructure.csproj
│   │   │
│   │   └── BuildingBlocks.Presentation/          # ✅ IMPLEMENTADO
│   │       ├── Controllers/
│   │       │   └── ApiControllerBase.cs
│   │       ├── Filters/
│   │       │   ├── ExceptionHandlingFilter.cs
│   │       │   └── ValidationFilter.cs
│   │       ├── Middleware/
│   │       │   ├── ExceptionHandlingMiddleware.cs
│   │       │   └── RequestLoggingMiddleware.cs
│   │       ├── Extensions/
│   │       │   ├── ResultExtensions.cs
│   │       │   └── ProblemDetailsExtensions.cs
│   │       ├── README.md
│   │       └── BuildingBlocks.Presentation.csproj
│   │
│   ├── modules/                                  # 📦 Módulos de Domínio
│   │   │
│   │   ├── users/                                # 👤 MÓDULO DE USUÁRIOS (Parcialmente Implementado)
│   │   │   │
│   │   │   ├── Users.Core/                       # ✅ IMPLEMENTADO
│   │   │   │   ├── Entities/
│   │   │   │   │   ├── User.cs
│   │   │   │   │   ├── Profile.cs
│   │   │   │   │   ├── Address.cs
│   │   │   │   │   ├── Session.cs
│   │   │   │   │   ├── Notification.cs
│   │   │   │   │   ├── NotificationPreferences.cs
│   │   │   │   │   └── LoginHistory.cs
│   │   │   │   ├── Events/
│   │   │   │   │   ├── UserCreatedEvent.cs
│   │   │   │   │   ├── EmailConfirmedEvent.cs
│   │   │   │   │   ├── ProfileCreatedEvent.cs
│   │   │   │   │   ├── ProfileUpdatedEvent.cs
│   │   │   │   │   ├── AddressAddedEvent.cs
│   │   │   │   │   ├── SessionCreatedEvent.cs
│   │   │   │   │   ├── SessionRevokedEvent.cs
│   │   │   │   │   └── UserLockedEvent.cs
│   │   │   │   ├── Repositories/
│   │   │   │   │   ├── IUserRepository.cs
│   │   │   │   │   ├── IProfileRepository.cs
│   │   │   │   │   ├── IAddressRepository.cs
│   │   │   │   │   ├── ISessionRepository.cs
│   │   │   │   │   ├── INotificationRepository.cs
│   │   │   │   │   ├── INotificationPreferencesRepository.cs
│   │   │   │   │   └── ILoginHistoryRepository.cs
│   │   │   │   └── Users.Core.csproj
│   │   │   │
│   │   │   ├── Users.Application/                # 🔨 A IMPLEMENTAR
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── RegisterUser/
│   │   │   │   │   │   ├── RegisterUserCommand.cs
│   │   │   │   │   │   ├── RegisterUserCommandValidator.cs
│   │   │   │   │   │   └── RegisterUserCommandHandler.cs
│   │   │   │   │   ├── CreateProfile/
│   │   │   │   │   │   ├── CreateProfileCommand.cs
│   │   │   │   │   │   ├── CreateProfileCommandValidator.cs
│   │   │   │   │   │   └── CreateProfileCommandHandler.cs
│   │   │   │   │   ├── UpdateProfile/
│   │   │   │   │   │   ├── UpdateProfileCommand.cs
│   │   │   │   │   │   ├── UpdateProfileCommandValidator.cs
│   │   │   │   │   │   └── UpdateProfileCommandHandler.cs
│   │   │   │   │   ├── AddAddress/
│   │   │   │   │   │   ├── AddAddressCommand.cs
│   │   │   │   │   │   ├── AddAddressCommandValidator.cs
│   │   │   │   │   │   └── AddAddressCommandHandler.cs
│   │   │   │   │   ├── UpdateAddress/
│   │   │   │   │   │   ├── UpdateAddressCommand.cs
│   │   │   │   │   │   ├── UpdateAddressCommandValidator.cs
│   │   │   │   │   │   └── UpdateAddressCommandHandler.cs
│   │   │   │   │   ├── DeleteAddress/
│   │   │   │   │   │   ├── DeleteAddressCommand.cs
│   │   │   │   │   │   └── DeleteAddressCommandHandler.cs
│   │   │   │   │   ├── ConfirmEmail/
│   │   │   │   │   │   ├── ConfirmEmailCommand.cs
│   │   │   │   │   │   └── ConfirmEmailCommandHandler.cs
│   │   │   │   │   ├── ChangePassword/
│   │   │   │   │   │   ├── ChangePasswordCommand.cs
│   │   │   │   │   │   ├── ChangePasswordCommandValidator.cs
│   │   │   │   │   │   └── ChangePasswordCommandHandler.cs
│   │   │   │   │   └── UpdateNotificationPreferences/
│   │   │   │   │       ├── UpdateNotificationPreferencesCommand.cs
│   │   │   │   │       ├── UpdateNotificationPreferencesCommandValidator.cs
│   │   │   │   │       └── UpdateNotificationPreferencesCommandHandler.cs
│   │   │   │   ├── Queries/
│   │   │   │   │   ├── GetUserById/
│   │   │   │   │   │   ├── GetUserByIdQuery.cs
│   │   │   │   │   │   └── GetUserByIdQueryHandler.cs
│   │   │   │   │   ├── GetUserProfile/
│   │   │   │   │   │   ├── GetUserProfileQuery.cs
│   │   │   │   │   │   └── GetUserProfileQueryHandler.cs
│   │   │   │   │   ├── GetUserAddresses/
│   │   │   │   │   │   ├── GetUserAddressesQuery.cs
│   │   │   │   │   │   └── GetUserAddressesQueryHandler.cs
│   │   │   │   │   ├── GetActiveSessions/
│   │   │   │   │   │   ├── GetActiveSessionsQuery.cs
│   │   │   │   │   │   └── GetActiveSessionsQueryHandler.cs
│   │   │   │   │   ├── GetNotifications/
│   │   │   │   │   │   ├── GetNotificationsQuery.cs
│   │   │   │   │   │   └── GetNotificationsQueryHandler.cs
│   │   │   │   │   └── GetLoginHistory/
│   │   │   │   │       ├── GetLoginHistoryQuery.cs
│   │   │   │   │       └── GetLoginHistoryQueryHandler.cs
│   │   │   │   ├── DTOs/
│   │   │   │   │   ├── UserDto.cs
│   │   │   │   │   ├── ProfileDto.cs
│   │   │   │   │   ├── AddressDto.cs
│   │   │   │   │   ├── SessionDto.cs
│   │   │   │   │   ├── NotificationDto.cs
│   │   │   │   │   └── LoginHistoryDto.cs
│   │   │   │   ├── EventHandlers/
│   │   │   │   │   ├── UserCreatedEventHandler.cs
│   │   │   │   │   ├── EmailConfirmedEventHandler.cs
│   │   │   │   │   └── UserLockedEventHandler.cs
│   │   │   │   ├── Services/
│   │   │   │   │   ├── IEmailService.cs
│   │   │   │   │   ├── ISmsService.cs
│   │   │   │   │   └── INotificationService.cs
│   │   │   │   └── Users.Application.csproj
│   │   │   │
│   │   │   ├── Users.Infrastructure/             # ✅ PARCIALMENTE IMPLEMENTADO
│   │   │   │   ├── Persistence/
│   │   │   │   │   ├── Configurations/
│   │   │   │   │   │   ├── UserConfiguration.cs
│   │   │   │   │   │   ├── ProfileConfiguration.cs
│   │   │   │   │   │   ├── AddressConfiguration.cs
│   │   │   │   │   │   ├── SessionConfiguration.cs
│   │   │   │   │   │   ├── NotificationConfiguration.cs
│   │   │   │   │   │   ├── NotificationPreferencesConfiguration.cs
│   │   │   │   │   │   └── LoginHistoryConfiguration.cs
│   │   │   │   │   ├── Migrations/              # 🔨 A GERAR
│   │   │   │   │   │   └── [timestamp]_InitialCreate.cs
│   │   │   │   │   └── UsersDbContext.cs
│   │   │   │   ├── Repositories/
│   │   │   │   │   ├── UserRepository.cs
│   │   │   │   │   ├── ProfileRepository.cs
│   │   │   │   │   ├── AddressRepository.cs
│   │   │   │   │   ├── SessionRepository.cs
│   │   │   │   │   ├── NotificationRepository.cs
│   │   │   │   │   ├── NotificationPreferencesRepository.cs
│   │   │   │   │   └── LoginHistoryRepository.cs
│   │   │   │   ├── Services/                    # 🔨 A IMPLEMENTAR
│   │   │   │   │   ├── EmailService.cs
│   │   │   │   │   ├── SmsService.cs
│   │   │   │   │   └── NotificationService.cs
│   │   │   │   ├── DependencyInjection.cs       # 🔨 A IMPLEMENTAR
│   │   │   │   └── Users.Infrastructure.csproj
│   │   │   │
│   │   │   ├── Users.Contracts/                  # 🔨 A IMPLEMENTAR
│   │   │   │   ├── Events/
│   │   │   │   │   └── UserCreatedIntegrationEvent.cs
│   │   │   │   └── Users.Contracts.csproj
│   │   │   │
│   │   │   └── Users.Presentation/               # 🔨 A IMPLEMENTAR
│   │   │       ├── Controllers/
│   │   │       │   ├── UsersController.cs
│   │   │       │   ├── ProfileController.cs
│   │   │       │   ├── AddressesController.cs
│   │   │       │   ├── SessionsController.cs
│   │   │       │   └── NotificationsController.cs
│   │   │       ├── Requests/
│   │   │       │   ├── RegisterUserRequest.cs
│   │   │       │   ├── CreateProfileRequest.cs
│   │   │       │   ├── UpdateProfileRequest.cs
│   │   │       │   ├── AddAddressRequest.cs
│   │   │       │   └── UpdateAddressRequest.cs
│   │   │       ├── DependencyInjection.cs
│   │   │       └── Users.Presentation.csproj
│   │   │
│   │   ├── catalog/                              # 🛍️ MÓDULO DE CATÁLOGO (A IMPLEMENTAR)
│   │   │   │
│   │   │   ├── Catalog.Core/
│   │   │   │   ├── Entities/
│   │   │   │   │   ├── Product.cs
│   │   │   │   │   ├── Category.cs
│   │   │   │   │   ├── Brand.cs
│   │   │   │   │   ├── ProductImage.cs
│   │   │   │   │   ├── StockMovement.cs
│   │   │   │   │   ├── StockReservation.cs
│   │   │   │   │   ├── ProductReview.cs
│   │   │   │   │   └── UserFavorite.cs
│   │   │   │   ├── Enums/
│   │   │   │   │   ├── ProductStatus.cs
│   │   │   │   │   └── StockMovementType.cs
│   │   │   │   ├── Events/
│   │   │   │   │   ├── ProductCreatedEvent.cs
│   │   │   │   │   ├── ProductPriceChangedEvent.cs
│   │   │   │   │   ├── ProductPublishedEvent.cs
│   │   │   │   │   ├── StockReservedEvent.cs
│   │   │   │   │   ├── StockReleasedEvent.cs
│   │   │   │   │   └── ReviewCreatedEvent.cs
│   │   │   │   ├── Repositories/
│   │   │   │   │   ├── IProductRepository.cs
│   │   │   │   │   ├── ICategoryRepository.cs
│   │   │   │   │   ├── IBrandRepository.cs
│   │   │   │   │   └── IProductReviewRepository.cs
│   │   │   │   ├── Exceptions/
│   │   │   │   │   ├── InsufficientStockException.cs
│   │   │   │   │   └── InvalidPriceException.cs
│   │   │   │   └── Catalog.Core.csproj
│   │   │   │
│   │   │   ├── Catalog.Application/
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── CreateProduct/
│   │   │   │   │   ├── UpdateProduct/
│   │   │   │   │   ├── UpdateProductPrice/
│   │   │   │   │   ├── PublishProduct/
│   │   │   │   │   ├── ReserveStock/
│   │   │   │   │   ├── ReleaseStock/
│   │   │   │   │   ├── CreateCategory/
│   │   │   │   │   └── CreateReview/
│   │   │   │   ├── Queries/
│   │   │   │   │   ├── GetProductById/
│   │   │   │   │   ├── SearchProducts/
│   │   │   │   │   ├── GetCategories/
│   │   │   │   │   ├── GetProductReviews/
│   │   │   │   │   └── GetUserFavorites/
│   │   │   │   ├── DTOs/
│   │   │   │   ├── EventHandlers/
│   │   │   │   └── Catalog.Application.csproj
│   │   │   │
│   │   │   ├── Catalog.Infrastructure/
│   │   │   │   ├── Persistence/
│   │   │   │   │   ├── Configurations/
│   │   │   │   │   ├── Migrations/
│   │   │   │   │   └── CatalogDbContext.cs
│   │   │   │   ├── Repositories/
│   │   │   │   ├── DependencyInjection.cs
│   │   │   │   └── Catalog.Infrastructure.csproj
│   │   │   │
│   │   │   ├── Catalog.Contracts/
│   │   │   │   ├── Events/
│   │   │   │   │   ├── ProductCreatedIntegrationEvent.cs
│   │   │   │   │   └── StockReservedIntegrationEvent.cs
│   │   │   │   └── Catalog.Contracts.csproj
│   │   │   │
│   │   │   └── Catalog.Presentation/
│   │   │       ├── Controllers/
│   │   │       │   ├── ProductsController.cs
│   │   │       │   ├── CategoriesController.cs
│   │   │       │   ├── BrandsController.cs
│   │   │       │   └── ReviewsController.cs
│   │   │       ├── Requests/
│   │   │       ├── DependencyInjection.cs
│   │   │       └── Catalog.Presentation.csproj
│   │   │
│   │   ├── cart/                                 # 🛒 MÓDULO DE CARRINHO (A IMPLEMENTAR)
│   │   │   │
│   │   │   ├── Cart.Core/
│   │   │   │   ├── Entities/
│   │   │   │   │   ├── Cart.cs
│   │   │   │   │   ├── CartItem.cs
│   │   │   │   │   ├── CartActivityLog.cs
│   │   │   │   │   └── SavedCart.cs
│   │   │   │   ├── Enums/
│   │   │   │   │   └── CartStatus.cs
│   │   │   │   ├── Events/
│   │   │   │   │   ├── CartCreatedEvent.cs
│   │   │   │   │   ├── ItemAddedToCartEvent.cs
│   │   │   │   │   ├── ItemRemovedFromCartEvent.cs
│   │   │   │   │   └── CartConvertedEvent.cs
│   │   │   │   ├── Repositories/
│   │   │   │   │   └── ICartRepository.cs
│   │   │   │   └── Cart.Core.csproj
│   │   │   │
│   │   │   ├── Cart.Application/
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── CreateCart/
│   │   │   │   │   ├── AddItemToCart/
│   │   │   │   │   ├── UpdateCartItem/
│   │   │   │   │   ├── RemoveItemFromCart/
│   │   │   │   │   ├── ApplyCoupon/
│   │   │   │   │   ├── ClearCart/
│   │   │   │   │   └── MergeCarts/
│   │   │   │   ├── Queries/
│   │   │   │   │   ├── GetCartByUserId/
│   │   │   │   │   ├── GetCartBySessionId/
│   │   │   │   │   └── GetSavedCarts/
│   │   │   │   ├── DTOs/
│   │   │   │   ├── EventHandlers/
│   │   │   │   └── Cart.Application.csproj
│   │   │   │
│   │   │   ├── Cart.Infrastructure/
│   │   │   │   ├── Persistence/
│   │   │   │   │   ├── Configurations/
│   │   │   │   │   ├── Migrations/
│   │   │   │   │   └── CartDbContext.cs
│   │   │   │   ├── Repositories/
│   │   │   │   ├── DependencyInjection.cs
│   │   │   │   └── Cart.Infrastructure.csproj
│   │   │   │
│   │   │   ├── Cart.Contracts/
│   │   │   │   ├── Events/
│   │   │   │   │   └── CartConvertedIntegrationEvent.cs
│   │   │   │   └── Cart.Contracts.csproj
│   │   │   │
│   │   │   └── Cart.Presentation/
│   │   │       ├── Controllers/
│   │   │       │   └── CartController.cs
│   │   │       ├── Requests/
│   │   │       ├── DependencyInjection.cs
│   │   │       └── Cart.Presentation.csproj
│   │   │
│   │   ├── orders/                               # 📦 MÓDULO DE PEDIDOS (A IMPLEMENTAR)
│   │   │   │
│   │   │   ├── Orders.Core/
│   │   │   │   ├── Entities/
│   │   │   │   │   ├── Order.cs
│   │   │   │   │   ├── OrderItem.cs
│   │   │   │   │   ├── OrderStatusHistory.cs
│   │   │   │   │   ├── TrackingEvent.cs
│   │   │   │   │   ├── Invoice.cs
│   │   │   │   │   └── OrderRefund.cs
│   │   │   │   ├── Enums/
│   │   │   │   │   ├── OrderStatus.cs
│   │   │   │   │   ├── ShippingMethod.cs
│   │   │   │   │   └── CancellationReason.cs
│   │   │   │   ├── ValueObjects/
│   │   │   │   │   └── AddressSnapshot.cs
│   │   │   │   ├── Events/
│   │   │   │   │   ├── OrderCreatedEvent.cs
│   │   │   │   │   ├── OrderPaidEvent.cs
│   │   │   │   │   ├── OrderShippedEvent.cs
│   │   │   │   │   ├── OrderDeliveredEvent.cs
│   │   │   │   │   └── OrderCancelledEvent.cs
│   │   │   │   ├── Repositories/
│   │   │   │   │   └── IOrderRepository.cs
│   │   │   │   └── Orders.Core.csproj
│   │   │   │
│   │   │   ├── Orders.Application/
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── CreateOrder/
│   │   │   │   │   ├── ConfirmOrder/
│   │   │   │   │   ├── CancelOrder/
│   │   │   │   │   ├── ShipOrder/
│   │   │   │   │   ├── DeliverOrder/
│   │   │   │   │   └── RequestRefund/
│   │   │   │   ├── Queries/
│   │   │   │   │   ├── GetOrderById/
│   │   │   │   │   ├── GetUserOrders/
│   │   │   │   │   ├── GetOrderTracking/
│   │   │   │   │   └── SearchOrders/
│   │   │   │   ├── DTOs/
│   │   │   │   ├── EventHandlers/
│   │   │   │   │   └── PaymentCapturedIntegrationEventHandler.cs
│   │   │   │   ├── Services/
│   │   │   │   │   └── IShippingService.cs
│   │   │   │   └── Orders.Application.csproj
│   │   │   │
│   │   │   ├── Orders.Infrastructure/
│   │   │   │   ├── Persistence/
│   │   │   │   │   ├── Configurations/
│   │   │   │   │   ├── Migrations/
│   │   │   │   │   └── OrdersDbContext.cs
│   │   │   │   ├── Repositories/
│   │   │   │   ├── Services/
│   │   │   │   │   └── ShippingService.cs
│   │   │   │   ├── DependencyInjection.cs
│   │   │   │   └── Orders.Infrastructure.csproj
│   │   │   │
│   │   │   ├── Orders.Contracts/
│   │   │   │   ├── Events/
│   │   │   │   │   ├── OrderCreatedIntegrationEvent.cs
│   │   │   │   │   └── OrderPaidInteg
```
