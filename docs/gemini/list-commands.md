Com base no planejamento detalhado do projeto **BCommerce**, foi definido um sistema robusto de **CQRS** (Command Query Responsibility Segregation) totalizando **367 operações** distribuídas entre os módulos.

Abaixo está a lista completa de Commands e Queries planejados para implementação:

### 1. Módulo Users (Autenticação & Usuários)

* **Commands**: `RegisterUser`, `Login`, `RefreshToken`, `Logout`, `RevokeAllSessions`, `RequestPasswordReset`, `ResetPassword`, `ChangePassword`, `SendEmailVerification`, `VerifyEmail`, `EnableTwoFactorAuthentication`, `DisableTwoFactorAuthentication`, `VerifyTwoFactorCode`, `RegenerateTwoFactorBackupCodes`, `UpdateUserProfile`, `UpdateUserPreferences`, `UploadUserAvatar`, `DeleteUserAvatar`, `AddUserAddress`, `UpdateUserAddress`, `DeleteUserAddress`, `SetDefaultAddress`, `DeactivateUserAccount`, `ReactivateUserAccount`, `DeleteUserAccount`, `AssignRoleToUser`, `RemoveRoleFromUser`, `CreateRole`, `UpdateRole`, `DeleteRole`, `AssignPermissionToRole`, `RemovePermissionFromRole`.
* **Queries**: `GetUserById`, `GetUserByEmail`, `GetCurrentUser`, `GetUserSessions`, `ValidateToken`, `GetUserAddresses`, `GetUserDefaultAddress`, `GetAddressById`, `GetUserPreferences`, `GetUsers`, `GetUserRoles`, `GetUserPermissions`, `GetAllRoles`, `GetRoleById`, `GetRolePermissions`.

Como Arquiteto de Software Líder, apresento o detalhamento técnico exaustivo para o **Módulo de Users** do projeto BCommerce. Este design prioriza a integridade dos dados através de **Value Objects**, o isolamento de domínios via **Clean Architecture** e a consistência através do **Outbox Pattern**.

---

# 1. Módulo Users (Autenticação & Usuários)

## 1.1 Commands (Escrita)

### Comandos de Autenticação e Registro

**1. RegisterUserCommand** ✅

* **Validação de Entrada:** Validar formatos de `Email`, `Cpf` e força da `Password` via FluentValidation.
* **Regra de Negócio (Unicidade):** Consultar `IUserRepository` para garantir que o `Email` e `Cpf` não estão em uso.
* **Criação de Entidade:** Instanciar `ApplicationUser` utilizando os Value Objects `Email` e `Cpf`.
* **Segurança:** Utilizar `IPasswordHasher` para gerar o hash da senha antes da persistência.
* **Persistência:** Salvar o usuário e o perfil inicial (`Profile`) via `UnitOfWork`.
* **Evento de Domínio:** Disparar `UserRegisteredEvent` (Outbox) para que o Módulo **Cart** crie um carrinho vinculado.
* **Retorno:** `Result.Success(UserId)`.

**2. LoginCommand** ✅

* **Autenticação:** Validar credenciais via `IIdentityService`.
* **Criação de Sessão:** Instanciar entidade `Session` com `DeviceInfo` e `IpAddress`.
* **Consistência (Merge de Carrinho):** Disparar evento de integração para o Módulo **Cart** processar o `MergeCartCommand` (Anônimo -> Logado).
* **Tokens:** Gerar par de JWT (Access + Refresh Token) via `ITokenService`.
* **Retorno:** `Result.Success(TokenDto)`.

**3. RefreshTokenCommand** ✅

* **Validação:** Verificar se o `RefreshToken` é válido, não expirado e não revogado.
* **Rotação de Token:** Revogar o token atual e gerar um novo par (estratégia de Refresh Token Rotation).
* **Retorno:** `Result.Success(NewTokenDto)`.

**4. LogoutCommand / RevokeAllSessionsCommand** ✅

* **Ação:** Marcar a `Session` atual (ou todas) como revogada no `ISessionRepository`.
* **Segurança:** Adicionar os tokens JTI à uma *blacklist* temporária no Redis.
* **Retorno:** `Result.Success()`.

### Comandos de Recuperação e Segurança

**5. RequestPasswordResetCommand / ResetPasswordCommand** ✅

* **Geração:** Criar token de segurança via `IIdentityService`.
* **Comunicação:** Disparar `SendEmailIntegrationEvent` para o serviço de mensageria.
* **Redefinição:** Validar token e atualizar o hash da senha na entidade `ApplicationUser`.

**6. SendEmailVerificationCommand / VerifyEmailCommand**✅

* **Lógica:** Similar ao reset de senha, validando o `Email` como verificado na entidade de domínio para permitir compras.

**7. MFA (Enable/Disable/VerifyTwoFactor)**✅

* **Configuração:** Ativar flag `IsTwoFactorEnabled`. Gerar segredo TOTP.
* **Verificação:** Validar código de 6 dígitos contra o provedor de identidade.
* **Backup:** `RegenerateTwoFactorBackupCodes` gera 10 códigos únicos de uso único.

### Comandos de Perfil e Preferências

**8. UpdateUserProfileCommand**✅

* **Recuperação:** Obter a entidade `Profile` do usuário.
* **Atualização:** Modificar `DisplayName`, `PhoneNumber` (Value Object) e `DateOfBirth`.
* **Retorno:** `Result.Success()`.

**9. UploadUserAvatarCommand / DeleteUserAvatarCommand**✅

* **Storage:** Enviar stream de imagem para o `IStorageService` (S3/Azure Blob).
* **Referência:** Atualizar a URL do avatar na entidade `Profile`.

**10. UpdateUserPreferencesCommand**✅

* **Dados:** Atualizar preferências de marketing, idioma e tema na entidade `NotificationPreference`.

### Comandos de Endereços

**11. AddUserAddressCommand**✅

* **Validação:** Validar `PostalCode` (CEP) via serviço externo (ViaCEP/APILayer).
* **Criação:** Instanciar `Address` vinculada ao usuário.
* **Regra de Negócio (Padrão):** Se `IsDefault` for true, desmarcar todos os outros endereços do usuário como padrão.
* **Persistência:** `IAddressRepository.AddAsync`.

**12. UpdateUserAddressCommand / DeleteUserAddressCommand**✅

* **Verificação:** Validar se o endereço pertence ao `CurrentUserId`.
* **Atualização:** Aplicar mudanças ou marcar como removido (`Soft Delete`).

**13. SetDefaultAddressCommand**✅

* **Lógica:** Operação atômica que redefine qual ID de endereço é o principal para entregas e cobrança.

### Comandos de Administração (Backoffice)

**14. DeactivateUserAccountCommand / ReactivateUserAccountCommand / DeleteUserAccountCommand**✅

