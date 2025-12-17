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
│   │   │   ├── ISoftDeletable.cs
│   │   │   └── IVersionable.cs
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
│   │   │   ├── IDomainEventHandler.cs
│   │   │   └── DomainEventBase.cs
│   │   │
│   │   ├── Specifications/
│   │   │   ├── ISpecification.cs
│   │   │   ├── Specification.cs
│   │   │   └── CompositeSpecification.cs
│   │   │
│   │   └── Exceptions/
│   │       ├── DomainException.cs
│   │       ├── InvalidValueObjectException.cs
│   │       └── BusinessRuleException.cs
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
│   │   │   ├── PerformanceBehavior.cs
│   │   │   └── IdempotencyBehavior.cs
│   │   │
│   │   ├── Models/
│   │   │   ├── Result.cs
│   │   │   ├── Error.cs
│   │   │   ├── ErrorType.cs
│   │   │   ├── PaginatedList.cs
│   │   │   ├── PagedRequest.cs
│   │   │   └── SortDescriptor.cs
│   │   │
│   │   ├── Exceptions/
│   │   │   ├── ApplicationException.cs
│   │   │   ├── ValidationException.cs
│   │   │   ├── NotFoundException.cs
│   │   │   ├── ConflictException.cs
│   │   │   ├── UnauthorizedException.cs
│   │   │   └── ForbiddenException.cs
│   │   │
│   │   └── Extensions/
│   │       ├── QueryableExtensions.cs
│   │       ├── EnumerableExtensions.cs
│   │       └── ResultExtensions.cs
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
│   │   │       ├── ValueObjectConverter.cs
│   │   │       └── DateTimeUtcConverter.cs
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
│   │   │   ├── Configuration/
│   │   │   │   └── OutboxConfiguration.cs
│   │   │   └── BackgroundJobs/
│   │   │       └── OutboxProcessorJob.cs
│   │   │
│   │   ├── Inbox/
│   │   │   ├── Models/
│   │   │   │   └── InboxMessage.cs
│   │   │   ├── Repositories/
│   │   │   │   ├── IInboxRepository.cs
│   │   │   │   └── InboxRepository.cs
│   │   │   ├── Processors/
│   │   │   │   └── InboxProcessor.cs
│   │   │   ├── Configuration/
│   │   │   │   └── InboxConfiguration.cs
│   │   │   └── BackgroundJobs/
│   │   │       └── InboxProcessorJob.cs
│   │   │
│   │   ├── AuditLog/
│   │   │   ├── Models/
│   │   │   │   └── AuditLog.cs
│   │   │   ├── Repositories/
│   │   │   │   ├── IAuditLogRepository.cs
│   │   │   │   └── AuditLogRepository.cs
│   │   │   └── Services/
│   │   │       ├── IAuditLogService.cs
│   │   │       └── AuditLogService.cs
│   │   │
│   │   ├── BackgroundJobs/
│   │   │   ├── IBackgroundJob.cs
│   │   │   ├── BackgroundJobRunner.cs
│   │   │   └── Jobs/
│   │   │       ├── ExpiredReservationsCleanupJob.cs
│   │   │       ├── AbandonedCartsJob.cs
│   │   │       ├── ExpiredPaymentsJob.cs
│   │   │       ├── ExpiredCouponsJob.cs
│   │   │       └── MaterializedViewRefreshJob.cs
│   │   │
│   │   ├── Time/
│   │   │   ├── IDateTimeProvider.cs
│   │   │   └── DateTimeProvider.cs
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
│   │   │   │   ├── ExceptionHandlingFilter.cs
│   │   │   │   └── IdempotencyFilter.cs
│   │   │   │
│   │   │   └── Consumers/
│   │   │       └── GenericConsumer.cs
│   │   │
│   │   ├── Events/
│   │   │   ├── IntegrationEvent.cs
│   │   │   └── Shared/
│   │   │       ├── UserRegisteredEvent.cs
│   │   │       ├── ProductPublishedEvent.cs
│   │   │       ├── StockReservedEvent.cs
│   │   │       ├── StockReleasedEvent.cs
│   │   │       ├── CartConvertedEvent.cs
│   │   │       ├── OrderPlacedEvent.cs
│   │   │       ├── OrderStatusChangedEvent.cs
│   │   │       ├── PaymentCompletedEvent.cs
│   │   │       ├── PaymentFailedEvent.cs
│   │   │       └── CouponUsedEvent.cs
│   │   │
│   │   └── Extensions/
│   │       └── ServiceCollectionExtensions.cs
│   │
│   ├── Bcommerce.BuildingBlocks.Web/
│   │   ├── Middleware/
│   │   │   ├── ExceptionHandlingMiddleware.cs
│   │   │   ├── RequestLoggingMiddleware.cs
│   │   │   ├── CorrelationIdMiddleware.cs
│   │   │   ├── PerformanceMonitoringMiddleware.cs
│   │   │   └── TenantResolutionMiddleware.cs
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
│   │   │   ├── PasswordHasher.cs
│   │   │   └── RefreshTokenService.cs
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
│       │       └── ServiceCollectionExtensions.cs
│       │
│       ├── Metrics/
│       │   ├── MetricsConfiguration.cs
│       │   ├── CustomMetrics/
│       │   │   ├── BusinessMetrics.cs
│       │   │   └── PerformanceMetrics.cs
│       │   └── Extensions/
│       │       └── ServiceCollectionExtensions.cs
│       │
│       └── Tracing/
│           ├── TracingConfiguration.cs
│           ├── ActivityExtensions.cs
│           └── Extensions/
│               └── ServiceCollectionExtensions.cs
│
├── Modules/
│   │
│   ├── Users/
│   │   ├── Bcommerce.Modules.Users.Domain/
│   │   │   ├── Entities/
│   │   │   │   ├── ApplicationUser.cs (extends IdentityUser)
│   │   │   │   ├── Profile.cs
│   │   │   │   ├── Address.cs
│   │   │   │   ├── Session.cs
│   │   │   │   ├── Notification.cs
│   │   │   │   ├── NotificationPreference.cs
│   │   │   │   └── LoginHistory.cs
│   │   │   │
│   │   │   ├── ValueObjects/
│   │   │   │   ├── Email.cs
│   │   │   │   ├── PhoneNumber.cs
│   │   │   │   ├── Cpf.cs
│   │   │   │   ├── PostalCode.cs
│   │   │   │   ├── DeviceInfo.cs
│   │   │   │   └── GeoLocation.cs
│   │   │   │
│   │   │   ├── Events/
│   │   │   │   ├── UserRegisteredEvent.cs
│   │   │   │   ├── ProfileUpdatedEvent.cs
│   │   │   │   ├── AddressAddedEvent.cs
│   │   │   │   ├── AddressUpdatedEvent.cs
│   │   │   │   ├── AddressDeletedEvent.cs
│   │   │   │   ├── SessionCreatedEvent.cs
│   │   │   │   ├── SessionRevokedEvent.cs
│   │   │   │   ├── PasswordChangedEvent.cs
│   │   │   │   └── UserDeletedEvent.cs
│   │   │   │
│   │   │   ├── Repositories/
│   │   │   │   ├── IUserRepository.cs
│   │   │   │   ├── IProfileRepository.cs
│   │   │   │   ├── IAddressRepository.cs
│   │   │   │   ├── ISessionRepository.cs
│   │   │   │   └── INotificationRepository.cs
│   │   │   │
│   │   │   └── Services/
│   │   │       └── IUserDomainService.cs
│   │   │
│   │   ├── Bcommerce.Modules.Users.Application/
│   │   │   ├── Commands/
│   │   │   │   ├── RegisterUser/
│   │   │   │   │   ├── RegisterUserCommand.cs
│   │   │   │   │   ├── RegisterUserCommandHandler.cs
│   │   │   │   │   └── RegisterUserCommandValidator.cs
│   │   │   │   │
│   │   │   │   ├── Login/
│   │   │   │   │   ├── LoginCommand.cs
│   │   │   │   │   ├── LoginCommandHandler.cs
│   │   │   │   │   └── LoginCommandValidator.cs
│   │   │   │   │
│   │   │   │   ├── RefreshToken/
│   │   │   │   │   ├── RefreshTokenCommand.cs
│   │   │   │   │   └── RefreshTokenCommandHandler.cs
│   │   │   │   │
│   │   │   │   ├── UpdateProfile/
│   │   │   │   │   ├── UpdateProfileCommand.cs
│   │   │   │   │   ├── UpdateProfileCommandHandler.cs
│   │   │   │   │   └── UpdateProfileCommandValidator.cs
│   │   │   │   │
│   │   │   │   ├── AddAddress/
│   │   │   │   │   ├── AddAddressCommand.cs
│   │   │   │   │   ├── AddAddressCommandHandler.cs
│   │   │   │   │   └── AddAddressCommandValidator.cs
│   │   │   │   │
│   │   │   │   ├── UpdateAddress/
│   │   │   │   ├── DeleteAddress/
│   │   │   │   ├── SetDefaultAddress/
│   │   │   │   ├── RevokeSession/
│   │   │   │   ├── MarkNotificationAsRead/
│   │   │   │   └── UpdateNotificationPreferences/
│   │   │   │
│   │   │   ├── Queries/
│   │   │   │   ├── GetUserById/
│   │   │   │   │   ├── GetUserByIdQuery.cs
│   │   │   │   │   └── GetUserByIdQueryHandler.cs
│   │   │   │   │
│   │   │   │   ├── GetUserProfile/
│   │   │   │   ├── GetUserAddresses/
│   │   │   │   ├── GetActiveSessions/
│   │   │   │   ├── GetNotifications/
│   │   │   │   ├── GetLoginHistory/
│   │   │   │   └── GetNotificationPreferences/
│   │   │   │
│   │   │   ├── DTOs/
│   │   │   │   ├── UserDto.cs
│   │   │   │   ├── ProfileDto.cs
│   │   │   │   ├── AddressDto.cs
│   │   │   │   ├── SessionDto.cs
│   │   │   │   ├── NotificationDto.cs
│   │   │   │   ├── LoginResponseDto.cs
│   │   │   │   └── TokenDto.cs
│   │   │   │
│   │   │   ├── Events/
│   │   │   │   └── Handlers/
│   │   │   │       ├── UserRegisteredEventHandler.cs
│   │   │   │       ├── ProfileUpdatedEventHandler.cs
│   │   │   │       └── SessionCreatedEventHandler.cs
│   │   │   │
│   │   │   ├── Services/
│   │   │   │   ├── IUserService.cs
│   │   │   │   ├── UserService.cs
│   │   │   │   ├── IAuthService.cs
│   │   │   │   └── AuthService.cs
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
│   │   │   │   │   ├── ApplicationUserConfiguration.cs
│   │   │   │   │   ├── ProfileConfiguration.cs
│   │   │   │   │   ├── AddressConfiguration.cs
│   │   │   │   │   ├── SessionConfiguration.cs
│   │   │   │   │   ├── NotificationConfiguration.cs
│   │   │   │   │   └── LoginHistoryConfiguration.cs
│   │   │   │   │
│   │   │   │   └── Repositories/
│   │   │   │       ├── UserRepository.cs
│   │   │   │       ├── ProfileRepository.cs
│   │   │   │       ├── AddressRepository.cs
│   │   │   │       ├── SessionRepository.cs
│   │   │   │       └── NotificationRepository.cs
│   │   │   │
│   │   │   ├── Identity/
│   │   │   │   ├── ApplicationUser.cs
│   │   │   │   ├── ApplicationRole.cs
│   │   │   │   ├── IdentityConfiguration.cs
│   │   │   │   └── IdentityService.cs
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
│   │       │   ├── AuthController.cs
│   │       │   ├── UsersController.cs
│   │       │   ├── ProfilesController.cs
│   │       │   ├── AddressesController.cs
│   │       │   ├── SessionsController.cs
│   │       │   └── NotificationsController.cs
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
│   │   │   │   ├── StockMovement.cs
│   │   │   │   ├── StockReservation.cs
│   │   │   │   └── UserFavorite.cs
│   │   │   │
│   │   │   ├── ValueObjects/
│   │   │   │   ├── Money.cs
│   │   │   │   ├── Sku.cs
│   │   │   │   ├── Slug.cs
│   │   │   │   ├── Stock.cs
│   │   │   │   ├── ProductDimensions.cs
│   │   │   │   ├── Rating.cs
│   │   │   │   └── CategoryPath.cs
│   │   │   │
│   │   │   ├── Enums/
│   │   │   │   ├── ProductStatus.cs
│   │   │   │   └── StockMovementType.cs
│   │   │   │
│   │   │   ├── Events/
│   │   │   │   ├── ProductCreatedEvent.cs
│   │   │   │   ├── ProductUpdatedEvent.cs
│   │   │   │   ├── ProductPublishedEvent.cs
│   │   │   │   ├── ProductDeletedEvent.cs
│   │   │   │   ├── StockUpdatedEvent.cs
│   │   │   │   ├── StockReservedEvent.cs
│   │   │   │   ├── StockReleasedEvent.cs
│   │   │   │   ├── ReviewAddedEvent.cs
│   │   │   │   ├── ReviewApprovedEvent.cs
│   │   │   │   └── ProductFavoritedEvent.cs
│   │   │   │
│   │   │   ├── Repositories/
│   │   │   │   ├── IProductRepository.cs
│   │   │   │   ├── ICategoryRepository.cs
│   │   │   │   ├── IBrandRepository.cs
│   │   │   │   ├── IProductReviewRepository.cs
│   │   │   │   └── IStockReservationRepository.cs
│   │   │   │
│   │   │   └── Services/
│   │   │       ├── IStockService.cs
│   │   │       ├── ISlugGenerator.cs
│   │   │       └── IPriceCalculator.cs
│   │   │
│   │   ├── Bcommerce.Modules.Catalog.Application/
│   │   │   ├── Commands/
│   │   │   │   ├── CreateProduct/
│   │   │   │   │   ├── CreateProductCommand.cs
│   │   │   │   │   ├── CreateProductCommandHandler.cs
│   │   │   │   │   └── CreateProductCommandValidator.cs
│   │   │   │   │
│   │   │   │   ├── UpdateProduct/
│   │   │   │   ├── DeleteProduct/
│   │   │   │   ├── PublishProduct/
│   │   │   │   ├── UpdateStock/
│   │   │   │   ├── ReserveStock/
│   │   │   │   ├── ReleaseStock/
│   │   │   │   ├── AddProductReview/
│   │   │   │   ├── ApproveReview/
│   │   │   │   ├── AddToFavorites/
│   │   │   │   ├── RemoveFromFavorites/
│   │   │   │   ├── CreateCategory/
│   │   │   │   ├── UpdateCategory/
│   │   │   │   └── CreateBrand/
│   │   │   │
│   │   │   ├── Queries/
│   │   │   │   ├── GetProductById/
│   │   │   │   │   ├── GetProductByIdQuery.cs
│   │   │   │   │   └── GetProductByIdQueryHandler.cs
│   │   │   │   │
│   │   │   │   ├── GetProducts/
│   │   │   │   ├── SearchProducts/
│   │   │   │   ├── GetProductsByCategory/
│   │   │   │   ├── GetProductsByBrand/
│   │   │   │   ├── GetFeaturedProducts/
│   │   │   │   ├── GetProductReviews/
│   │   │   │   ├── GetCategories/
│   │   │   │   ├── GetCategoryTree/
│   │   │   │   ├── GetBrands/
│   │   │   │   ├── GetUserFavorites/
│   │   │   │   └── GetProductStats/
│   │   │   │
│   │   │   ├── DTOs/
│   │   │   │   ├── ProductDto.cs
│   │   │   │   ├── ProductDetailDto.cs
│   │   │   │   ├── ProductListDto.cs
│   │   │   │   ├── CategoryDto.cs
│   │   │   │   ├── CategoryTreeDto.cs
│   │   │   │   ├── BrandDto.cs
│   │   │   │   ├── ProductReviewDto.cs
│   │   │   │   ├── ProductStatsDto.cs
│   │   │   │   └── StockMovementDto.cs
│   │   │   │
│   │   │   ├── Events/
│   │   │   │   └── Handlers/
│   │   │   │       ├── ProductPublishedEventHandler.cs
│   │   │   │       ├── StockReservedEventHandler.cs
│   │   │   │       └── ReviewAddedEventHandler.cs
│   │   │   │
│   │   │   ├── Services/
│   │   │   │   ├── IProductService.cs
│   │   │   │   ├── ProductService.cs
│   │   │   │   ├── IStockService.cs
│   │   │   │   ├── StockService.cs
│   │   │   │   ├── ICategoryService.cs
│   │   │   │   └── CategoryService.cs
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
│   │   │   │   │   ├── ProductReviewConfiguration.cs
│   │   │   │   │   ├── StockMovementConfiguration.cs
│   │   │   │   │   ├── StockReservationConfiguration.cs
│   │   │   │   │   └── UserFavoriteConfiguration.cs
│   │   │   │   │
│   │   │   │   └── Repositories/
│   │   │   │       ├── ProductRepository.cs
│   │   │   │       ├── CategoryRepository.cs
│   │   │   │       ├── BrandRepository.cs
│   │   │   │       ├── ProductReviewRepository.cs
│   │   │   │       └── StockReservationRepository.cs
│   │   │   │
│   │   │   ├── Services/
│   │   │   │   ├── SlugGenerator.cs
│   │   │   │   ├── ImageStorageService.cs
│   │   │   │   └── PriceCalculator.cs
│   │   │   │
│   │   │   └── Extensions/
│   │   │       └── ServiceCollectionExtensions.cs
│   │   │
│   │   └── Bcommerce.Modules.Catalog.Api/
│   │       ├── Controllers/
│   │       │   ├── ProductsController.cs
│   │       │   ├── CategoriesController.cs
│   │       │   ├── BrandsController.cs
│   │       │   ├── ReviewsController.cs
│   │       │   ├── FavoritesController.cs
│   │       │   └── StockController.cs
│   │       │
│   │       └── Extensions/
│   │           └── ModuleExtensions.cs
│   │
│   ├── Cart/
│   │   ├── Bcommerce.Modules.Cart.Domain/
│   │   │   ├── Entities/
│   │   │   │   ├── ShoppingCart.cs
│   │   │   │   ├── CartItem.cs
│   │   │   │   ├── SavedCart.cs
│   │   │   │   └── CartActivityLog.cs
│   │   │   │
│   │   │   ├── ValueObjects/
│   │   │   │   ├── CartId.cs
│   │   │   │   ├── SessionId.cs
│   │   │   │   ├── ProductSnapshot.cs
│   │   │   │   └── CartTotals.cs
│   │   │   │
│   │   │   ├── Enums/
│   │   │   │   └── CartStatus.cs
│   │   │   │
│   │   │   ├── Events/
│   │   │   │   ├── CartCreatedEvent.cs
│   │   │   │   ├── ItemAddedToCartEvent.cs
│   │   │   │   ├── ItemUpdatedInCartEvent.cs
│   │   │   │   ├── ItemRemovedFromCartEvent.cs
│   │   │   │   ├── CartClearedEvent.cs
│   │   │   │   ├── CouponAppliedEvent.cs
│   │   │   │   ├── CouponRemovedEvent.cs
│   │   │   │   ├── CartConvertedEvent.cs
│   │   │   │   ├── CartMergedEvent.cs
│   │   │   │   └── CartAbandonedEvent.cs
│   │   │   │
│   │   │   ├── Repositories/
│   │   │   │   ├── ICartRepository.cs
│   │   │   │   └── ISavedCartRepository.cs
│   │   │   │
│   │   │   └── Services/
│   │   │       ├── ICartDomainService.cs
│   │   │       └── ICartPricingService.cs
│   │   │
│   │   ├── Bcommerce.Modules.Cart.Application/
│   │   │   ├── Commands/
│   │   │   │   ├── CreateCart/
│   │   │   │   │   ├── CreateCartCommand.cs
│   │   │   │   │   ├── CreateCartCommandHandler.cs
│   │   │   │   │   └── CreateCartCommandValidator.cs
│   │   │   │   │
│   │   │   │   ├── AddItemToCart/
│   │   │   │   │   ├── AddItemToCartCommand.cs
│   │   │   │   │   ├── AddItemToCartCommandHandler.cs
│   │   │   │   │   └── AddItemToCartCommandValidator.cs
│   │   │   │   │
│   │   │   │   ├── UpdateCartItem/
│   │   │   │   ├── RemoveCartItem/
│   │   │   │   ├── ClearCart/
│   │   │   │   ├── ApplyCoupon/
│   │   │   │   ├── RemoveCoupon/
│   │   │   │   ├── MergeCarts/
│   │   │   │   ├── SaveCart/
│   │   │   │   └── ConvertCart/
│   │   │   │
│   │   │   ├── Queries/
│   │   │   │   ├── GetCart/
│   │   │   │   │   ├── GetCartQuery.cs
│   │   │   │   │   └── GetCartQueryHandler.cs
│   │   │   │   │
│   │   │   │   ├── GetCartByUser/
│   │   │   │   ├── GetCartBySession/
│   │   │   │   ├── GetCartTotals/
│   │   │   │   ├── GetAbandonedCarts/
│   │   │   │   └── GetSavedCarts/
│   │   │   │
│   │   │   ├── DTOs/
│   │   │   │   ├── CartDto.cs
│   │   │   │   ├── CartItemDto.cs
│   │   │   │   ├── CartSummaryDto.cs
│   │   │   │   ├── CartTotalsDto.cs
│   │   │   │   └── SavedCartDto.cs
│   │   │   │
│   │   │   ├── Events/
│   │   │   │   └── Handlers/
│   │   │   │       ├── ItemAddedToCartEventHandler.cs
│   │   │   │       ├── CartConvertedEventHandler.cs
│   │   │   │       └── CouponAppliedEventHandler.cs
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
│   │   │   │   │   ├── CartItemConfiguration.cs
│   │   │   │   │   ├── SavedCartConfiguration.cs
│   │   │   │   │   └── CartActivityLogConfiguration.cs
│   │   │   │   │
│   │   │   │   └── Repositories/
│   │   │   │       ├── CartRepository.cs
│   │   │   │       └── SavedCartRepository.cs
│   │   │   │
│   │   │   └── Extensions/
│   │   │       └── ServiceCollectionExtensions.cs
│   │   │
│   │   └── Bcommerce.Modules.Cart.Api/
│   │       ├── Controllers/
│   │       │   ├── CartController.cs
│   │       │   └── SavedCartsController.cs
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
│   │   │   │   ├── BillingAddress.cs
│   │   │   │   ├── OrderTotals.cs
│   │   │   │   ├── TrackingInfo.cs
│   │   │   │   └── RefundAmount.cs
│   │   │   │
│   │   │   ├── Enums/
│   │   │   │   ├── OrderStatus.cs
│   │   │   │   ├── ShippingMethod.cs
│   │   │   │   ├── CancellationReason.cs
│   │   │   │   └── RefundStatus.cs
│   │   │   │
│   │   │   ├── Events/
│   │   │   │   ├── OrderCreatedEvent.cs
│   │   │   │   ├── OrderPlacedEvent.cs
│   │   │   │   ├── OrderStatusChangedEvent.cs
│   │   │   │   ├── OrderPaidEvent.cs
│   │   │   │   ├── OrderShippedEvent.cs
│   │   │   │   ├── OrderDeliveredEvent.cs
│   │   │   │   ├── OrderCancelledEvent.cs
│   │   │   │   ├── OrderRefundedEvent.cs
│   │   │   │   └── InvoiceGeneratedEvent.cs
│   │   │   │
│   │   │   ├── Repositories/
│   │   │   │   ├── IOrderRepository.cs
│   │   │   │   └── IInvoiceRepository.cs
│   │   │   │
│   │   │   └── Services/
│   │   │       ├── IOrderDomainService.cs
│   │   │       └── IOrderNumberGenerator.cs
│   │   │
│   │   ├── Bcommerce.Modules.Orders.Application/
│   │   │   ├── Commands/
│   │   │   │   ├── CreateOrder/
│   │   │   │   │   ├── CreateOrderCommand.cs
│   │   │   │   │   ├── CreateOrderCommandHandler.cs
│   │   │   │   │   └── CreateOrderCommandValidator.cs
│   │   │   │   │
│   │   │   │   ├── PlaceOrder/
│   │   │   │   ├── CancelOrder/
│   │   │   │   ├── UpdateOrderStatus/
│   │   │   │   ├── ShipOrder/
│   │   │   │   ├── DeliverOrder/
│   │   │   │   ├── RefundOrder/
│   │   │   │   ├── AddTrackingEvent/
│   │   │   │   └── GenerateInvoice/
│   │   │   │
│   │   │   ├── Queries/
│   │   │   │   ├── GetOrderById/
│   │   │   │   │   ├── GetOrderByIdQuery.cs
│   │   │   │   │   └── GetOrderByIdQueryHandler.cs
│   │   │   │   │
│   │   │   │   ├── GetOrderByNumber/
│   │   │   │   ├── GetUserOrders/
│   │   │   │   ├── GetOrdersByStatus/
│   │   │   │   ├── GetOrderStatusHistory/
│   │   │   │   ├── GetTrackingEvents/
│   │   │   │   ├── GetInvoice/
│   │   │   │   └── GetOrdersSummary/
│   │   │   │
│   │   │   ├── DTOs/
│   │   │   │   ├── OrderDto.cs
│   │   │   │   ├── OrderDetailDto.cs
│   │   │   │   ├── OrderListDto.cs
│   │   │   │   ├── OrderItemDto.cs
│   │   │   │   ├── OrderStatusHistoryDto.cs
│   │   │   │   ├── TrackingEventDto.cs
│   │   │   │   ├── InvoiceDto.cs
│   │   │   │   └── OrderSummaryDto.cs
│   │   │   │
│   │   │   ├── Events/
│   │   │   │   └── Handlers/
│   │   │   │       ├── OrderPlacedEventHandler.cs
│   │   │   │       ├── OrderPaidEventHandler.cs
│   │   │   │       ├── OrderShippedEventHandler.cs
│   │   │   │       └── OrderCancelledEventHandler.cs
│   │   │   │
│   │   │   ├── Services/
│   │   │   │   ├── IOrderService.cs
│   │   │   │   ├── OrderService.cs
│   │   │   │   ├── IShippingService.cs
│   │   │   │   └── ShippingService.cs
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
│   │   │   │   │   ├── OrderStatusHistoryConfiguration.cs
│   │   │   │   │   ├── TrackingEventConfiguration.cs
│   │   │   │   │   ├── InvoiceConfiguration.cs
│   │   │   │   │   └── OrderRefundConfiguration.cs
│   │   │   │   │
│   │   │   │   └── Repositories/
│   │   │   │       ├── OrderRepository.cs
│   │   │   │       └── InvoiceRepository.cs
│   │   │   │
│   │   │   ├── Services/
│   │   │   │   ├── OrderNumberGenerator.cs
│   │   │   │   ├── ShippingCalculator.cs
│   │   │   │   └── InvoiceGenerator.cs
│   │   │   │
│   │   │   └── Extensions/
│   │   │       └── ServiceCollectionExtensions.cs
│   │   │
│   │   └── Bcommerce.Modules.Orders.Api/
│   │       ├── Controllers/
│   │       │   ├── OrdersController.cs
│   │       │   ├── TrackingController.cs
│   │       │   └── InvoicesController.cs
│   │       │
│   │       └── Extensions/
│   │           └── ModuleExtensions.cs
│   │
│   ├── Payments/
│   │   ├── Bcommerce.Modules.Payments.Domain/
│   │   │   ├── Entities/
│   │   │   │   ├── Payment.cs
│   │   │   │   ├── UserPaymentMethod.cs
│   │   │   │   ├── PaymentTransaction.cs
│   │   │   │   ├── PaymentRefund.cs
│   │   │   │   ├── Chargeback.cs
│   │   │   │   └── Webhook.cs
│   │   │   │
│   │   │   ├── ValueObjects/
│   │   │   │   ├── PaymentAmount.cs
│   │   │   │   ├── PaymentMethod.cs
│   │   │   │   ├── CreditCard.cs
│   │   │   │   ├── PixInfo.cs
│   │   │   │   ├── BoletoInfo.cs
│   │   │   │   ├── InstallmentInfo.cs
│   │   │   │   └── FraudAnalysis.cs
│   │   │   │
│   │   │   ├── Enums/
│   │   │   │   ├── PaymentStatus.cs
│   │   │   │   ├── PaymentMethodType.cs
│   │   │   │   ├── CardBrand.cs
│   │   │   │   ├── TransactionType.cs
│   │   │   │   └── RefundStatus.cs
│   │   │   │
│   │   │   ├── Events/
│   │   │   │   ├── PaymentCreatedEvent.cs
│   │   │   │   ├── PaymentAuthorizedEvent.cs
│   │   │   │   ├── PaymentCapturedEvent.cs
│   │   │   │   ├── PaymentCompletedEvent.cs
│   │   │   │   ├── PaymentFailedEvent.cs
│   │   │   │   ├── PaymentCancelledEvent.cs
│   │   │   │   ├── PaymentRefundedEvent.cs
│   │   │   │   ├── ChargebackCreatedEvent.cs
│   │   │   │   └── WebhookReceivedEvent.cs
│   │   │   │
│   │   │   ├── Repositories/
│   │   │   │   ├── IPaymentRepository.cs
│   │   │   │   ├── IUserPaymentMethodRepository.cs
│   │   │   │   └── IWebhookRepository.cs
│   │   │   │
│   │   │   └── Services/
│   │   │       ├── IPaymentDomainService.cs
│   │   │       └── IPaymentGateway.cs
│   │   │
│   │   ├── Bcommerce.Modules.Payments.Application/
│   │   │   ├── Commands/
│   │   │   │   ├── CreatePayment/
│   │   │   │   │   ├── CreatePaymentCommand.cs
│   │   │   │   │   ├── CreatePaymentCommandHandler.cs
│   │   │   │   │   └── CreatePaymentCommandValidator.cs
│   │   │   │   │
│   │   │   │   ├── ProcessPayment/
│   │   │   │   ├── CapturePayment/
│   │   │   │   ├── CancelPayment/
│   │   │   │   ├── RefundPayment/
│   │   │   │   ├── SavePaymentMethod/
│   │   │   │   ├── DeletePaymentMethod/
│   │   │   │   ├── ProcessWebhook/
│   │   │   │   └── ProcessChargeback/
│   │   │   │
│   │   │   ├── Queries/
│   │   │   │   ├── GetPaymentById/
│   │   │   │   │   ├── GetPaymentByIdQuery.cs
│   │   │   │   │   └── GetPaymentByIdQueryHandler.cs
│   │   │   │   │
│   │   │   │   ├── GetPaymentByOrder/
│   │   │   │   ├── GetUserPaymentMethods/
│   │   │   │   ├── GetPaymentTransactions/
│   │   │   │   ├── GetPaymentRefunds/
│   │   │   │   └── GetChargebacks/
│   │   │   │
│   │   │   ├── DTOs/
│   │   │   │   ├── PaymentDto.cs
│   │   │   │   ├── PaymentDetailDto.cs
│   │   │   │   ├── UserPaymentMethodDto.cs
│   │   │   │   ├── PaymentTransactionDto.cs
│   │   │   │   ├── PaymentRefundDto.cs
│   │   │   │   ├── ChargebackDto.cs
│   │   │   │   ├── PixPaymentDto.cs
│   │   │   │   └── BoletoPaymentDto.cs
│   │   │   │
│   │   │   ├── Events/
│   │   │   │   └── Handlers/
│   │   │   │       ├── PaymentCompletedEventHandler.cs
│   │   │   │       ├── PaymentFailedEventHandler.cs
│   │   │   │       └── WebhookReceivedEventHandler.cs
│   │   │   │
│   │   │   ├── Services/
│   │   │   │   ├── IPaymentService.cs
│   │   │   │   ├── PaymentService.cs
│   │   │   │   ├── IPaymentGatewayService.cs
│   │   │   │   └── PaymentGatewayService.cs
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
│   │   │   │   │   ├── UserPaymentMethodConfiguration.cs
│   │   │   │   │   ├── PaymentTransactionConfiguration.cs
│   │   │   │   │   ├── PaymentRefundConfiguration.cs
│   │   │   │   │   ├── ChargebackConfiguration.cs
│   │   │   │   │   └── WebhookConfiguration.cs
│   │   │   │   │
│   │   │   │   └── Repositories/
│   │   │   │       ├── PaymentRepository.cs
│   │   │   │       ├── UserPaymentMethodRepository.cs
│   │   │   │       └── WebhookRepository.cs
│   │   │   │
│   │   │   ├── Gateways/
│   │   │   │   ├── Stripe/
│   │   │   │   │   ├── StripeGateway.cs
│   │   │   │   │   ├── StripeSettings.cs
│   │   │   │   │   └── StripeMapper.cs
│   │   │   │   │
│   │   │   │   ├── Pagarme/
│   │   │   │   │   ├── PagarmeGateway.cs
│   │   │   │   │   ├── PagarmeSettings.cs
│   │   │   │   │   └── PagarmeMapper.cs
│   │   │   │   │
│   │   │   │   └── IPaymentGatewayAdapter.cs
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
│       │   │   ├── EligibleCategory.cs
│       │   │   ├── EligibleProduct.cs
│       │   │   ├── EligibleUser.cs
│       │   │   ├── CouponUsage.cs
│       │   │   └── CouponReservation.cs
│       │   │
│       │   ├── ValueObjects/
│       │   │   ├── CouponCode.cs
│       │   │   ├── DiscountValue.cs
│       │   │   ├── UsageLimits.cs
│       │   │   └── ValidityPeriod.cs
│       │   │
│       │   ├── Enums/
│       │   │   ├── CouponType.cs
│       │   │   ├── CouponStatus.cs
│       │   │   └── CouponScope.cs
│       │   │
│       │   ├── Events/
│       │   │   ├── CouponCreatedEvent.cs
│       │   │   ├── CouponActivatedEvent.cs
│       │   │   ├── CouponUsedEvent.cs
│       │   │   ├── CouponDepletedEvent.cs
│       │   │   ├── CouponExpiredEvent.cs
│       │   │   └── CouponReservedEvent.cs
│       │   │
│       │   ├── Repositories/
│       │   │   ├── ICouponRepository.cs
│       │   │   └── ICouponReservationRepository.cs
│       │   │
│       │   └── Services/
│       │       ├── ICouponDomainService.cs
│       │       └── ICouponValidator.cs
│       │
│       ├── Bcommerce.Modules.Coupons.Application/
│       │   ├── Commands/
│       │   │   ├── CreateCoupon/
│       │   │   │   ├── CreateCouponCommand.cs
│       │   │   │   ├── CreateCouponCommandHandler.cs
│       │   │   │   └── CreateCouponCommandValidator.cs
│       │   │   │
│       │   │   ├── UpdateCoupon/
│       │   │   ├── ActivateCoupon/
│       │   │   ├── DeactivateCoupon/
│       │   │   ├── DeleteCoupon/
│       │   │   ├── ValidateCoupon/
│       │   │   ├── ReserveCoupon/
│       │   │   ├── ReleaseCoupon/
│       │   │   └── UseCoupon/
│       │   │
│       │   ├── Queries/
│       │   │   ├── GetCouponById/
│       │   │   │   ├── GetCouponByIdQuery.cs
│       │   │   │   └── GetCouponByIdQueryHandler.cs
│       │   │   │
│       │   │   ├── GetCouponByCode/
│       │   │   ├── GetCoupons/
│       │   │   ├── GetActiveCoupons/
│       │   │   ├── GetUserCoupons/
│       │   │   ├── GetCouponUsages/
│       │   │   └── GetCouponMetrics/
│       │   │
│       │   ├── DTOs/
│       │   │   ├── CouponDto.cs
│       │   │   ├── CouponDetailDto.cs
│       │   │   ├── CouponUsageDto.cs
│       │   │   ├── CouponValidationDto.cs
│       │   │   └── CouponMetricsDto.cs
│       │   │
│       │   ├── Events/
│       │   │   └── Handlers/
│       │   │       ├── CouponUsedEventHandler.cs
│       │   │       ├── CouponDepletedEventHandler.cs
│       │   │       └── CouponExpiredEventHandler.cs
│       │   │
│       │   ├── Services/
│       │   │   ├── ICouponService.cs
│       │   │   └── CouponService.cs
│       │   │
│       │   └── Mappings/
│       │       └──