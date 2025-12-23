Bcommerce/
│
├── BuildingBlocks/
│   │
│   ├── Bcommerce.BuildingBlocks.Domain/
│   │   ├── Abstractions/
│   │   │   ├── IEntity.cs
│   │   │   ├── IAggregateRoot.cs
│   │   │   ├── IDomainEvent.cs
│   │   │   ├── IValueObject.cs
│   │   │   └── ISoftDeletable.cs
│   │   │
│   │   ├── Base/
│   │   │   ├── Entity.cs
│   │   │   ├── AggregateRoot.cs
│   │   │   ├── ValueObject.cs
│   │   │   ├── DomainEvent.cs
│   │   │   └── Enumeration.cs
│   │   │
│   │   ├── Events/
│   │   │   ├── DomainEventDispatcher.cs
│   │   │   └── IDomainEventHandler.cs
│   │   │
│   │   └── Specifications/
│   │       ├── ISpecification.cs
│   │       ├── Specification.cs
│   │       └── CompositeSpecification.cs
│   │
│   ├── Bcommerce.BuildingBlocks.Application/
│   │   ├── Abstractions/
│   │   │   ├── Messaging/
│   │   │   │   ├── ICommand.cs
│   │   │   │   ├── ICommandHandler.cs
│   │   │   │   ├── IQuery.cs
│   │   │   │   ├── IQueryHandler.cs
│   │   │   │   ├── IIntegrationEvent.cs
│   │   │   │   └── IIntegrationEventHandler.cs
│   │   │   │
│   │   │   ├── Data/
│   │   │   │   ├── IUnitOfWork.cs
│   │   │   │   ├── IRepository.cs
│   │   │   │   └── IReadRepository.cs
│   │   │   │
│   │   │   └── Services/
│   │   │       ├── ICurrentUserService.cs
│   │   │       ├── IDateTimeProvider.cs
│   │   │       └── IEmailService.cs
│   │   │
│   │   ├── Behaviors/
│   │   │   ├── ValidationBehavior.cs
│   │   │   ├── LoggingBehavior.cs
│   │   │   ├── TransactionBehavior.cs
│   │   │   ├── CachingBehavior.cs
│   │   │   └── PerformanceBehavior.cs
│   │   │
│   │   ├── Models/
│   │   │   ├── Result.cs
│   │   │   ├── Error.cs
│   │   │   ├── PaginatedList.cs
│   │   │   ├── PagedRequest.cs
│   │   │   └── SortDescriptor.cs
│   │   │
│   │   ├── Exceptions/
│   │   │   ├── ApplicationException.cs
│   │   │   ├── ValidationException.cs
│   │   │   ├── NotFoundException.cs
│   │   │   ├── ConflictException.cs
│   │   │   └── BusinessRuleException.cs
│   │   │
│   │   └── Extensions/
│   │       ├── QueryableExtensions.cs
│   │       └── EnumerableExtensions.cs
│   │
│   ├── Bcommerce.BuildingBlocks.Infrastructure/
│   │   ├── Data/
│   │   │   ├── BaseDbContext.cs
│   │   │   ├── UnitOfWork.cs
│   │   │   ├── Repository.cs
│   │   │   ├── ReadRepository.cs
│   │   │   │
│   │   │   ├── Configurations/
│   │   │   │   ├── EntityConfiguration.cs
│   │   │   │   └── AggregateRootConfiguration.cs
│   │   │   │
│   │   │   ├── Interceptors/
│   │   │   │   ├── AuditableEntityInterceptor.cs
│   │   │   │   ├── SoftDeleteInterceptor.cs
│   │   │   │   ├── DomainEventInterceptor.cs
│   │   │   │   └── OptimisticLockInterceptor.cs
│   │   │   │
│   │   │   └── Converters/
│   │   │       ├── EnumerationConverter.cs
│   │   │       └── ValueObjectConverter.cs
│   │   │
│   │   ├── Outbox/
│   │   │   ├── Models/
│   │   │   │   └── OutboxMessage.cs
│   │   │   ├── Repositories/
│   │   │   │   ├── IOutboxRepository.cs
│   │   │   │   └── OutboxRepository.cs
│   │   │   ├── Processors/
│   │   │   │   ├── OutboxProcessor.cs
│   │   │   │   └── IOutboxPublisher.cs
│   │   │   └── Configuration/
│   │   │       └── OutboxConfiguration.cs
│   │   │
│   │   ├── Inbox/
│   │   │   ├── Models/
│   │   │   │   └── InboxMessage.cs
│   │   │   ├── Repositories/
│   │   │   │   ├── IInboxRepository.cs
│   │   │   │   └── InboxRepository.cs
│   │   │   ├── Processors/
│   │   │   │   └── InboxProcessor.cs
│   │   │   └── Configuration/
│   │   │       └── InboxConfiguration.cs
│   │   │
│   │   ├── AuditLog/
│   │   │   ├── Models/
│   │   │   │   └── AuditLog.cs
│   │   │   ├── Repositories/
│   │   │   │   ├── IAuditLogRepository.cs
│   │   │   │   └── AuditLogRepository.cs
│   │   │   └── Services/
│   │   │       └── AuditLogService.cs
│   │   │
│   │   ├── BackgroundJobs/
│   │   │   ├── IBackgroundJob.cs
│   │   │   ├── BackgroundJobRunner.cs
│   │   │   └── Jobs/
│   │   │       ├── OutboxProcessorJob.cs
│   │   │       ├── ExpiredReservationsCleanupJob.cs
│   │   │       └── AbandonedCartsJob.cs
│   │   │
│   │   └── Extensions/
│   │       └── ServiceCollectionExtensions.cs
│   │
│   ├── Bcommerce.BuildingBlocks.Messaging/
│   │   ├── Abstractions/
│   │   │   ├── IEventBus.cs
│   │   │   ├── IIntegrationEventPublisher.cs
│   │   │   └── IIntegrationEventHandler.cs
│   │   │
│   │   ├── MassTransit/
│   │   │   ├── Configuration/
│   │   │   │   ├── MassTransitConfiguration.cs
│   │   │   │   └── InMemoryConfiguration.cs
│   │   │   │
│   │   │   ├── Filters/
│   │   │   │   ├── InboxFilter.cs
│   │   │   │   ├── LoggingFilter.cs
│   │   │   │   └── ExceptionHandlingFilter.cs
│   │   │   │
│   │   │   └── Consumers/
│   │   │       └── GenericConsumer.cs
│   │   │
│   │   ├── Events/
│   │   │   ├── IntegrationEvent.cs
│   │   │   └── Shared/
│   │   │       ├── UserCreatedEvent.cs
│   │   │       ├── OrderPlacedEvent.cs
│   │   │       ├── PaymentCompletedEvent.cs
│   │   │       └── StockReservedEvent.cs
│   │   │
│   │   └── Extensions/
│   │       └── ServiceCollectionExtensions.cs
│   │
│   ├── Bcommerce.BuildingBlocks.Web/
│   │   ├── Middleware/
│   │   │   ├── ExceptionHandlingMiddleware.cs
│   │   │   ├── RequestLoggingMiddleware.cs
│   │   │   ├── CorrelationIdMiddleware.cs
│   │   │   └── PerformanceMonitoringMiddleware.cs
│   │   │
│   │   ├── Filters/
│   │   │   ├── ValidationFilter.cs
│   │   │   ├── ApiExceptionFilter.cs
│   │   │   └── AuthorizationFilter.cs
│   │   │
│   │   ├── Models/
│   │   │   ├── ApiResponse.cs
│   │   │   ├── PaginatedResponse.cs
│   │   │   ├── ErrorResponse.cs
│   │   │   └── ValidationErrorResponse.cs
│   │   │
│   │   ├── Controllers/
│   │   │   └── ApiControllerBase.cs
│   │   │
│   │   └── Extensions/
│   │       ├── ServiceCollectionExtensions.cs
│   │       └── ApplicationBuilderExtensions.cs
│   │
│   ├── Bcommerce.BuildingBlocks.Security/
│   │   ├── Authentication/
│   │   │   ├── JwtSettings.cs
│   │   │   ├── JwtTokenGenerator.cs
│   │   │   ├── ITokenGenerator.cs
│   │   │   ├── IPasswordHasher.cs
│   │   │   └── PasswordHasher.cs
│   │   │
│   │   ├── Authorization/
│   │   │   ├── Policies/
│   │   │   │   ├── PolicyNames.cs
│   │   │   │   └── Permissions.cs
│   │   │   │
│   │   │   ├── Requirements/
│   │   │   │   ├── PermissionRequirement.cs
│   │   │   │   └── ModuleAccessRequirement.cs
│   │   │   │
│   │   │   └── Handlers/
│   │   │       ├── PermissionHandler.cs
│   │   │       └── ModuleAccessHandler.cs
│   │   │
│   │   ├── Services/
│   │   │   ├── ICurrentUserService.cs
│   │   │   └── CurrentUserService.cs
│   │   │
│   │   └── Extensions/
│   │       ├── ClaimsPrincipalExtensions.cs
│   │       └── ServiceCollectionExtensions.cs
│   │
│   ├── Bcommerce.BuildingBlocks.Caching/
│   │   ├── Abstractions/
│   │   │   ├── ICacheService.cs
│   │   │   └── ICacheKeyGenerator.cs
│   │   │
│   │   ├── Redis/
│   │   │   ├── RedisCacheService.cs
│   │   │   └── RedisSettings.cs
│   │   │
│   │   ├── Memory/
│   │   │   └── MemoryCacheService.cs
│   │   │
│   │   ├── Strategies/
│   │   │   ├── CacheAsideStrategy.cs
│   │   │   └── CacheInvalidationStrategy.cs
│   │   │
│   │   └── Extensions/
│   │       └── ServiceCollectionExtensions.cs
│   │
│   └── Bcommerce.BuildingBlocks.Observability/
│       ├── Logging/
│       │   ├── LoggingConfiguration.cs
│       │   ├── SerilogEnrichers/
│       │   │   ├── CorrelationIdEnricher.cs
│       │   │   └── UserContextEnricher.cs
│       │   └── Extensions/
│       │
│       ├── Metrics/
│       │   ├── MetricsConfiguration.cs
│       │   ├── CustomMetrics/
│       │   │   ├── BusinessMetrics.cs
│       │   │   └── PerformanceMetrics.cs
│       │   └── Extensions/
│       │
│       └── Tracing/
│           ├── TracingConfiguration.cs
│           ├── ActivityExtensions.cs
│           └── Extensions/
│
├── Modules/
│   │
│   ├── Users/
│   │   ├── Bcommerce.Modules.Users.Domain/
│   │   │   ├── Entities/
│   │   │   │   ├── User.cs (ASP.NET Identity - managed by EF)
│   │   │   │   ├── Profile.cs
│   │   │   │   ├── Address.cs
│   │   │   │   ├── Session.cs
│   │   │   │   └── Notification.cs
│   │   │   │
│   │   │   ├── ValueObjects/
│   │   │   │   ├── Email.cs
│   │   │   │   ├── PhoneNumber.cs
│   │   │   │   ├── Cpf.cs
│   │   │   │   └── PostalCode.cs
│   │   │   │
│   │   │   ├── Events/
│   │   │   │   ├── UserRegisteredEvent.cs
│   │   │   │   ├── ProfileUpdatedEvent.cs
│   │   │   │   ├── AddressAddedEvent.cs
│   │   │   │   └── SessionCreatedEvent.cs
│   │   │   │
│   │   │   ├── Repositories/
│   │   │   │   ├── IUserRepository.cs
│   │   │   │   ├── IProfileRepository.cs
│   │   │   │   ├── IAddressRepository.cs
│   │   │   │   └── ISessionRepository.cs
│   │   │   │
│   │   │   └── Services/
│   │   │       └── IDomainUserService.cs
│   │   │
│   │   ├── Bcommerce.Modules.Users.Application/
│   │   │   ├── Commands/
│   │   │   │   ├── RegisterUser/
│   │   │   │   │   ├── RegisterUserCommand.cs
│   │   │   │   │   ├── RegisterUserCommandHandler.cs
│   │   │   │   │   └── RegisterUserCommandValidator.cs
│   │   │   │   │
│   │   │   │   ├── UpdateProfile/
│   │   │   │   ├── AddAddress/
│   │   │   │   └── DeleteAddress/
│   │   │   │
│   │   │   ├── Queries/
│   │   │   │   ├── GetUserById/
│   │   │   │   │   ├── GetUserByIdQuery.cs
│   │   │   │   │   └── GetUserByIdQueryHandler.cs
│   │   │   │   │
│   │   │   │   ├── GetUserProfile/
│   │   │   │   ├── GetUserAddresses/
│   │   │   │   └── GetActiveSessions/
│   │   │   │
│   │   │   ├── DTOs/
│   │   │   │   ├── UserDto.cs
│   │   │   │   ├── ProfileDto.cs
│   │   │   │   ├── AddressDto.cs
│   │   │   │   └── SessionDto.cs
│   │   │   │
│   │   │   ├── Events/
│   │   │   │   └── Handlers/
│   │   │   │       ├── UserRegisteredEventHandler.cs
│   │   │   │       └── ProfileUpdatedEventHandler.cs
│   │   │   │
│   │   │   ├── Services/
│   │   │   │   ├── IUserService.cs
│   │   │   │   └── UserService.cs
│   │   │   │
│   │   │   └── Mappings/
│   │   │       └── UserMappingProfile.cs
│   │   │
│   │   ├── Bcommerce.Modules.Users.Infrastructure/
│   │   │   ├── Persistence/
│   │   │   │   ├── UsersDbContext.cs
│   │   │   │   ├── Migrations/
│   │   │   │   │
│   │   │   │   ├── Configurations/
│   │   │   │   │   ├── ProfileConfiguration.cs
│   │   │   │   │   ├── AddressConfiguration.cs
│   │   │   │   │   ├── SessionConfiguration.cs
│   │   │   │   │   └── NotificationConfiguration.cs
│   │   │   │   │
│   │   │   │   └── Repositories/
│   │   │   │       ├── UserRepository.cs
│   │   │   │       ├── ProfileRepository.cs
│   │   │   │       ├── AddressRepository.cs
│   │   │   │       └── SessionRepository.cs
│   │   │   │
│   │   │   ├── Identity/
│   │   │   │   ├── ApplicationUser.cs
│   │   │   │   ├── ApplicationRole.cs
│   │   │   │   └── IdentityConfiguration.cs
│   │   │   │
│   │   │   ├── Services/
│   │   │   │   ├── EmailService.cs
│   │   │   │   └── SmsService.cs
│   │   │   │
│   │   │   └── Extensions/
│   │   │       └── ServiceCollectionExtensions.cs
│   │   │
│   │   └── Bcommerce.Modules.Users.Api/
│   │       ├── Controllers/
│   │       │   ├── UsersController.cs
│   │       │   ├── ProfilesController.cs
│   │       │   ├── AddressesController.cs
│   │       │   └── SessionsController.cs
│   │       │
│   │       └── Extensions/
│   │           └── ModuleExtensions.cs
│   │
│   ├── Catalog/
│   │   ├── Bcommerce.Modules.Catalog.Domain/
│   │   │   ├── Entities/
│   │   │   │   ├── Product.cs
│   │   │   │   ├── Category.cs
│   │   │   │   ├── Brand.cs
│   │   │   │   ├── ProductImage.cs
│   │   │   │   ├── ProductReview.cs
│   │   │   │   └── StockReservation.cs
│   │   │   │
│   │   │   ├── ValueObjects/
│   │   │   │   ├── Money.cs
│   │   │   │   ├── Sku.cs
│   │   │   │   ├── Slug.cs
│   │   │   │   ├── Stock.cs
│   │   │   │   ├── ProductDimensions.cs
│   │   │   │   └── Rating.cs
│   │   │   │
│   │   │   ├── Enums/
│   │   │   │   ├── ProductStatus.cs
│   │   │   │   └── StockMovementType.cs
│   │   │   │
│   │   │   ├── Events/
│   │   │   │   ├── ProductCreatedEvent.cs
│   │   │   │   ├── ProductPublishedEvent.cs
│   │   │   │   ├── StockReservedEvent.cs
│   │   │   │   ├── StockReleasedEvent.cs
│   │   │   │   └── ReviewAddedEvent.cs
│   │   │   │
│   │   │   ├── Repositories/
│   │   │   │   ├── IProductRepository.cs
│   │   │   │   ├── ICategoryRepository.cs
│   │   │   │   ├── IBrandRepository.cs
│   │   │   │   └── IStockReservationRepository.cs
│   │   │   │
│   │   │   └── Services/
│   │   │       ├── IStockService.cs
│   │   │       └── ISlugGenerator.cs
│   │   │
│   │   ├── Bcommerce.Modules.Catalog.Application/
│   │   │   ├── Commands/
│   │   │   │   ├── CreateProduct/
│   │   │   │   ├── UpdateProduct/
│   │   │   │   ├── PublishProduct/
│   │   │   │   ├── ReserveStock/
│   │   │   │   ├── ReleaseStock/
│   │   │   │   └── AddProductReview/
│   │   │   │
│   │   │   ├── Queries/
│   │   │   │   ├── GetProductById/
│   │   │   │   ├── GetProducts/
│   │   │   │   ├── SearchProducts/
│   │   │   │   ├── GetProductsByCategory/
│   │   │   │   └── GetProductReviews/
│   │   │   │
│   │   │   ├── DTOs/
│   │   │   │   ├── ProductDto.cs
│   │   │   │   ├── ProductDetailDto.cs
│   │   │   │   ├── CategoryDto.cs
│   │   │   │   ├── BrandDto.cs
│   │   │   │   └── ProductReviewDto.cs
│   │   │   │
│   │   │   ├── Events/
│   │   │   │   └── Handlers/
│   │   │   │       ├── ProductPublishedEventHandler.cs
│   │   │   │       └── StockReservedEventHandler.cs
│   │   │   │
│   │   │   ├── Services/
│   │   │   │   ├── IProductService.cs
│   │   │   │   ├── ProductService.cs
│   │   │   │   ├── IStockService.cs
│   │   │   │   └── StockService.cs
│   │   │   │
│   │   │   └── Mappings/
│   │   │       └── CatalogMappingProfile.cs
│   │   │
│   │   ├── Bcommerce.Modules.Catalog.Infrastructure/
│   │   │   ├── Persistence/
│   │   │   │   ├── CatalogDbContext.cs
│   │   │   │   ├── Migrations/
│   │   │   │   │
│   │   │   │   ├── Configurations/
│   │   │   │   │   ├── ProductConfiguration.cs
│   │   │   │   │   ├── CategoryConfiguration.cs
│   │   │   │   │   ├── BrandConfiguration.cs
│   │   │   │   │   ├── ProductImageConfiguration.cs
│   │   │   │   │   └── StockReservationConfiguration.cs
│   │   │   │   │
│   │   │   │   └── Repositories/
│   │   │   │       ├── ProductRepository.cs
│   │   │   │       ├── CategoryRepository.cs
│   │   │   │       ├── BrandRepository.cs
│   │   │   │       └── StockReservationRepository.cs
│   │   │   │
│   │   │   ├── Services/
│   │   │   │   ├── SlugGenerator.cs
│   │   │   │   └── ImageStorageService.cs
│   │   │   │
│   │   │   └── Extensions/
│   │   │       └── ServiceCollectionExtensions.cs
│   │   │
│   │   └── Bcommerce.Modules.Catalog.Api/
│   │       ├── Controllers/
│   │       │   ├── ProductsController.cs
│   │       │   ├── CategoriesController.cs
│   │       │   ├── BrandsController.cs
│   │       │   └── ReviewsController.cs
│   │       │
│   │       └── Extensions/
│   │           └── ModuleExtensions.cs
│   │
│   ├── Cart/
│   │   ├── Bcommerce.Modules.Cart.Domain/
│   │   │   ├── Entities/
│   │   │   │   ├── ShoppingCart.cs
│   │   │   │   ├── CartItem.cs
│   │   │   │   └── SavedCart.cs
│   │   │   │
│   │   │   ├── ValueObjects/
│   │   │   │   ├── CartId.cs
│   │   │   │   ├── SessionId.cs
│   │   │   │   └── ProductSnapshot.cs
│   │   │   │
│   │   │   ├── Enums/
│   │   │   │   └── CartStatus.cs
│   │   │   │
│   │   │   ├── Events/
│   │   │   │   ├── CartCreatedEvent.cs
│   │   │   │   ├── ItemAddedToCartEvent.cs
│   │   │   │   ├── ItemRemovedFromCartEvent.cs
│   │   │   │   ├── CartConvertedEvent.cs
│   │   │   │   └── CartAbandonedEvent.cs
│   │   │   │
│   │   │   ├── Repositories/
│   │   │   │   └── ICartRepository.cs
│   │   │   │
│   │   │   └── Services/
│   │   │       └── ICartDomainService.cs
│   │   │
│   │   ├── Bcommerce.Modules.Cart.Application/
│   │   │   ├── Commands/
│   │   │   │   ├── CreateCart/
│   │   │   │   ├── AddItemToCart/
│   │   │   │   ├── UpdateCartItem/
│   │   │   │   ├── RemoveCartItem/
│   │   │   │   ├── ApplyCoupon/
│   │   │   │   └── ClearCart/
│   │   │   │
│   │   │   ├── Queries/
│   │   │   │   ├── GetCart/
│   │   │   │   ├── GetCartByUser/
│   │   │   │   └── GetAbandonedCarts/
│   │   │   │
│   │   │   ├── DTOs/
│   │   │   │   ├── CartDto.cs
│   │   │   │   ├── CartItemDto.cs
│   │   │   │   └── CartSummaryDto.cs
│   │   │   │
│   │   │   ├── Events/
│   │   │   │   └── Handlers/
│   │   │   │       ├── ItemAddedToCartEventHandler.cs
│   │   │   │       └── CartConvertedEventHandler.cs
│   │   │   │
│   │   │   ├── Services/
│   │   │   │   ├── ICartService.cs
│   │   │   │   └── CartService.cs
│   │   │   │
│   │   │   └── Mappings/
│   │   │       └── CartMappingProfile.cs
│   │   │
│   │   ├── Bcommerce.Modules.Cart.Infrastructure/
│   │   │   ├── Persistence/
│   │   │   │   ├── CartDbContext.cs
│   │   │   │   ├── Migrations/
│   │   │   │   │
│   │   │   │   ├── Configurations/
│   │   │   │   │   ├── ShoppingCartConfiguration.cs
│   │   │   │   │   └── CartItemConfiguration.cs
│   │   │   │   │
│   │   │   │   └── Repositories/
│   │   │   │       └── CartRepository.cs
│   │   │   │
│   │   │   └── Extensions/
│   │   │       └── ServiceCollectionExtensions.cs
│   │   │
│   │   └── Bcommerce.Modules.Cart.Api/
│   │       ├── Controllers/
│   │       │   └── CartController.cs
│   │       │
│   │       └── Extensions/
│   │           └── ModuleExtensions.cs
│   │
│   ├── Orders/
│   │   ├── Bcommerce.Modules.Orders.Domain/
│   │   │   ├── Entities/
│   │   │   │   ├── Order.cs
│   │   │   │   ├── OrderItem.cs
│   │   │   │   ├── OrderStatusHistory.cs
│   │   │   │   ├── TrackingEvent.cs
│   │   │   │   ├── Invoice.cs
│   │   │   │   └── OrderRefund.cs
│   │   │   │
│   │   │   ├── ValueObjects/
│   │   │   │   ├── OrderNumber.cs
│   │   │   │   ├── ShippingAddress.cs
│   │   │   │   ├── TrackingCode.cs
│   │   │   │   └── OrderTotal.cs
│   │   │   │
│   │   │   ├── Enums/
│   │   │   │   ├── OrderStatus.cs
│   │   │   │   ├── ShippingMethod.cs
│   │   │   │   └── CancellationReason.cs
│   │   │   │
│   │   │   ├── Events/
│   │   │   │   ├── OrderPlacedEvent.cs
│   │   │   │   ├── OrderPaidEvent.cs
│   │   │   │   ├── OrderShippedEvent.cs
│   │   │   │   ├── OrderDeliveredEvent.cs
│   │   │   │   └── OrderCancelledEvent.cs
│   │   │   │
│   │   │   ├── Repositories/
│   │   │   │   └── IOrderRepository.cs
│   │   │   │
│   │   │   └── Services/
│   │   │       ├── IOrderDomainService.cs
│   │   │       └── OrderStateMachine.cs
│   │   │
│   │   ├── Bcommerce.Modules.Orders.Application/
│   │   │   ├── Commands/
│   │   │   │   ├── PlaceOrder/
│   │   │   │   ├── ConfirmPayment/
│   │   │   │   ├── ShipOrder/
│   │   │   │   ├── CancelOrder/
│   │   │   │   └── RequestRefund/
│   │   │   │
│   │   │   ├── Queries/
│   │   │   │   ├── GetOrderById/
│   │   │   │   ├── GetUserOrders/
│   │   │   │   ├── GetOrderStatus/
│   │   │   │   └── GetOrderTracking/
│   │   │   │
│   │   │   ├── DTOs/
│   │   │   │   ├── OrderDto.cs
│   │   │   │   ├── OrderDetailDto.cs
│   │   │   │   ├── OrderItemDto.cs
│   │   │   │   └── TrackingDto.cs
│   │   │   │
│   │   │   ├── Events/
│   │   │   │   └── Handlers/
│   │   │   │       ├── OrderPlacedEventHandler.cs
│   │   │   │       ├── OrderPaidEventHandler.cs
│   │   │   │       └── OrderShippedEventHandler.cs
│   │   │   │
│   │   │   ├── Services/
│   │   │   │   ├── IOrderService.cs
│   │   │   │   └── OrderService.cs
│   │   │   │
│   │   │   └── Mappings/
│   │   │       └── OrderMappingProfile.cs
│   │   │
│   │   ├── Bcommerce.Modules.Orders.Infrastructure/
│   │   │   ├── Persistence/
│   │   │   │   ├── OrdersDbContext.cs
│   │   │   │   ├── Migrations/
│   │   │   │   │
│   │   │   │   ├── Configurations/
│   │   │   │   │   ├── OrderConfiguration.cs
│   │   │   │   │   ├── OrderItemConfiguration.cs
│   │   │   │   │   └── OrderStatusHistoryConfiguration.cs
│   │   │   │   │
│   │   │   │   └── Repositories/
│   │   │   │       └── OrderRepository.cs
│   │   │   │
│   │   │   ├── Services/
│   │   │   │   ├── ShippingService.cs
│   │   │   │   └── InvoiceService.cs
│   │   │   │
│   │   │   └── Extensions/
│   │   │       └── ServiceCollectionExtensions.cs
│   │   │
│   │   └── Bcommerce.Modules.Orders.Api/
│   │       ├── Controllers/
│   │       │   ├── OrdersController.cs
│   │       │   └── TrackingController.cs
│   │       │
│   │       └── Extensions/
│   │           └── ModuleExtensions.cs
│   │
│   ├── Payments/
│   │   ├── Bcommerce.Modules.Payments.Domain/
│   │   │   ├── Entities/
│   │   │   │   ├── Payment.cs
│   │   │   │   ├── PaymentMethod.cs
│   │   │   │   ├── PaymentTransaction.cs
│   │   │   │   ├── PaymentRefund.cs
│   │   │   │   └── Chargeback.cs
│   │   │   │
│   │   │   ├── ValueObjects/
│   │   │   │   ├── PaymentAmount.cs
│   │   │   │   ├── CardDetails.cs
│   │   │   │   ├── PixData.cs
│   │   │   │   └── BoletoData.cs
│   │   │   │
│   │   │   ├── Enums/
│   │   │   │   ├── PaymentStatus.cs
│   │   │   │   ├── PaymentMethodType.cs
│   │   │   │   ├── TransactionType.cs
│   │   │   │   └── CardBrand.cs
│   │   │   │
│   │   │   ├── Events/
│   │   │   │   ├── PaymentInitiatedEvent.cs
│   │   │   │   ├── PaymentAuthorizedEvent.cs
│   │   │   │   ├── PaymentCapturedEvent.cs
│   │   │   │   ├── PaymentFailedEvent.cs
│   │   │   │   └── RefundProcessedEvent.cs
│   │   │   │
│   │   │   ├── Repositories/
│   │   │   │   ├── IPaymentRepository.cs
│   │   │   │   └── IPaymentMethodRepository.cs
│   │   │   │
│   │   │   └── Services/
│   │   │       ├── IPaymentGateway.cs
│   │   │       └── IPaymentDomainService.cs
│   │   │
│   │   ├── Bcommerce.Modules.Payments.Application/
│   │   │   ├── Commands/
│   │   │   │   ├── ProcessPayment/
│   │   │   │   ├── CapturePayment/
│   │   │   │   ├── RefundPayment/
│   │   │   │   └── SavePaymentMethod/
│   │   │   │
│   │   │   ├── Queries/
│   │   │   │   ├── GetPaymentStatus/
│   │   │   │   ├── GetPaymentMethods/
│   │   │   │   └── GetPaymentHistory/
│   │   │   │
│   │   │   ├── DTOs/
│   │   │   │   ├── PaymentDto.cs
│   │   │   │   ├── PaymentMethodDto.cs
│   │   │   │   └── PaymentResultDto.cs
│   │   │   │
│   │   │   ├── Events/
│   │   │   │   └── Handlers/
│   │   │   │       ├── PaymentCapturedEventHandler.cs
│   │   │   │       └── PaymentFailedEventHandler.cs
│   │   │   │
│   │   │   ├── Services/
│   │   │   │   ├── IPaymentService.cs
│   │   │   │   └── PaymentService.cs
│   │   │   │
│   │   │   └── Mappings/
│   │   │       └── PaymentMappingProfile.cs
│   │   │
│   │   ├── Bcommerce.Modules.Payments.Infrastructure/
│   │   │   ├── Persistence/
│   │   │   │   ├── PaymentsDbContext.cs
│   │   │   │   ├── Migrations/
│   │   │   │   │
│   │   │   │   ├── Configurations/
│   │   │   │   │   ├── PaymentConfiguration.cs
│   │   │   │   │   └── PaymentMethodConfiguration.cs
│   │   │   │   │
│   │   │   │   └── Repositories/
│   │   │   │       ├── PaymentRepository.cs
│   │   │   │       └── PaymentMethodRepository.cs
│   │   │   │
│   │   │   ├── Gateways/
│   │   │   │   ├── Stripe/
│   │   │   │   │   ├── StripeGateway.cs
│   │   │   │   │   └── StripeConfiguration.cs
│   │   │   │   │
│   │   │   │   ├── MercadoPago/
│   │   │   │   │   ├── MercadoPagoGateway.cs
│   │   │   │   │   └── MercadoPagoConfiguration.cs
│   │   │   │   │
│   │   │   │   └── Abstractions/
│   │   │   │       └── PaymentGatewayBase.cs
│   │   │   │
│   │   │   ├── Webhooks/
│   │   │   │   ├── IWebhookProcessor.cs
│   │   │   │   └── WebhookProcessor.cs
│   │   │   │
│   │   │   └── Extensions/
│   │   │       └── ServiceCollectionExtensions.cs
│   │   │
│   │   └── Bcommerce.Modules.Payments.Api/
│   │       ├── Controllers/
│   │       │   ├── PaymentsController.cs
│   │       │   ├── PaymentMethodsController.cs
│   │       │   └── WebhooksController.cs
│   │       │
│   │       └── Extensions/
│   │           └── ModuleExtensions.cs
│   │
│   └── Coupons/
│       ├── Bcommerce.Modules.Coupons.Domain/
│       │   ├── Entities/
│       │   │   ├── Coupon.cs
│       │   │   ├── CouponUsage.cs
│       │   │   ├── CouponReservation.cs
│       │   │   └── CouponEligibility.cs
│       │   │
│       │   ├── ValueObjects/
│       │   │   ├── CouponCode.cs
│       │   │   ├── DiscountValue.cs
│       │   │   └── ValidityPeriod.cs
│       │   │
│       │   ├── Enums/
│       │   │   ├── CouponType.cs
│       │   │   ├── CouponStatus.cs
│       │   │   └── CouponScope.cs
│       │   │
│       │   ├── Events/
│       │   │   ├── CouponCreatedEvent.cs
│       │   │   ├── CouponUsedEvent.cs
│       │   │   ├── CouponExpiredEvent.cs
│       │   │   └── CouponDepletedEvent.cs
│       │   │
│       │   ├── Repositories/
│       │   │   └── ICouponRepository.cs
│       │   │
│       │   └── Services/
│       │       └── ICouponValidator.cs
│       │
│       ├── Bcommerce.Modules.Coupons.Application/
│       │   ├── Commands/
│       │   │   ├── CreateCoupon/
│       │   │   ├── ActivateCoupon/
│       │   │   ├── DeactivateCoupon/
│       │   │   ├── ApplyCoupon/
│       │   │   └── ReleaseCouponReservation/
│       │   │
│       │   ├── Queries/
│       │   │   ├── GetCouponByCode/
│       │   │   ├── GetActiveCoupons/
│       │   │   ├── ValidateCoupon/
│       │   │   └── GetCouponUsageStats/
│       │   │
│       │   ├── DTOs/
│       │   │   ├── CouponDto.cs
│       │   │   ├── CouponValidationDto.cs
│       │   │   └── CouponStatsDto.cs
│       │   │
│       │   ├── Events/
│       │   │   └── Handlers/
│       │   │       ├── CouponUsedEventHandler.cs
│       │   │       └── CouponExpiredEventHandler.cs
│       │   │
│       │   ├── Services/
│       │   │   ├── ICouponService.cs
│       │   │   └── CouponService.cs
│       │   │
│       │   └── Mappings/
│       │       └── CouponMappingProfile.cs
│       │
│       ├── Bcommerce.Modules.Coupons.Infrastructure/
│       │   ├── Persistence/
│       │   │   ├── CouponsDbContext.cs
│       │   │   ├── Migrations/
│       │   │   │
│       │   │   ├── Configurations/
│       │   │   │   ├── CouponConfiguration.cs
│       │   │   │   └── CouponUsageConfiguration.cs
│       │   │   │
│       │   │   └── Repositories/
│       │   │       └── CouponRepository.cs
│       │   │
│       │   └── Extensions/
│       │       └── ServiceCollectionExtensions.cs
│       │
│       └── Bcommerce.Modules.Coupons.Api/
│           ├── Controllers/
│           │   └── CouponsController.cs
│           │
│           └── Extensions/
│               └── ModuleExtensions.cs
│
├── Host/
│   └── Bcommerce.Host.WebApi/
│       ├── Program.cs
│       ├── Startup.cs
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       ├── appsettings.Production.json
│       │
│       ├── Configuration/
│       │   ├── ModulesConfiguration.cs
│       │   ├── DatabaseConfiguration.cs
│       │   ├── MassTransitConfiguration.cs
│       │   └── SwaggerConfiguration.cs
│       │
│       ├── BackgroundServices/
│       │   ├── OutboxProcessorService.cs
│       │   ├── InboxProcessorService.cs
│       │   └── StockReservationCleanupService.cs
│       │
│       └── Extensions/
│           └── HostExtensions.cs
│
├── Tests/
│   ├── UnitTests/
│   │   ├── Bcommerce.Modules.Users.Domain.Tests/
│   │   ├── Bcommerce.Modules.Catalog.Domain.Tests/
│   │   ├── Bcommerce.Modules.Cart.Domain.Tests/
│   │   ├── Bcommerce.Modules.Orders.Domain.Tests/
│   │   ├── Bcommerce.Modules.Payments.Domain.Tests/
│   │   └── Bcommerce.Modules.Coupons.Domain.Tests/
│   │
│   ├── IntegrationTests/
│   │   ├── Bcommerce.Modules.Users.IntegrationTests/
│   │   ├── Bcommerce.Modules.Catalog.IntegrationTests/
│   │   ├── Bcommerce.Modules.Cart.IntegrationTests/
│   │   ├── Bcommerce.Modules.Orders.IntegrationTests/
│   │   ├── Bcommerce.Modules.Payments.IntegrationTests/
│   │   └── Bcommerce.Modules.Coupons.IntegrationTests/
│   │
│   ├── ArchitectureTests/
│   │   └── Bcommerce.ArchitectureTests/
│   │       ├── ModularityTests.cs
│   │       ├── DependencyTests.cs
│   │       └── LayerTests.cs
│   │
│   └── TestHelpers/
│       ├── Bcommerce.TestHelpers/
│       │   ├── Builders/
│       │   ├── Fixtures/
│       │   └── Mocks/
│       │
│       └── Bcommerce.TestHelpers.Integration/
│           ├── DatabaseFixture.cs
│           ├── WebApplicationFactoryFixture.cs
│           └── TestContainers/
│
├── Scripts/
│   ├── Database/
│   │   ├── schema.sql
│   │   ├── seed-data.sql
│   │   └── migrations/
│   │
│   ├── Docker/
│   │   ├── Dockerfile
│   │   ├── docker-compose.yml
│   │   └── docker-compose.override.yml
│   │
│   └── CI-CD/
│       ├── build.sh
│       ├── test.sh
│       └── deploy.sh
│
├── Docs/
│   ├── Architecture/
│   │   ├── ADR/
│   │   │   ├── 001-modular-monolith.md
│   │   │   ├── 002-masstransit-in-memory.md
│   │   │   └── 003-outbox-inbox-pattern.md
│   │   │
│   │   ├── diagrams/
│   │   │   ├── system-context.puml
│   │   │   ├── module-dependencies.puml
│   │   │   └── data-flow.puml
│   │   │
│   │   └── README.md
│   │
│   └── API/
│       ├── swagger.json
│       └── postman-collection.json
│
├── .github/
│   └── workflows/
│       ├── ci.yml
│       ├── cd.yml
│       └── pr-validation.yml
│
├── .editorconfig
├── .gitignore
├── Directory.Build.props
├── Directory.Packages.props
├── global.json
├── nuget.config
├── Bcommerce.sln
└── README.md