* **Estado:** Alterar status do usuário para `Inactive` ou aplicar `Soft Delete` (ISoftDeletable).
* **Evento:** Disparar `UserAccountDeactivatedEvent` para cancelar pedidos pendentes (Módulo **Orders**).

**15. RBAC (Create/Update/Delete Role)**✅

* **Domínio:** Gerenciar entidades de `Role` e suas Claims de permissão.

**16. AssignRoleToUserCommand / RemoveRoleFromUserCommand**✅

* **Vínculo:** Associar usuários a perfis (Ex: Admin, Customer, Manager).

---

## 1.2 Queries (Leitura)

**1. GetUserByIdQuery / GetUserByEmailQuery**✅

* **Acesso:** Consulta ao `UsersDbContext` com `AsNoTracking()`.
* **Mapeamento:** Retornar `UserDetailedDto` incluindo dados de Perfil e Status.

**2. GetCurrentUserQuery**✅

* **Recuperação:** Extrair `UserId` do `ClaimsPrincipal` do contexto HTTP.
* **Cache:** Verificar no Redis (Cache-Aside) o perfil básico do usuário logado.
* **Retorno:** `UserDto`.

**3. GetUserSessionsQuery**✅

* **Consulta:** Listar todas as sessões ativas (IP, Dispositivo, Último Acesso) do usuário.

**4. ValidateTokenQuery**✅

* **Sanitização:** Verificar assinatura e expiração sem tocar no banco de dados (Validação Local de JWT).

**5. GetUserAddressesQuery / GetAddressByIdQuery**✅

* **Lógica:** Retornar lista de endereços ativos do usuário, ordenando pelo padrão (`IsDefault`) primeiro.

**6. GetUserPreferencesQuery**✅

* **Retorno:** Objeto de configuração de notificações e UI.

### Queries Administrativas (Admin & Métricas)

**7. GetUsersQuery (Listagem com Filtros)**✅

* **Paginação:** Aplicar `PagedRequest` com filtros de Nome, CPF, Email e Status.
* **Otimização:** Uso de Dapper ou Projeção EF Core para evitar carregar colunas desnecessárias.

**8. GetUserRolesQuery / GetUserPermissionsQuery / GetAllRolesQuery**✅

* **Finalidade:** Alimentar a UI de gestão de acessos do Backoffice.

**9. GetUserGrowthMetricsQuery (Relatório Admin)**✅

* **Data:** Consultar a tabela de usuários agrupando por `CreatedAt` (Mensal/Diário).
* **Cálculo:** Taxa de conversão de visitantes para usuários registrados.
* **Retorno:** `List<MetricPointDto>`.

**10. GetActiveSessionsStatsQuery**✅

* **Métrica:** Contador de usuários simultâneos (Unique sessions nos últimos 5 minutos).

---

### Notas de Implementação do Arquiteto:

1. **Imutabilidade:** O `Cpf` e `Email` são Value Objects; qualquer alteração neles resulta em uma nova instância com validação rigorosa no construtor.
2. **Segurança Pragmática:** A Query `ValidateTokenQuery` é usada internamente pelos middlewares de autenticação do Monólito.
3. **Performance:** Relatórios administrativos (Queries 9 e 10) devem preferencialmente ler de uma réplica de leitura (Read Replica) do PostgreSQL para não onerar as transações de escrita do sistema.

### 2. Módulo Catalog (Catálogo de Produtos)

* **Commands**: `CreateProduct`, `UpdateProduct`, `UpdateProductSku`, `UpdateProductPrice`, `DeleteProduct`, `RestoreProduct`, `PublishProduct`, `UnpublishProduct`, `ArchiveProduct`, `AddProductImage`, `UpdateProductImageOrder`, `SetPrimaryProductImage`, `DeleteProductImage`, `CreateProductVariant`, `UpdateProductVariant`, `DeleteProductVariant`, `BulkUpdateProductPrices`, `BulkUpdateProductStock`, `CreateCategory`, `UpdateCategory`, `DeleteCategory`, `ReorderCategories`, `UploadCategoryImage`, `CreateBrand`, `UpdateBrand`, `DeleteBrand`, `UploadBrandLogo`.
* **Queries**: `GetProductById`, `GetProductBySku`, `GetProductBySlug`, `SearchProducts`, `GetFeaturedProducts`, `GetNewArrivals`, `GetBestSellers`, `GetRelatedProducts`, `GetProductsByIds`, `GetProductImages`, `GetProductVariants`, `CheckProductAvailability`, `GetProductStock`, `GetProductStatistics`, `GetLowStockProducts`, `GetOutOfStockProducts`, `GetCategoryById`, `GetCategoryBySlug`, `GetAllCategories`, `GetCategoryTree`, `GetCategoryChildren`, `GetCategoryBreadcrumb`, `GetCategoryProductCount`, `GetBrandById`, `GetBrandBySlug`, `GetAllBrands`, `GetBrandProductCount`, `GetPopularBrands`.


Como Arquiteto de Software Líder, apresento o detalhamento técnico exaustivo para o **Módulo Catalog** do BCommerce. Este módulo é o coração da oferta comercial e exige alta consistência na escrita e performance extrema na leitura.

---

# 2. Módulo Catalog (Catálogo de Produtos)

## 2.1 Commands (Escrita)

### Gestão de Produtos e Atributos Básicos

**1. CreateProductCommand**

* **Validação de Entrada**: Validar obrigatoriedade de Nome, `Sku` e `Slug` via FluentValidation.
* **Verificação de Unicidade**: Consultar `IProductRepository` para garantir que o SKU e Slug não existem.
* **Criação de Entidade**: Instanciar `Product` com status `Draft`. Utilizar Value Objects: `Sku`, `Slug` e `Money` (para preço base).
* **Associações**: Validar existência de `CategoryId` e `BrandId` antes de associar.
* **Persistência**: Salvar via `IProductRepository` e disparar o `UnitOfWork`.
* **Evento de Domínio**: Adicionar `ProductCreatedEvent` (Outbox) para indexação futura em motores de busca (ElasticSearch).
* **Retorno**: `Result.Success(ProductId)`.

**2. UpdateProductCommand**

* **Recuperação**: Obter a entidade `Product` via repositório.
* **Atualização**: Aplicar mudanças em Nome, Descrição e Dimensões (usando VO `ProductDimensions`).
* **Regra de Negócio**: Se o produto estiver publicado, disparar evento de atualização para limpar caches de vitrine.
* **Persistência**: `UnitOfWork.SaveChangesAsync()`.

**3. UpdateProductSkuCommand / UpdateProductPriceCommand**

* **Lógica Específica**: Alterar campos críticos separadamente para manter trilha de auditoria clara. O preço deve usar o VO `Money` para garantir precisão decimal e regras de moeda.

**4. DeleteProductCommand / RestoreProductCommand**

* **Estado**: Aplicar `Soft Delete` alterando a flag `IsDeleted` e `DeletedAt`. O Restore reativa o produto apenas se a Categoria vinculada estiver ativa.

**5. PublishProductCommand / UnpublishProductCommand / ArchiveProductCommand**

* **Máquina de Estados**: Validar transições de status (ex: não pode publicar sem imagem ou estoque).
* **Evento**: `ProductPublishedEvent` dispara notificações para usuários em listas de interesse.

### Gestão de Mídia e Imagens

**6. AddProductImageCommand / DeleteProductImageCommand**

* **Storage**: Processar upload via `IImageStorageService` para gerar diferentes tamanhos (Thumb, MD, LG).
* **Domínio**: Criar entidade `ProductImage`. A primeira imagem adicionada é automaticamente marcada como `IsPrimary`.

**7. UpdateProductImageOrderCommand / SetPrimaryProductImageCommand**

* **Ação**: Reordenar a coleção de imagens do produto. Garantir que apenas uma imagem tenha a flag `IsPrimary` ativa por vez.

### Gestão de Estoque em Lote (Bulk)

**8. BulkUpdateProductStockCommand**

* **Lógica**: Receber lista de pares `{Sku, Quantity}`.
* **Processamento**: Iterar e chamar o `IStockService` para cada item, registrando o `StockMovementType.Adjustment`.
* **Segurança**: Operação executada dentro de uma única transação de banco de dados.

### Gestão de Categorias e Marcas

**9. CreateCategoryCommand / UpdateCategoryCommand**

* **Hierarquia**: Validar se o `ParentCategoryId` (se houver) existe e não cria uma referência circular.
* **URL**: Gerar e validar o VO `Slug` baseado no nome da categoria.

**10. ReorderCategoriesCommand**

* **Ação**: Atualizar o campo `DisplayOrder` de uma lista de categorias irmãs para refletir no menu de navegação.

---

## 2.2 Queries (Leitura)

### Consultas de Produto

**1. GetProductByIdQuery / GetProductBySkuQuery / GetProductBySlugQuery**

* **Sanitização**: Validar formato do identificador.
* **Cache Check**: Verificar no Redis usando a chave `catalog:product:{id|sku|slug}`.
* **Acesso**: Consulta com `AsNoTracking()`. Incluir (`Include`) Imagens, Marca e Categoria.
* **Mapeamento**: Projetar para `ProductDetailedDto`.

**2. SearchProductsQuery (Vitrine)**

* **Filtros**: Aplicar filtros de Preço (Min/Max), Categoria, Marca e Rating.
* **Ordenação**: Suportar "Mais Recentes", "Menor Preço", "Mais Vendidos".
* **Paginação**: Retornar `PaginatedList<ProductDto>`.

**3. GetFeaturedProductsQuery / GetNewArrivalsQuery**

* **Otimização**: Queries cacheadas por longo período (ex: 1 hora) por serem dados de página inicial.

**4. CheckProductAvailabilityQuery**

* **Propósito**: Usado pelo checkout. Verifica `StockValue` disponível subtraindo `StockReservations` ativas.

### Consultas de Estrutura (Navegação)

**5. GetCategoryTreeQuery**

* **Lógica**: Recuperar todas as categorias ativas e montar estrutura recursiva de árvore em memória (ou via CTE no Postgres).
* **Cache**: Cache global de longa duração, invalidado apenas no `Create/UpdateCategory`.

**6. GetCategoryBreadcrumbQuery**

* **Caminho**: Traversal ascendente do ID atual até a raiz para exibir o rastro de navegação (ex: Home > Esportes > Tenis).

### Queries de Administração e Relatórios

**7. GetProductStatisticsQuery (Dashboard Admin)**

* **Agregação**: Contar produtos por status (`Published`, `Draft`, `OutOfStock`).
* **Performance**: Executar via Dapper para contagem rápida em bases grandes.

**8. GetLowStockProductsQuery**

* **Filtro**: Produtos onde `StockValue` <= `LowStockThreshold`.

**9. GetBrandProductCountQuery / GetCategoryProductCountQuery**

* **Relatório**: Agrupar produtos e retornar contagem para filtros de interface ("Filtre por Marca (15)").

---

### Decisões do Arquiteto Líder:

1. **Imutabilidade de Preço**: O comando `UpdateProductPrice` deve obrigatoriamente criar um novo snapshot para não afetar pedidos já realizados (que usam o snapshot capturado no Módulo Orders).
2. **Estratégia de Cache**: Queries de busca utilizam o padrão **Cache-Aside**. O evento `ProductUpdatedEvent` invalida as chaves relacionadas no Redis.
3. **VOs**: `Sku` e `Slug` possuem validações Regex rigorosas no construtor para evitar caracteres especiais que quebrem URLs ou integrações.
4. **Performance**: Para a query `SearchProducts`, se a base ultrapassar 50k itens, a infraestrutura deve alternar a leitura do Postgres para o índice do **ElasticSearch**.


### 3. Módulo Cart (Carrinho de Compras)

* **Commands**: `CreateCart`, `AddItemToCart`, `UpdateCartItemQuantity`, `RemoveItemFromCart`, `ClearCart`, `MergeCarts`, `ApplyCouponToCart`, `RemoveCouponFromCart`, `MoveItemToWishlist`, `SaveCartForLater`, `SendAbandonedCartReminder`.
* **Queries**: `GetCartById`, `GetCartByUserId`, `GetCartTotal`, `ValidateCart`, `GetCartItemsCount`, `GetAbandonedCarts`, `GetAbandonedCartStatistics`.

Como Arquiteto de Software Líder, apresento o detalhamento técnico exaustivo para o **Módulo 3: Cart (Carrinho de Compras)**.

Este módulo é crítico para a conversão de vendas. Ele utiliza uma estratégia de **Snapshots de Produto** para garantir que o preço e os dados do item permaneçam consistentes durante a sessão de compra, além de implementar a lógica complexa de transição de usuários anônimos para logados (**Merge de Carrinhos**).

---

# 3. Módulo Cart (Carrinho de Compras)

## 3.1 Commands (Escrita)

**1. CreateCartCommand**

* **Validação de Entrada**: Verificar se o `UserId` (opcional) ou `SessionId` (para anônimos) foi fornecido.
* **Instanciação**: Criar a entidade `ShoppingCart` utilizando o Value Object `CartId`.
* **Regra de Domínio**: Definir o status inicial como `Active` e a moeda padrão (usando VO `Currency`).
* **Persistência**: Salvar no `CartDbContext` via `ICartRepository`.
* **Retorno**: `Result.Success(CartId)`.

**2. AddItemToCartCommand**

* **Validação**: Validar `ProductId` e garantir que `Quantity` é maior que zero.
* **Recuperação**: Obter o carrinho atual. Se não existir, executar o `CreateCartCommand`.
* **Regra de Negócio (Snapshot)**: Consultar o Módulo **Catalog** para obter um `ProductSnapshot` (Preço atual via VO `Money`, Nome e Imagem). Isso evita que alterações no catálogo quebrem o carrinho em tempo real de forma inesperada.
* **Verificação de Estoque**: Consultar o Módulo **Catalog** para validar se a quantidade solicitada está disponível.
* **Adição**: Chamar `cart.AddItem(productId, quantity, snapshot)`. Se o item já existir, somar a quantidade.
* **Persistência**: `IUnitOfWork.SaveChangesAsync()`.
* **Retorno**: `Result.Success()`.

**3. UpdateCartItemQuantityCommand**

* **Validação**: Verificar se o `CartItemId` pertence ao carrinho do usuário.
* **Regra de Domínio**: Atualizar a quantidade no objeto `CartItem`.
* **Recuperação de Totais**: Recalcular o valor total do carrinho (Subtotal - Descontos).
* **Persistência**: Atualizar entidade e disparar `UnitOfWork`.
* **Retorno**: `Result.Success()`.

**4. RemoveItemFromCartCommand**

* **Ação**: Remover o item da coleção interna do carrinho.
* **Regra de Negócio**: Se o carrinho ficar vazio, manter a entidade mas zerar os totais.
* **Retorno**: `Result.Success()`.

**5. ClearCartCommand**

* **Ação**: Remover todos os itens e cupons vinculados.
* **Estado**: Marcar o carrinho como `Empty` ou remover a entidade (dependendo da política de retenção).
* **Retorno**: `Result.Success()`.

**6. MergeCartsCommand (Anônimo -> Logado)**

* **Recuperação**: Obter o `AnonymousCart` (via `SessionId`) e o `UserCart` (via `UserId`).
* **Lógica de Mesclagem**:
1. Para cada item no `AnonymousCart`:
2. Verificar se o produto já existe no `UserCart`.
3. Se sim: Atualizar a quantidade no `UserCart` (respeitando limites de estoque).
4. Se não: Adicionar o novo item ao `UserCart` mantendo o snapshot original.


* **Conflitos**: Em caso de duplicidade de cupons, priorizar o cupom de maior desconto.
* **Finalização**: Marcar o `AnonymousCart` como `Merged` (ou excluí-lo).
* **Evento de Domínio**: Disparar `CartMergedEvent` para fins de Analytics.
* **Retorno**: `Result.Success(UserCartId)`.

**7. ApplyCouponToCartCommand**

* **Integração**: Validar o código do cupom no Módulo **Coupons**.
* **Aplicação**: Se válido, associar o `CouponId` e o `DiscountAmount` (VO `Money`) ao carrinho.
* **Recalcular**: Subtrair o desconto do total geral.
* **Retorno**: `Result.Success()`.

**8. MoveItemToWishlistCommand**

* **Ação**: Remover item do carrinho.
* **Evento**: Disparar `AddedToWishlistIntegrationEvent` (Outbox) para o Módulo **Users**.
* **Retorno**: `Result.Success()`.

**9. SaveCartForLaterCommand**

* **Ação**: Mover itens selecionados da tabela `CartItems` para `SavedItems`.
* **Propósito**: Itens que o usuário quer comprar depois, sem poluir o subtotal atual.
* **Retorno**: `Result.Success()`.

**10. SendAbandonedCartReminderCommand**

* **Trigger**: Comando disparado por um Background Job.
* **Filtro**: Identificar carrinhos `Active` sem modificação há mais de 24 horas.
* **Comunicação**: Disparar `SendEmailIntegrationEvent` com a lista de itens.
* **Retorno**: `Result.Success()`.

---

## 3.2 Queries (Leitura)

**1. GetCartByIdQuery / GetCartByUserIdQuery**

* **Acesso**: Consulta ao `CartDbContext` com `AsNoTracking()`.
* **Carregamento**: Incluir `Items` e `AppliedCoupons`.
* **Mapeamento**: Converter para `CartDto`.
* **Enriquecimento**: Validar se os preços no snapshot ainda são os mesmos do Catálogo (exibir alerta "Este preço mudou" se houver divergência).
* **Retorno**: `Result.Success(CartDto)`.

**2. GetCartTotalQuery**

* **Otimização**: Retornar apenas um objeto simplificado com `Subtotal`, `ShippingEstimate`, `TotalDiscount` e `FinalTotal`.

**3. ValidateCartQuery**

* **Sanidade**: Verificar se todos os itens no carrinho ainda possuem estoque disponível no Módulo **Catalog**.
* **Uso**: Chamada obrigatória antes de transicionar para o checkout.

**4. GetCartItemsCountQuery**

* **Performance**: Consulta escalar `COUNT` no banco de dados para atualizar o ícone do carrinho no cabeçalho.

### Queries Administrativas (Admin & Analytics)

**5. GetAbandonedCartsQuery (Admin)**

* **Filtro**: Listar carrinhos com status `Active` e `UpdatedAt` > X horas.
* **Projeção**: Incluir e-mail do usuário (se logado) e valor total perdido.
* **Retorno**: `PagedList<AbandonedCartDto>`.

**6. GetAbandonedCartStatisticsQuery (Dashboard)**

* **Cálculo**:
* `TotalAbandonedValue`: Soma dos totais de carrinhos abandonados.
* `RecoveryRate`: Percentual de carrinhos que foram convertidos em pedidos após o lembrete.
* `TopAbandonedProducts`: Ranking de produtos que mais ficam "presos" no carrinho.


* **Performance**: Utilizar **Materialized View** para o ranking de produtos.
* **Retorno**: `CartMetricsDto`.

---

### Notas de Implementação do Arquiteto:

1. **Consistência de Preço**: O uso do `ProductSnapshot` dentro do carrinho protege o cliente de mudanças súbitas de preço durante o checkout, garantindo uma melhor experiência de usuário (UX).
2. **Idempotência**: O comando `AddItemToCart` deve ser idempotente baseado no `ProductId` para evitar duplicação por cliques repetidos no botão "Comprar".
3. **Segurança**: Carrinhos anônimos devem ter uma expiração configurada no banco (ex: 30 dias) para limpeza automática.


### 4. Módulo Orders (Pedidos)

* **Commands**: `CreateOrderFromCart`, `CreateManualOrder`, `UpdateOrderShippingAddress`, `UpdateOrderBillingAddress`, `AddOrderNote`, `UpdateOrderItemQuantity`, `RemoveOrderItem`, `ConfirmOrder`, `MarkOrderAsPaid`, `ProcessOrder`, `ShipOrder`, `MarkOrderAsDelivered`, `CancelOrder`, `HoldOrder`, `ResumeOrder`, `CreateOrderRefund`, `ApproveRefund`, `RejectRefund`, `ProcessRefundPayment`, `CreateReturnRequest`, `ApproveReturnRequest`, `RejectReturnRequest`, `MarkReturnAsReceived`, `CompleteReturn`, `GenerateInvoice`, `SendInvoiceToCustomer`, `MarkInvoiceAsPaid`, `VoidInvoice`, `GenerateCreditNote`.
* **Queries**: `GetOrderById`, `GetOrderByNumber`, `GetUserOrders`, `SearchOrders`, `GetOrderItems`, `GetOrderNotes`, `GetOrderStatusHistory`, `GetOrderTracking`, `GetOrderTrackingByNumber`, `GetOrderStatistics`, `GetOrdersByStatus`, `GetPendingOrders`, `GetOrdersRequiringAttention`, `GetOrderRefunds`, `GetRefundById`, `GetPendingRefunds`, `GetReturnRequests`, `GetReturnRequestById`, `GetOrderInvoice`, `GetInvoiceById`, `GetInvoiceByNumber`, `GetUnpaidInvoices`.

Como Arquiteto de Software Líder, apresento o detalhamento técnico exaustivo para o **Módulo 4: Orders (Pedidos)**. Este módulo é o orquestrador do ciclo de vida da venda e exige o uso rigoroso de **Snapshots** para garantir a imutabilidade financeira e fiscal, além de uma **State Machine** para gerenciar as transições de status.

---

# 4. Módulo Orders (Pedidos)

## 4.1 Commands (Escrita)

### Criação e Gestão de Ciclo de Vida

**1. CreateOrderFromCartCommand**

* **Validação de Entrada**: Verificar se o `CartId` e `ShippingMethodId` são válidos via FluentValidation.
* **Recuperação de Dados**: Obter o carrinho completo do módulo **Cart** (incluindo `ProductSnapshots`).
* **Regra de Negócio (Estoque)**: Validar se as reservas de estoque (Stock Reservations) no módulo **Catalog** ainda são válidas e não expiraram.
* **Criação de Entidade**: Instanciar a `Order` com um novo `OrderNumber` (VO). Criar `OrderItems` usando os snapshots de preço e nome do carrinho para garantir que alterações futuras no catálogo não afetem este pedido.
* **Cálculo de Totais**: Aplicar impostos, frete (VO `Money`) e descontos de cupons validados.
* **Persistência**: Salvar a `Order` via `IOrderRepository` e disparar o `UnitOfWork`.
* **Evento de Domínio**: Adicionar `OrderPlacedEvent` ao Outbox para o módulo **Payments** iniciar a cobrança.
* **Retorno**: `Result.Success(OrderNumber)`.

**2. ConfirmOrderCommand / ProcessOrderCommand / ShipOrderCommand**

* **Máquina de Estados**: Validar se o status atual permite a transição (ex: `Confirmed` -> `Processing`).
* **Ação Específica (Ship)**: Gerar ou vincular o `TrackingCode` (VO).
* **Persistência**: Registrar a mudança na entidade `OrderStatusHistory`.
* **Evento**: Disparar `OrderShippedEvent` para notificação ao cliente.

**3. MarkOrderAsDeliveredCommand**

* **Validação**: Confirmar recebimento via webhook da transportadora ou input manual.
* **Finalização**: Alterar status para `Delivered` e encerrar o ciclo de vida principal.

**4. CancelOrderCommand**

* **Regra de Negócio**: Verificar se o pedido já não foi enviado. Se pago, disparar automaticamente o `CreateOrderRefundCommand`.
* **Estorno de Estoque**: Adicionar `OrderCancelledEvent` ao Outbox para que o módulo **Catalog** libere os produtos.

### Ajustes e Notas

**5. UpdateOrderShippingAddressCommand / UpdateOrderBillingAddressCommand**

* **Restrição**: Permitir alteração apenas se o status for `Pending` ou `Confirmed`.
* **Imutabilidade**: Criar novo snapshot de `ShippingAddress` (VO) na `Order`.

**6. AddOrderNoteCommand**

* **Ação**: Adicionar comentário interno ou visível ao cliente na coleção de notas da `Order`.

### Logística Reversa (Devoluções e Reembolsos)

**7. CreateOrderRefundCommand / ApproveRefundCommand / ProcessRefundPaymentCommand**

* **Validação Financeira**: Garantir que o valor do reembolso (VO `Money`) não excede o valor pago.
* **Integração**: Disparar evento para o módulo **Payments** realizar o estorno no gateway.

**8. CreateReturnRequestCommand / ApproveReturnRequestCommand / CompleteReturnCommand**

* **Fluxo**: Registrar o motivo da devolução (`CancellationReason` Enum). Gerar etiqueta de logística reversa.
* **Conclusão**: Após receber o item (`MarkReturnAsReceived`), validar condição do produto e liberar o crédito/estorno.

### Faturamento (Invoicing)

**9. GenerateInvoiceCommand / SendInvoiceToCustomerCommand**

* **Ação**: Consolidar dados da `Order`, `User` (CPF/CNPJ) e snapshots de preço para gerar a `Invoice` (Fatura).
* **Comunicação**: Enviar PDF/Link via `IEmailService`.

**10. VoidInvoiceCommand / GenerateCreditNoteCommand**

* **Fiscal**: Anular faturas em caso de cancelamento total ou gerar nota de crédito para devoluções parciais.

---

## 4.2 Queries (Leitura)

### Consultas de Pedidos (Customer & Admin)

**1. GetOrderByIdQuery / GetOrderByNumberQuery**

* **Sanitização**: Validar formato do `OrderNumber`.
* **Acesso a Dados**: Consulta ao `OrdersDbContext` com `AsNoTracking()`.
* **Carregamento**: Incluir `OrderItems`, `StatusHistory` e `TrackingEvents`.
* **Mapeamento**: Projetar para `OrderDetailedDto`.

**2. GetUserOrdersQuery**

* **Filtro**: `UserId` obtido do `ICurrentUserService`.
* **Paginação**: Retornar histórico ordenado por `CreatedAt` descendente.

**3. SearchOrdersQuery (Admin)**

* **Filtros Avançados**: Data, Status, Valor Min/Max, CPF do cliente ou SKU de produto contido.
* **Performance**: Uso de índices compostos no PostgreSQL para filtros de status + data.

**4. GetOrderTrackingQuery / GetOrderTrackingByNumberQuery**

* **Integração**: Consultar o estado atual no banco e, se necessário, atualizar via API da transportadora em tempo real.

### Queries de Gestão de Fluxo

**5. GetPendingOrdersQuery / GetOrdersRequiringAttentionQuery**

* **Filtro**: Pedidos com pagamento pendente há mais de X horas ou com falha na integração logística.

**6. GetOrderRefundsQuery / GetPendingRefundsQuery**

* **Finalidade**: Dashboard financeiro para aprovação de estornos pendentes.

**7. GetReturnRequestsQuery / GetReturnRequestByIdQuery**

* **Finalidade**: Gestão de SAC e Logística Reversa.

### Queries de Faturamento

**8. GetOrderInvoiceQuery / GetUnpaidInvoicesQuery**

* **Ação**: Recuperar dados legais para exibição ou download de faturas.

### 9. Queries de Métricas e Relatórios (Admin)

* **GetOrderStatisticsQuery**:
* **Cálculo**: Total de vendas (VO `Money`), Quantidade de pedidos por status, Ticket Médio e Taxa de Cancelamento.
* **Otimização**: Ler de **Materialized Views** atualizadas a cada 1 hora.


* **GetSalesPerformanceReportQuery**:
* **Agrupamento**: Vendas por categoria de produto e por região geográfica (baseado no `PostalCode` do snapshot).


* **GetOrderProcessingTimeQuery**:
* **Métrica**: Tempo médio entre `OrderPlaced` e `OrderShipped` para monitorar eficiência logística.



---

### Notas do Arquiteto:

1. **Snapshots de Preço**: O `OrderItem` não aponta para o ID do produto para buscar o preço; ele armazena o `Value` do VO `Money` no momento da compra. Isso é inegociável para auditoria fiscal.
2. **OrderNumber**: Não utilizamos o ID sequencial (Primary Key) para o cliente. O `OrderNumber` é um VO gerado com lógica de negócio (ex: `2025-ABCD-1234`).
3. **Resiliência**: Comandos que dependem de módulos externos (como frete no `CreateOrder`) usam o padrão **Fallback** caso o serviço da transportadora esteja fora do ar, utilizando tabelas de contingência.

Deseja que eu prossiga com o **Módulo 5: Payments**, detalhando a lógica de **Idempotência** e **Webhooks de Gateway**?

### 5. Módulo Payments (Pagamentos)

* **Commands**: `CreatePayment`, `ProcessPayment`, `ConfirmPayment`, `CancelPayment`, `RefundPayment`, `CapturePayment`, `SavePaymentMethod`, `DeletePaymentMethod`, `SetDefaultPaymentMethod`, `ProcessStripeWebhook`, `ProcessMercadoPagoWebhook`, `GeneratePixPayment`, `GenerateBoletoPayment`.
* **Queries**: `GetPaymentById`, `GetPaymentByOrderId`, `GetPaymentByTransactionId`, `GetUserPaymentMethods`, `GetPaymentHistory`, `GetPendingPayments`, `GetFailedPayments`, `GetPaymentStatistics`.

Como Arquiteto de Software Líder, apresento o detalhamento técnico exaustivo para o **Módulo 5: Payments (Pagamentos)**.

Este módulo é o mais sensível do sistema, exigindo **Idempotência** rigorosa para evitar cobranças duplicadas, uso de **Value Objects** para segurança de dados (como `CardDetails` tokenizados) e uma integração assíncrona robusta via **Webhooks** para garantir que o status do pedido reflita a realidade financeira.

---

# 5. Módulo Payments (Pagamentos)

## 5.1 Commands (Escrita)

**1. CreatePaymentCommand**

* **Validação de Entrada**: Verificar `OrderId` e se o `PaymentMethodType` é suportado via FluentValidation.
* **Recuperação de Dados**: Obter o valor total do pedido no Módulo **Orders**.
* **Criação da Entidade**: Instanciar a entidade `Payment` com status `Pending`. Utilizar o Value Object `PaymentAmount` (baseado em `Money`) para garantir a precisão da moeda.
* **Persistência**: Salvar o registro inicial no `PaymentsDbContext`.
* **Retorno**: `Result.Success(PaymentId)`.

**2. ProcessPaymentCommand**

* **Idempotência**: Verificar se já existe uma transação processada para este `PaymentId` para evitar "Double Charge".
* **Estratégia de Gateway**: Selecionar o provedor (Stripe/Mercado Pago) com base no método escolhido.
* **Chamada Externa**: Enviar os dados para o Gateway. Se for Cartão, utilizar o VO `CardDetails` (apenas tokens, nunca dados sensíveis puros).
* **Criação de Transação**: Registrar `PaymentTransaction` com o `ProviderTransactionId` retornado.
* **Atualização de Estado**: Alterar status para `Processing` ou `Authorized`.
* **Evento de Domínio**: Adicionar `PaymentInitiatedEvent` ao Outbox.
* **Retorno**: `Result.Success(TransactionStatus)`.

**3. ConfirmPaymentCommand**

* **Regra de Negócio**: Validar se a transação foi autorizada com sucesso pelo gateway.
* **Transição de Estado**: Alterar status do `Payment` para `Paid`.
* **Consistência**: Disparar o `IUnitOfWork.SaveChangesAsync`.
* **Evento de Integração**: Adicionar `PaymentCompletedEvent` ao Outbox para que o Módulo **Orders** mude o status do pedido para `Paid` e o Módulo **Catalog** baixe o estoque definitivo.
* **Retorno**: `Result.Success()`.

**4. CapturePaymentCommand**

* **Contexto**: Utilizado em fluxos onde o pagamento é apenas "Autorizado" no checkout e "Capturado" após a validação manual ou faturamento.
* **Ação**: Chamar API de captura do Gateway usando o `AuthorizationId`.
* **Atualização**: Mover status para `Paid`.

**5. RefundPaymentCommand**

* **Validação**: Verificar se o valor do estorno não excede o valor original pago (VO `PaymentAmount`).
* **Gateway**: Solicitar o estorno à API do provedor original.
* **Entidade**: Criar um registro de `PaymentRefund` vinculado ao pagamento original.
* **Evento**: Disparar `PaymentRefundedEvent` para o Módulo **Orders** tratar o cancelamento fiscal/logístico.
* **Retorno**: `Result.Success(RefundId)`.

**6. ProcessStripeWebhookCommand / ProcessMercadoPagoWebhookCommand**

* **Sanitização**: Validar a assinatura da requisição (`Webhook Secret`) para garantir que veio do gateway real.
* **Idempotência**: Verificar na tabela de `InboxMessages` se este evento (`EventId`) já foi processado.
* **Lógica de Mapeamento**: Traduzir o status do gateway (ex: `succeeded`, `charge.failed`) para o `PaymentStatus` do domínio.
* **Atualização**: Executar o comando `ConfirmPayment` ou `MarkAsFailed` dependendo do payload.

**7. GeneratePixPaymentCommand / GenerateBoletoPaymentCommand**

* **Dados Específicos**: Gerar `PixData` (Copia e Cola + QR Code) ou `BoletoData` (Linha Digitável + PDF) via integração.
* **Snapshot**: Salvar os dados de expiração e chaves de pagamento na transação.
* **Retorno**: `Result.Success(PaymentInstructionsDto)`.

**8. SavePaymentMethodCommand / DeletePaymentMethodCommand**

* **Segurança**: Salvar o `Token` do cartão retornado pelo gateway na entidade `UserPaymentMethod` para compras "One-Click". **Nunca salvar o CVV ou o PAN completo**.

---

## 5.2 Queries (Leitura)

**1. GetPaymentByIdQuery / GetPaymentByTransactionIdQuery**

* **Acesso**: Consulta ao `PaymentsDbContext` com `AsNoTracking()`.
* **Detalhamento**: Incluir o histórico de transações e tentativas de cobrança.
* **Mapeamento**: Projetar para `PaymentDto`.

**2. GetUserPaymentMethodsQuery**

* **Privacidade**: Retornar apenas os 4 últimos dígitos do cartão e a bandeira (`CardBrand` Enum), ocultando dados sensíveis.

**3. GetPaymentHistoryQuery**

* **Filtro**: `UserId` e período de tempo.
* **Uso**: Exibir para o cliente na área "Meus Pagamentos".

**4. GetPendingPaymentsQuery / GetFailedPaymentsQuery**

* **Admin**: Lista de pagamentos que aguardam compensação (Pix/Boleto) ou que falharam para análise do time financeiro.

### 5. Queries de Métricas (Admin)

**5. GetPaymentStatisticsQuery**

* **Cálculo (Materialized View)**:
* `ConversionRate`: Pagamentos aprovados vs. Total de tentativas.
* `RevenueByMethod`: Total transacionado por Pix, Cartão e Boleto (VO `Money`).
* `ChargebackRate`: Percentual de pagamentos contestados.


* **Retorno**: `PaymentDashboardDto`.

---

### Notas de Implementação do Arquiteto:

1. **Segurança PCI-DSS**: O projeto segue o padrão de **Tokenização**. Os dados do cartão são enviados diretamente do Front-end para o Gateway (Stripe.js), e o nosso Command recebe apenas o `PaymentToken`.
2. **Resiliência (Retry Pattern)**: Falhas de rede na chamada ao Gateway devem disparar retentativas exponenciais (Polly) antes de marcar o pagamento como falhado.
3. **Money Value Object**: Todas as operações financeiras utilizam `decimal` com precisão de 2 casas e validação de moeda para evitar erros de arredondamento em cálculos de estorno parcial.

Deseja finalizar com o **Módulo 6: Coupons**, detalhando as regras de **Elegibilidade** e **Prevenção de Uso Indevido**?

### 6. Módulo Coupons (Cupons de Desconto)

* **Commands**: `CreateCoupon`, `UpdateCoupon`, `DeleteCoupon`, `ActivateCoupon`, `DeactivateCoupon`, `RecordCouponUsage`.
* **Queries**: `GetCouponById`, `GetCouponByCode`, `ValidateCoupon`, `GetActiveCoupons`, `GetCouponUsageHistory`, `GetUserCouponUsage`, `GetCouponStatistics`.

Como Arquiteto de Software Líder, finalizo o detalhamento técnico exaustivo com o **Módulo 6: Coupons (Cupons de Desconto)**.

Este módulo é vital para as estratégias de marketing e retenção. Ele utiliza **Value Objects** para garantir a integridade das regras de negócio (como períodos de validade e limites de desconto) e implementa um motor de validação rigoroso para evitar prejuízos financeiros por uso indevido.

---

# 6. Módulo Coupons (Cupons de Desconto)

## 6.1 Commands (Escrita)

**1. CreateCouponCommand**

* **Validação de Entrada**: Verificar formato do `Code` (alfanumérico), se a data de expiração é futura e se o valor do desconto é positivo via FluentValidation.
* **Regra de Negócio (Unicidade)**: Consultar `ICouponRepository` para garantir que o `CouponCode` (VO) é único no sistema.
* **Criação da Entidade**: Instanciar a entidade `Coupon` utilizando os Value Objects `CouponCode`, `DiscountValue` (que encapsula se é percentual ou valor fixo) e `ValidityPeriod`.
* **Configuração de Elegibilidade**: Definir propriedades de `CouponEligibility` (ex: valor mínimo de pedido, categorias elegíveis, limite de usos por CPF).
* **Persistência**: Salvar via `ICouponRepository` e disparar o `UnitOfWork`.
* **Evento de Domínio**: Adicionar `CouponCreatedEvent` ao Outbox.
* **Retorno**: `Result.Success(CouponId)`.

**2. UpdateCouponCommand**

* **Recuperação**: Obter a entidade `Coupon` via ID.
* **Regra de Negócio**: Impedir a alteração do tipo de desconto (percentual/fixo) se o cupom já possuir registros de uso, para manter a integridade do histórico financeiro.
* **Atualização**: Modificar datas do `ValidityPeriod` ou limites de uso.
* **Persistência**: `IUnitOfWork.SaveChangesAsync()`.
* **Retorno**: `Result.Success()`.

**3. DeleteCouponCommand**

* **Ação**: Aplicar `Soft Delete` na entidade `Coupon`.
* **Regra de Negócio**: Impedir a deleção se houver pedidos pendentes vinculados (usar `Deactivate` em vez de Delete nesse caso).
* **Retorno**: `Result.Success()`.

**4. ActivateCouponCommand / DeactivateCouponCommand**

* **Transição de Estado**: Alterar o `CouponStatus` (Enum).
* **Impacto**: Cupons desativados são ignorados pelo motor de cálculo do carrinho instantaneamente.
* **Retorno**: `Result.Success()`.

**5. RecordCouponUsageCommand**

* **Contexto**: Chamado pelo Módulo **Orders** após a confirmação do pagamento.
* **Ação**: Instanciar entidade `CouponUsage` vinculando `UserId`, `OrderId` e o valor economizado.
* **Incremento**: Atualizar o contador `UsedCount` na entidade principal `Coupon`.
* **Evento de Domínio**: Disparar `CouponUsedEvent` (Outbox) para alimentar dashboards de marketing.
* **Retorno**: `Result.Success()`.

---

## 6.2 Queries (Leitura)

**1. GetCouponByIdQuery / GetCouponByCodeQuery**

* **Sanitização**: Validar se o código não contém caracteres especiais.
* **Acesso**: Consulta ao `CouponsDbContext` utilizando `AsNoTracking()`.
* **Mapeamento**: Projetar para `CouponDto`.
* **Retorno**: `Result.Success(CouponDto)` ou `Error.NotFound`.

**2. ValidateCouponQuery**

* **Entrada**: Receber `Code`, `UserId` e `CartTotal`.
* **Verificações de Domínio**:
1. **Status**: Está ativo?
2. **Validade**: A data atual está dentro do `ValidityPeriod`?
3. **Limites**: O `UsedCount` é menor que o `UsageLimit` global?
4. **Elegibilidade**: O `CartTotal` atende ao valor mínimo? O usuário já atingiu o limite por CPF?


* **Cálculo**: Retornar o valor exato do desconto (VO `Money`) que será aplicado.
* **Retorno**: `Result.Success(ValidationResultDto)` com o desconto calculado ou `Result.Failure` com o motivo específico (ex: "Cupom Expirado").

**3. GetActiveCouponsQuery**

* **Filtro**: Listar cupons com status `Active` e que estejam dentro do período de validade.
* **Uso**: Alimentar vitrines de "Cupons do Dia".
* **Cache**: Cache-Aside no Redis por 5 minutos.

**4. GetCouponUsageHistoryQuery / GetUserCouponUsageQuery**

* **Consulta**: Listar registros da tabela `CouponUsage` com detalhes de data e pedido.
* **Paginação**: Suporte a `PagedRequest`.

### 6.3 Queries de Métricas e Relatórios (Admin)

**5. GetCouponStatisticsQuery**

* **Cálculo (Agregação)**:
* `TotalDiscountGiven`: Soma de todos os descontos aplicados (ROI de campanhas).
* `RedemptionRate`: Relação entre cupons emitidos e cupons efetivamente usados.
* `TopPerformingCoupons`: Ranking de cupons que geraram maior volume de vendas.


* **Performance**: Execução via Dapper consultando a View de `CouponMetrics`.
* **Retorno**: `CouponAnalyticsDto`.

---

### Encerramento do Projeto de Arquitetura

Com este detalhamento, cobrimos os **6 módulos fundamentais** do BCommerce sob a ótica de um Arquiteto Líder.

**Resumo da Robustez do Design:**

1. **Consistência**: Uso do Outbox Pattern em todos os comandos críticos de transição (Registro, Pedido, Pagamento, Cupom).
2. **Segurança Financeira**: Uso sistemático de **Snapshots** e **Value Objects** (`Money`, `Cpf`, `Email`, `DiscountValue`) para evitar erros de precisão e estado.
3. **Performance**: Separação clara entre comandos e consultas (CQRS), com estratégias de cache Redis e Projeções (DTOs) otimizadas.
4. **Modularidade**: Cada módulo possui seu próprio `DbContext` e `Schema`, permitindo que o monólito seja quebrado em microserviços futuramente com esforço mínimo.

**Próximos Passos Sugeridos:**

* Implementação dos **Pipeline Behaviors** do MediatR para Logging e Validação Automática.
* Configuração das **Background Tasks** para limpeza de cupons expirados e carrinhos abandonados.

O projeto está pronto para o desenvolvimento seguindo os mais altos padrões de engenharia de software!

### 7. Gestão e Módulos Transversais (Admin, Analytics, Shipping, etc.)

#### **7.1 Módulo de Notificações (Notifications)**

* **Commands:**
* `SendTransactionalEmail`: Envia e-mails disparados por eventos (ex: confirmação de pedido).
* `SendSmsNotification`: Envia notificações via SMS para atualizações críticas.
* `PushNotificationToUser`: Dispara alertas para dispositivos móveis.
* `QueueMarketingEmail`: Gerencia o envio em massa de campanhas.
* `MarkNotificationAsRead`: Atualiza o status de leitura no painel do usuário.
* `UpdateSubscriptionPreferences`: Gere as opções de Opt-in/Opt-out do cliente.


* **Queries:**
* `GetUserNotifications`: Lista as notificações recentes de um usuário.
* `GetNotificationHistory`: Histórico completo de comunicações enviadas.
* `GetUnreadNotificationCount`: Contador para badges de interface.



#### **7.2 Módulo de Frete e Logística (Shipping)**

* **Commands:**
* `CalculateShippingRates`: Calcula valores e prazos com base no CEP e cubagem.
* `CreateShippingLabel`: Gera a etiqueta para postagem.
* `UpdateShippingStatus`: Atualiza o progresso da entrega via integração com transportadora.
* `RequestCourierPickup`: Solicita a coleta da mercadoria.
* `HandleShippingWebhook`: Processa atualizações em tempo real de parceiros logísticos.


* **Queries:**
* `GetShippingMethods`: Lista as transportadoras disponíveis para uma região.
* `TrackPackage`: Consulta detalhada do status de um pacote.
* `GetShippingManifests`: Lista documentos de despacho para conferência.



#### **7.3 Módulo de Avaliações (Reviews)**

* **Commands:**
* `SubmitProductReview`: Registra o comentário e nota do cliente.
* `ApproveReview`: Moderação administrativa para publicação.
* `RejectReview`: Remove ou oculta avaliações impróprias.
* `ReplyToReview`: Resposta oficial da loja ao cliente.


* **Queries:**
* `GetProductReviews`: Lista avaliações de um produto específico.
* `GetPendingReviews`: Fila de moderação para o administrador.
* `GetAverageRating`: Média de estrelas calculada por produto.



#### **7.4 Módulo de Analytics e Dashboards**

* **Queries (Foco em Leitura Otimizada):**
* `GetSalesSummary`: Dashboard de receita bruta e líquida por período.
* `GetTopSellingProducts`: Ranking de produtos mais vendidos.
* `GetCustomerGrowthStats`: Relatório de novos cadastros e retenção.
* `GetConversionRate`: Métrica de visitas vs. pedidos finalizados.
* `GetInventoryValuation`: Valor total do estoque em mãos.
* `GetAbandonedCartRate`: Percentual de carrinhos que não viraram venda.



#### **7.5 Configurações e Gestão Geral (Admin/CMS)**

* **Commands:**
* `UpdateStoreSettings`: Configurações globais (Nome, Logo, Moeda).
* `ManageHomeBanner`: Adiciona ou remove banners da página principal.
* `CreateCmsPage`: Cria páginas estáticas (ex: Termos de Uso, FAQ).
* `UploadAsset`: Gerencia arquivos de mídia globais.
* `ImportProductsCsv`: Processamento em lote de novos catálogos.
* `ExportOrdersReport`: Gera arquivos Excel/CSV para contabilidade.


* **Queries:**
* `GetAuditLogs`: Consulta o rastro de alterações feitas por administradores.
* `GetSystemHealthStatus`: Status de conexão com gateways e serviços externos.
* `GetStoreConfiguration`: Recupera metadados globais da loja.