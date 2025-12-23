Como Arquiteto de Software Líder, apresento o mapeamento completo da camada de mensageria do **BCommerce**. No nosso Monólito Modular, a distinção entre eventos é vital para manter o desacoplamento:

1. **Domain Events (Eventos de Domínio):** Ocorrem dentro do limite (*boundary*) de um módulo para disparar efeitos colaterais internos (via MediatR).
2. **Integration Events (Eventos de Integração):** Ocorrem para comunicar mudanças de estado entre módulos (via Outbox Pattern e Message Broker/MassTransit).

Abaixo, a lista exaustiva dividida por módulos:

---

### 1. Módulo Users (Identidade & Perfil)

Focado em sincronizar a existência do utilizador e segurança com os outros módulos.

* **Domain Events:**
* `UserCreatedDomainEvent`: Disparado ao instanciar o utilizador.
* `UserAddressAddedDomainEvent`: Gatilho para validações de geolocalização.
* `PasswordChangedDomainEvent`: Força o logoff de todas as sessões.
* `MfaEnabledDomainEvent`: Atualiza o nível de segurança da sessão atual.


* **Integration Events (Outbox):**
* `UserRegisteredIntegrationEvent`: **Crítico.** Notifica o módulo **Cart** para criar o carrinho inicial e o módulo **Marketing** para o e-mail de boas-vindas.
* `UserDeletedIntegrationEvent`: Notifica todos os módulos para aplicarem a LGPD e removerem dados sensíveis.
* `UserAddressUpdatedIntegrationEvent`: Atualiza estimativas de frete em pedidos abertos.

Como Arquiteto de Software Líder, apresento o detalhamento técnico dos eventos do **Módulo de Users**. Este design garante que a consistência e a segurança da identidade do usuário sejam propagadas por todo o ecossistema BCommerce de forma resiliente.

---

# 1. Módulo Users: Detalhamento de Eventos

## 1.1 Domain Events (Eventos de Domínio)

### Evento: UserCreatedDomainEvent

* **Tipo:** Domain Event
* **Módulo de Origem:** Users
* **Fluxo Passo a Passo:**
* **Gatilho:** Uma nova instância da entidade `ApplicationUser` é criada no domínio (geralmente via método factory ou construtor) durante o processo de registro.
* **Ações Imediatas:** A entidade adiciona o evento à sua coleção interna `_domainEvents`. O `AuditableEntityInterceptor` registra a criação básica.
* **Reações Assíncronas (Handlers Locais):** Um handler interno inicializa as preferências de notificação padrão do usuário e vincula o `Profile` inicial.
* **Garantia de Entrega:** Capturado pelo `DomainEventInterceptor` e processado via MediatR após o `SaveChangesAsync`.



### Evento: UserAddressAddedDomainEvent

* **Tipo:** Domain Event
* **Módulo de Origem:** Users
* **Fluxo Passo a Passo:**
* **Gatilho:** O método `AddAddress` na entidade `ApplicationUser` ou `Profile` é executado com sucesso, recebendo um Value Object `PostalCode` válido.
* **Ações Imediatas:** Registro de auditoria indicando a adição de um novo ponto de entrega.
* **Reações Assíncronas (Handlers Locais):** O `GeoLocationService` (interno ao módulo) é acionado para converter o `PostalCode` em coordenadas geográficas via API externa e atualizar a entidade de forma assíncrona.
* **Garantia de Entrega:** Persistido temporariamente na coleção de eventos da entidade até o commit da transação.



### Evento: PasswordChangedDomainEvent

* **Tipo:** Domain Event
* **Módulo de Origem:** Users
* **Fluxo Passo a Passo:**
* **Gatilho:** O hash da senha é atualizado via comandos `ChangePassword` ou `ResetPassword` na entidade de usuário.
* **Ações Imediatas:** Atualização da propriedade `SecurityStamp` para invalidar cookies de autenticação existentes.
* **Reações Assíncronas (Handlers Locais):** Dispara o comando interno `RevokeAllSessionsCommand`, que limpa todos os Refresh Tokens do usuário no Redis e no PostgreSQL.
* **Garantia de Entrega:** Processado de forma atômica com a alteração da senha.



### Evento: MfaEnabledDomainEvent

* **Tipo:** Domain Event
* **Módulo de Origem:** Users
* **Fluxo Passo a Passo:**
* **Gatilho:** O usuário confirma a validação do primeiro código TOTP via `EnableTwoFactorCommand`.
* **Ações Imediatas:** A flag `IsTwoFactorEnabled` é setada como `true` e os códigos de backup são gerados.
* **Reações Assíncronas (Handlers Locais):** Registro em log de segurança de alta prioridade e envio de e-mail transacional local confirmando a ativação da camada de segurança.
* **Garantia de Entrega:** Capturado via Interceptor de Domínio.



---

## 1.2 Integration Events (Eventos de Integração)

### Evento: UserRegisteredIntegrationEvent

* **Tipo:** Integration Event
* **Módulo de Origem:** Users
* **Módulos Envolvidos:** Cart, Notifications (Marketing)
* **Fluxo Passo a Passo:**
* **Gatilho:** O comando `RegisterUserCommand` é finalizado e a transação de banco de dados é commitada com sucesso.
* **Persistência:** O evento é serializado e inserido na tabela `users.outbox_messages` como parte da mesma transação atômica do cadastro do usuário.
* **Consumo (Cart):** O módulo **Cart** reage criando automaticamente uma entidade `ShoppingCart` vazia vinculada ao novo `UserId`.
* **Consumo (Notifications):** O serviço de mensageria dispara o e-mail de "Boas-vindas" contendo o link de verificação de conta.
* **Garantia de Entrega:** Utiliza **Outbox Pattern** com o `OutboxProcessorService` garantindo o disparo para o Message Broker (RabbitMQ/MassTransit).



### Evento: UserDeletedIntegrationEvent

* **Tipo:** Integration Event
* **Módulo de Origem:** Users
* **Módulos Envolvidos:** Catalog, Cart, Orders, Payments, Coupons
* **Fluxo Passo a Passo:**
* **Gatilho:** Um administrador ou o próprio usuário solicita a exclusão da conta via `DeleteUserAccountCommand` (aplicando Soft Delete).
* **Persistência:** Inserção na tabela de Outbox para garantir conformidade com a LGPD em todos os módulos.
* **Consumo (Cart/Orders):** Os módulos limpam carrinhos ativos e anonimizam dados sensíveis em pedidos finalizados (mantendo apenas dados fiscais necessários).
* **Consumo (Payments):** Solicita ao gateway a remoção de métodos de pagamento salvos (Tokens de cartão).
* **Garantia de Entrega:** Outbox Pattern para garantir que a deleção seja propagada mesmo em caso de falha parcial do sistema.



### Evento: UserAddressUpdatedIntegrationEvent

* **Tipo:** Integration Event
* **Módulo de Origem:** Users
* **Módulos Envolvidos:** Orders
* **Fluxo Passo a Passo:**
* **Gatilho:** O usuário altera um endereço existente que está marcado como `IsDefault` ou vinculado a um checkout em andamento.
* **Persistência:** O `OutboxProcessor` captura a mudança do `PostalCode` e publica o evento.
* **Consumo (Orders):** O módulo **Orders** verifica se existem pedidos com status `PENDING`. Caso existam, ele invalida a estimativa de frete anterior e solicita um novo cálculo baseado no novo endereço.
* **Ação Adicional:** Notifica o usuário se a mudança de endereço alterar o prazo de entrega de um pedido já realizado (mas não enviado).
* **Garantia de Entrega:** Outbox Pattern via `shared.domain_events` (ou tabela específica do módulo).



---

### Observação de Arquitetura:

Todos os **Integration Events** detalhados utilizam o padrão **At-Least-Once Delivery**. Isso significa que os módulos consumidores (**Cart**, **Orders**, etc.) devem implementar a interface `IIntegrationEventHandler` utilizando o `InboxFilter` para garantir que o processamento seja **idempotente**, evitando duplicidade caso o Message Broker entregue o mesmo evento mais de uma vez.

---

### 2. Módulo Catalog (Catálogo & Stock)

Responsável por notificar mudanças de preços e disponibilidade de produtos.

* **Domain Events:**
* `ProductPriceChangedDomainEvent`: Atualiza o histórico de preços interno.
* `ProductStatusChangedDomainEvent`: Valida se o produto pode ser exibido.
* `StockMovementRecordedDomainEvent`: Regista entradas e saídas de armazém.


* **Integration Events (Outbox):**
* `ProductPublishedIntegrationEvent`: Gatilho para indexação no **ElasticSearch** (Módulo de Busca).
* `ProductPriceUpdatedIntegrationEvent`: Notifica o módulo **Cart** para atualizar snapshots de preços.
* `StockReservedIntegrationEvent`: Resposta positiva ao pedido de reserva do Checkout.
* `StockInsufficientIntegrationEvent`: Resposta negativa à tentativa de compra por falta de stock.
* `LowStockAlertIntegrationEvent`: Notifica o sistema de compras/admin.

Como Arquiteto de Software Líder, apresento o detalhamento técnico dos eventos do **Módulo Catalog**. Este design assegura que as mudanças no inventário e no portfólio de produtos sejam refletidas com alta performance e consistência eventual em todo o ecossistema BCommerce.

---

# 2. Módulo Catalog: Detalhamento de Eventos

## 2.1 Domain Events (Eventos de Domínio)

### Evento: ProductPriceChangedDomainEvent

* **Tipo:** Domain Event
* **Módulo de Origem:** Catalog
* **Fluxo Passo a Passo:**
* **Gatilho:** O método `UpdatePrice` na entidade `Product` é chamado, recebendo um novo Value Object `Money`.
* **Ações Imediatas:** A entidade registra o evento em sua coleção interna `_domainEvents`. O `AuditableEntityInterceptor` captura a alteração para logs básicos.
* **Reações Assíncronas:** Um handler local (`UpdatePriceHistoryHandler`) insere um registro na tabela de histórico de preços do módulo Catalog para fins de auditoria interna e geração de gráficos de variação de preço no Admin.
* **Garantia de Entrega:** Capturado pelo `DomainEventInterceptor` e disparado via MediatR após o sucesso da transação no banco.



### Evento: ProductStatusChangedDomainEvent

* **Tipo:** Domain Event
* **Módulo de Origem:** Catalog
* **Fluxo Passo a Passo:**
* **Gatilho:** O status de um produto é alterado (ex: de `Draft` para `Active` ou `Archived`) através do método `ChangeStatus` na entidade `Product`.
* **Ações Imediatas:** Validação de integridade (ex: não permite status `Active` se o produto não tiver descrição ou preço).
* **Reações Assíncronas:** Se o status mudar para `Archived`, um handler local cancela todas as reservas de estoque pendentes para este produto específico.
* **Garantia de Entrega:** Processado pelo MediatR dentro do escopo da transação atual.



### Evento: StockMovementRecordedDomainEvent

* **Tipo:** Domain Event
* **Módulo de Origem:** Catalog
* **Fluxo Passo a Passo:**
* **Gatilho:** Uma nova entrada ou saída de estoque é processada pelo `IStockService`, gerando um registro de `StockMovement`.
* **Ações Imediatas:** Incremento ou decremento atômico da coluna `CurrentQuantity` na tabela de produtos.
* **Reações Assíncronas:** O handler verifica se o novo saldo atingiu o `LowStockThreshold`. Em caso positivo, o sistema prepara o disparo do evento de integração de alerta.
* **Audit:** Registro detalhado no log de movimentação (quem moveu, quantidade, motivo e timestamp).
* **Garantia de Entrega:** Utiliza o interceptor padrão de eventos de domínio do projeto.



---

## 2.2 Integration Events (Eventos de Integração)

### Evento: ProductPublishedIntegrationEvent

* **Tipo:** Integration Event
* **Módulo de Origem:** Catalog
* **Módulos Envolvidos:** Search (ElasticSearch), Marketing
* **Fluxo Passo a Passo:**
* **Gatilho:** Um produto é movido para o status `Active` e a transação é persistida.
* **Persistência:** O evento é inserido na tabela de Outbox do módulo Catalog.
* **Consumo (Search):** O serviço de indexação recebe o evento e envia o payload completo do produto para o **ElasticSearch**, tornando-o pesquisável na vitrine.
* **Consumo (Marketing):** O módulo de notificações identifica se há clientes na "Lista de Espera" para este produto e dispara avisos de disponibilidade.
* **Garantia de Entrega:** Outbox Pattern garante que o produto só será indexado se a persistência no banco SQL ocorrer com sucesso.



### Evento: ProductPriceUpdatedIntegrationEvent

* **Tipo:** Integration Event
* **Módulo de Origem:** Catalog
* **Módulos Envolvidos:** Cart
* **Fluxo Passo a Passo:**
* **Gatilho:** A confirmação de alteração de preço (Command `UpdateProductPrice`) é concluída.
* **Persistência:** Registro na tabela de Outbox para propagação externa.
* **Consumo (Cart):** O módulo **Cart** recebe o evento e percorre todos os carrinhos ativos que possuem este `ProductId`. Ele atualiza o `ProductSnapshot` dentro do carrinho ou marca o item com uma flag `PriceChanged`, alertando o usuário no front-end.
* **Garantia de Entrega:** Outbox Pattern com retentativas via MassTransit.



### Evento: StockReservedIntegrationEvent

* **Tipo:** Integration Event
* **Módulo de Origem:** Catalog
* **Módulos Envolvidos:** Orders
* **Fluxo Passo a Passo:**
* **Gatilho:** O comando `ReserveStockCommand` é executado com sucesso para todos os itens de um pedido.
* **Persistência:** Inserção no Outbox indicando sucesso na reserva.
* **Consumo (Orders):** O módulo **Orders** reage alterando o status do pedido de `Pending` para `Confirmed`, permitindo que o fluxo siga para o faturamento/pagamento.
* **Garantia de Entrega:** Persistência atômica com a criação da reserva na tabela `catalog.stock_reservations`.



### Evento: StockInsufficientIntegrationEvent

* **Tipo:** Integration Event
* **Módulo de Origem:** Catalog
* **Módulos Envolvidos:** Orders, Notifications
* **Fluxo Passo a Passo:**
* **Gatilho:** Durante a tentativa de reserva, o sistema detecta que um ou mais itens não possuem saldo suficiente.
* **Persistência:** Registro de falha no Outbox.
* **Consumo (Orders):** O módulo **Orders** cancela o pedido automaticamente com o motivo `StockOut`.
* **Consumo (Notifications):** Dispara e-mail ao cliente informando que o pedido não pôde ser processado por falta de estoque.
* **Garantia de Entrega:** Outbox Pattern.



### Evento: LowStockAlertIntegrationEvent

* **Tipo:** Integration Event
* **Módulo de Origem:** Catalog
* **Módulos Envolvidos:** Admin (Backoffice), Notifications
* **Fluxo Passo a Passo:**
* **Gatilho:** Após um movimento de estoque, o saldo atual fica abaixo do limite de segurança configurado.
* **Persistência:** Inserção no Outbox para notificação gerencial.
* **Consumo (Admin):** O dashboard do administrador destaca o produto em uma lista de "Reposição Necessária".
* **Consumo (Notifications):** Envia um alerta via E-mail/Slack para o time de compras.
* **Garantia de Entrega:** Outbox Pattern através da tabela `catalog.outbox_messages`.



---

### Observação do Arquiteto:

Diferente dos eventos do módulo de usuários, os eventos de **Stock** no módulo Catalog utilizam um `CorrelationId` vinculado ao `OrderId`. Isso permite que, em caso de falhas em cascata, o sistema consiga rastrear exatamente qual reserva pertence a qual intenção de compra, facilitando o *troubleshooting* e a consistência eventual.

---

### 3. Módulo Cart (Carrinho de Compras)

Gere a intenção de compra e a transição de estado do cliente.

* **Domain Events:**
* `CartItemAddedDomainEvent`: Recalcula totais de forma síncrona.
* `CartClearedDomainEvent`: Limpa reservas temporárias.


* **Integration Events (Outbox):**
* `CartMergedIntegrationEvent`: Quando o carrinho anónimo é fundido com o logado (para Analytics).
* `CartConvertedIntegrationEvent`: Disparado quando o carrinho vira um pedido (inicia limpeza do carrinho).
* `CartAbandonedIntegrationEvent`: Gatilho para o Background Job de remarketing por e-mail.

Como Arquiteto de Software Líder, apresento o detalhamento técnico dos eventos do **Módulo de Cart (Carrinho de Compras)**. Este design é fundamental para garantir que a experiência de compra seja fluida e que a transição do estado de "interesse" (carrinho) para "fato" (pedido) ocorra com total integridade de dados.

---

# 3. Módulo Cart: Detalhamento de Eventos

## 3.1 Domain Events (Eventos de Domínio)

### Evento: CartItemAddedDomainEvent

* **Tipo:** Domain Event
* **Módulo de Origem:** Cart
* **Fluxo Passo a Passo:**
* **Gatilho:** O método `AddItem` na entidade `ShoppingCart` é executado com sucesso, recebendo um `ProductId` e um `ProductSnapshot`.
* **Ações Imediatas:** A entidade recalcula automaticamente o `Subtotal` e o `TotalAmount` usando o Value Object `Money` presente no snapshot. O evento é adicionado à coleção `_domainEvents`.
* **Reações Assíncronas:** Um handler local (`UpdateCartCacheHandler`) atualiza a representação do carrinho no Redis para garantir que a próxima consulta de leitura seja instantânea.
* **Garantia de Entrega:** Capturado pelo `DomainEventInterceptor` e processado via MediatR logo após o `SaveChangesAsync`.



### Evento: CartClearedDomainEvent

* **Tipo:** Domain Event
* **Módulo de Origem:** Cart
* **Fluxo Passo a Passo:**
* **Gatilho:** O método `Clear` é chamado na entidade `ShoppingCart` (geralmente após a conversão para pedido ou por ação manual do usuário).
* **Ações Imediatas:** A coleção interna de `CartItems` é esvaziada e os totais são zerados.
* **Reações Assíncronas:** Dispara um handler para remover quaisquer associações de cupons temporários que estavam travados para aquele carrinho no banco de dados.
* **Garantia de Entrega:** Processado de forma síncrona/assíncrona localmente via MediatR dentro da transação do módulo.



---

## 3.2 Integration Events (Eventos de Integração)

### Evento: CartMergedIntegrationEvent

* **Tipo:** Integration Event
* **Módulo de Origem:** Cart
* **Módulos Envolvidos:** Analytics, Users
* **Fluxo Passo a Passo:**
* **Gatilho:** O comando `MergeCartsCommand` é processado após um usuário anônimo realizar login, transferindo itens do carrinho da sessão para o carrinho do perfil logado.
* **Persistência:** O evento é serializado e inserido na tabela `cart.outbox_messages`.
* **Consumo (Analytics):** O módulo de inteligência de dados registra a conversão de um usuário anônimo para identificado, permitindo o rastreio da jornada de compra completa (Funnel Analysis).
* **Consumo (Users):** O perfil do usuário é atualizado com metadados sobre sua última atividade de compra.
* **Garantia de Entrega:** Utiliza o **Outbox Pattern** para garantir que a fusão de dados seja reportada mesmo se o broker de mensagens oscilar.



### Evento: CartConvertedIntegrationEvent

* **Tipo:** Integration Event
* **Módulo de Origem:** Cart
* **Módulos Envolvidos:** Orders, Catalog
* **Fluxo Passo a Passo:**
* **Gatilho:** O checkout é finalizado com sucesso. O comando de criação de pedido sinaliza que o carrinho não é mais a entidade "mestra" daquela transação.
* **Persistência:** Inserção na tabela de Outbox do módulo Cart.
* **Consumo (Orders):** O módulo Orders confirma que recebeu todos os dados do carrinho e pode prosseguir com o ciclo de vida do pedido.
* **Consumo (Catalog):** O módulo Catalog reage liberando quaisquer reservas de estoque temporárias que foram criadas durante a navegação, uma vez que o pedido agora possui suas próprias reservas definitivas.
* **Garantia de Entrega:** Outbox Pattern garantindo a consistência eventual entre Carrinho e Pedido.



### Evento: CartAbandonedIntegrationEvent

* **Tipo:** Integration Event
* **Módulo de Origem:** Cart
* **Módulos Envolvidos:** Users (Notifications), Marketing
* **Fluxo Passo a Passo:**
* **Gatilho:** O background job `AbandonedCartsJob` identifica um carrinho com status `Active` que não recebe atualizações há mais de 24 horas.
* **Persistência:** O job insere o evento no Outbox para processamento em massa.
* **Consumo (Notifications):** O módulo de notificações reage disparando um e-mail ou push notification para o cliente com o título "Esqueceu algo?", listando os itens e, opcionalmente, um cupom de desconto (via integração com o módulo Coupons).
* **Consumo (Marketing):** Registra o abandono para métricas de taxa de rejeição do checkout.
* **Garantia de Entrega:** Outbox Pattern através da infraestrutura de Background Jobs do Monólito.



---

### Nota do Arquiteto sobre o Módulo Cart:

A integridade financeira aqui é mantida pelo uso do **ProductSnapshot** capturado no momento do gatilho `CartItemAddedDomainEvent`. Se o preço no catálogo mudar após este evento, o sistema detecta a divergência no momento da query de leitura e pode disparar um novo evento interno para notificar o usuário, mantendo a transparência total antes da conversão para o pedido.

---

### 4. Módulo Orders (Pedidos & Logística)

O principal orquestrador do fluxo financeiro e operacional.

* **Domain Events:**
* `OrderCreatedDomainEvent`: Inicia a máquina de estados do pedido.
* `OrderItemAddedDomainEvent`: Snapshot fiscal do item.
* `OrderStatusHistoryAddedDomainEvent`: Regista o rastro de auditoria.


* **Integration Events (Outbox):**
* `OrderPlacedIntegrationEvent`: **Gatilho de Pagamento.** Notifica o módulo **Payments** para processar a cobrança.
* `OrderCancelledIntegrationEvent`: Notifica o módulo **Catalog** para libertar o stock e o módulo **Coupons** para reativar o código.
* `OrderPaidIntegrationEvent`: Notifica o armazém para iniciar o *Picking & Packing*.
* `OrderShippedIntegrationEvent`: Envia o código de rastreamento ao cliente.
* `OrderDeliveredIntegrationEvent`: Inicia o período de garantia e pedido de review (Módulo Catalog).

Como Arquiteto de Software Líder, apresento o detalhamento técnico dos eventos do **Módulo de Orders (Pedidos)**. Este módulo atua como o grande orquestrador do sucesso do cliente, garantindo que desde a intenção de compra até a entrega final, todos os sistemas estejam sincronizados e os dados protegidos por transações atômicas.

---

# 4. Módulo Orders: Detalhamento de Eventos

## 4.1 Domain Events (Eventos de Domínio)

### Evento: OrderCreatedDomainEvent

* **Tipo:** Domain Event
* **Módulo de Origem:** Orders
* **Fluxo Passo a Passo:**
* **Gatilho:** O método factory `Order.Create` é invocado durante o processamento do comando de criação de pedido.
* **Ações Imediatas:** A entidade `Order` é instanciada com status `PENDING`, um `OrderNumber` único é gerado e o evento é adicionado à coleção interna `_domainEvents`.
* **Reações Assíncronas:** Um handler local dispara a criação do primeiro registro na tabela de `OrderStatusHistory` e inicializa o objeto de `OrderTotal` (Money VO) como zero.
* **Garantia de Entrega:** Capturado pelo `DomainEventInterceptor` e processado via MediatR após a persistência no banco.



### Evento: OrderItemAddedDomainEvent

* **Tipo:** Domain Event
* **Módulo de Origem:** Orders
* **Fluxo Passo a Passo:**
* **Gatilho:** O método `AddOrderItem` é chamado na entidade `Order` para cada item vindo do carrinho.
* **Ações Imediatas:** O sistema captura o snapshot de preço e nome do produto. O `OrderTotal` é recalculado somando o valor do novo item.
* **Reações Assíncronas:** Registro de auditoria interna detalhando a quantidade e o preço unitário capturado (imutabilidade financeira).
* **Garantia de Entrega:** Processado dentro da transação atômica da criação do pedido.



### Evento: OrderStatusHistoryAddedDomainEvent

* **Tipo:** Domain Event
* **Módulo de Origem:** Orders
* **Fluxo Passo a Passo:**
* **Gatilho:** Sempre que o método `ChangeStatus` é executado na entidade `Order`, uma nova entrada é adicionada à coleção `StatusHistory`.
* **Ações Imediatas:** Persistência do log de status com timestamp e, se houver, o motivo da alteração (ex: cancelamento).
* **Reações Assíncronas:** O handler sinaliza o `MaterializedViewRefreshJob` para atualizar os indicadores de desempenho (KPIs) de vendas no Dashboard Admin.
* **Garantia de Entrega:** Utiliza a infraestrutura padrão de interceptação de eventos de domínio.



---

## 4.2 Integration Events (Eventos de Integração)

### Evento: OrderPlacedIntegrationEvent

* **Tipo:** Integration Event
* **Módulo de Origem:** Orders
* **Módulos Envolvidos:** Payments, Catalog, Users
* **Fluxo Passo a Passo:**
* **Gatilho:** O pedido é salvo com sucesso no banco de dados com status inicial `PENDING`.
* **Persistência:** O evento é inserido na tabela `orders.outbox_messages` na mesma transação.
* **Consumo (Payments):** O módulo **Payments** reage iniciando a transação financeira (gera o QR Code Pix ou solicita autorização do cartão).
* **Consumo (Catalog):** O módulo **Catalog** converte as reservas de estoque "temporárias" em "definitivas" para este pedido.
* **Consumo (Users):** O serviço de notificações envia o e-mail "Pedido Recebido" para o cliente.
* **Garantia de Entrega:** Outbox Pattern garante que o pagamento só será solicitado se o pedido existir no banco.



### Evento: OrderCancelledIntegrationEvent

* **Tipo:** Integration Event
* **Módulo de Origem:** Orders
* **Módulos Envolvidos:** Catalog, Coupons, Payments
* **Fluxo Passo a Passo:**
* **Gatilho:** O comando `CancelOrderCommand` é executado (seja por solicitação do cliente ou falha de pagamento).
* **Persistência:** Registro no Outbox do módulo Orders.
* **Consumo (Catalog):** O módulo **Catalog** libera o estoque dos produtos vinculados ao pedido, tornando-os disponíveis para outros clientes.
* **Consumo (Coupons):** Caso um cupom tenha sido usado, o módulo **Coupons** libera o limite de uso para o CPF do usuário.
* **Consumo (Payments):** Se houver um pagamento capturado, o módulo **Payments** inicia o fluxo de estorno (Refund).
* **Garantia de Entrega:** Outbox Pattern garantindo a restauração do estado do sistema.



### Evento: OrderPaidIntegrationEvent

* **Tipo:** Integration Event
* **Módulo de Origem:** Orders
* **Módulos Envolvidos:** Logistics (Picking), Notifications
* **Fluxo Passo a Passo:**
* **Gatilho:** O módulo Orders recebe o `PaymentCompletedIntegrationEvent` e altera o status do pedido para `PAID`.
* **Persistência:** Inserção no Outbox para notificar sistemas operacionais.
* **Consumo (Logística):** Notifica o sistema de armazém para gerar a lista de *Picking & Packing* (separação física dos produtos).
* **Consumo (Notifications):** Envia o e-mail de "Pagamento Confirmado" para o cliente.
* **Garantia de Entrega:** Outbox Pattern através da infraestrutura MassTransit.



### Evento: OrderShippedIntegrationEvent

* **Tipo:** Integration Event
* **Módulo de Origem:** Orders
* **Módulos Envolvidos:** Users (Notifications)
* **Fluxo Passo a Passo:**
* **Gatilho:** O administrador insere o código de rastreamento e altera o status para `SHIPPED`.
* **Ação Local:** Registro do `TrackingCode` (VO) na entidade de entrega.
* **Reação Assíncrona:** O módulo de notificações dispara um SMS/E-mail com o link de rastreio para o cliente acompanhar a entrega em tempo real.
* **Garantia de Entrega:** Outbox Pattern persistido no schema de Orders.



### Evento: OrderDeliveredIntegrationEvent

* **Tipo:** Integration Event
* **Módulo de Origem:** Orders
* **Módulos Envolvidos:** Catalog (Reviews), Marketing
* **Fluxo Passo a Passo:**
* **Gatilho:** A transportadora confirma a entrega via Webhook ou o cliente confirma o recebimento.
* **Persistência:** Registro final no ciclo de vida do pedido no Outbox.
* **Consumo (Catalog):** O módulo de Catálogo agenda uma notificação para o usuário solicitar uma "Avaliação do Produto" (Review) após 3 dias da entrega.
* **Consumo (Marketing):** O cliente é movido para o segmento de "Compradores Ativos" para futuras campanhas.
* **Garantia de Entrega:** Outbox Pattern para garantir o fechamento do funil de vendas.



---

### Nota do Arquiteto sobre Resiliência:

No módulo de **Orders**, utilizamos um identificador de correlação (`CorrelationId`) que viaja em todos esses eventos. Isso permite que, se um processo de `OrderPaid` falhar ao tentar notificar a logística, o sistema de **Retentativa (Retry Policy)** saiba exatamente qual pedido está tentando processar, mantendo a consistência mesmo sob condições adversas de rede.

---

### 5. Módulo Payments (Pagamentos)

Comunica o resultado das transações financeiras externas.

* **Domain Events:**
* `PaymentTransactionCreatedDomainEvent`: Registo inicial da tentativa.
* `PaymentMethodStoredDomainEvent`: Registo de token de cartão.


* **Integration Events (Outbox):**
* `PaymentAuthorizedIntegrationEvent`: Reserva de saldo no cartão confirmada.
* `PaymentCompletedIntegrationEvent`: **Sucesso Financeiro.** Notifica o módulo **Orders** para avançar o pedido.
* `PaymentFailedIntegrationEvent`: Notifica o módulo **Orders** para alertar o cliente e o módulo **Catalog** para libertar o stock.
* `RefundProcessedIntegrationEvent`: Confirmação de estorno concluído.

Como Arquiteto de Software Líder, apresento o detalhamento técnico dos eventos do **Módulo de Payments (Pagamentos)**. Este módulo lida com a integridade financeira do BCommerce, exigindo que cada evento seja tratado com segurança máxima, idempotência e rastreabilidade total.

---

# 5. Módulo Payments: Detalhamento de Eventos

## 5.1 Domain Events (Eventos de Domínio)

### Evento: PaymentTransactionCreatedDomainEvent

* **Tipo:** Domain Event
* **Módulo de Origem:** Payments
* **Fluxo Passo a Passo:**
* **Gatilho:** O comando `ProcessPaymentCommand` instancia uma nova entidade `PaymentTransaction` para registrar uma tentativa de cobrança (seja via cartão, Pix ou boleto).
* **Ações Imediatas:** A entidade registra o evento em sua coleção interna `_domainEvents`. O sistema captura o `TransactionType` e o identificador externo do gateway.
* **Reações Assíncronas:** Um handler local (`UpdatePaymentStatusHandler`) verifica se o status do objeto `Payment` pai precisa ser atualizado para `Processing` ou `Pending`.
* **Garantia de Entrega:** Capturado pelo `DomainEventInterceptor` e processado via MediatR após o `IUnitOfWork.SaveChangesAsync()`.



### Evento: PaymentMethodStoredDomainEvent

* **Tipo:** Domain Event
* **Módulo de Origem:** Payments
* **Fluxo Passo a Passo:**
* **Gatilho:** O usuário salva um novo cartão para compras futuras através do comando `SavePaymentMethodCommand`.
* **Ações Imediatas:** A entidade `PaymentMethod` armazena o token gerado pelo gateway (Stripe/Mercado Pago) e os metadados (bandeira, últimos 4 dígitos).
* **Reações Assíncronas:** Se marcado como padrão, um handler local desativa o status de "padrão" dos outros métodos de pagamento do usuário.
* **Garantia de Entrega:** Utiliza a infraestrutura de eventos de domínio do Monólito Modular.



---

## 5.2 Integration Events (Eventos de Integração)

### Evento: PaymentAuthorizedIntegrationEvent

* **Tipo:** Integration Event
* **Módulo de Origem:** Payments
* **Módulos Envolvidos:** Orders
* **Fluxo Passo a Passo:**
* **Gatilho:** O gateway de cartão de crédito confirma que o limite do cliente foi reservado com sucesso (Status: Authorized).
* **Persistência:** O evento é inserido na tabela `payments.outbox_messages` na mesma transação que atualiza o status da transação.
* **Consumo (Orders):** O módulo **Orders** reage movendo o status do pedido para `Processing`, sinalizando que a mercadoria já pode ser preparada, embora o dinheiro ainda não tenha sido "capturado".
* **Garantia de Entrega:** Outbox Pattern garante que a autorização seja reportada ao módulo de pedidos de forma resiliente.



### Evento: PaymentCompletedIntegrationEvent

* **Tipo:** Integration Event
* **Módulo de Origem:** Payments
* **Módulos Envolvidos:** Orders, Catalog, Users
* **Fluxo Passo a Passo:**
* **Gatilho:** **Sucesso Financeiro.** O dinheiro foi capturado no cartão, o Pix foi pago ou o boleto foi compensado.
* **Persistência:** Inserção na tabela de Outbox do módulo Payments.
* **Consumo (Orders):** O módulo **Orders** altera o status do pedido para `PAID` e inicia o faturamento.
* **Consumo (Catalog):** O módulo **Catalog** efetiva a baixa definitiva do estoque (converte reserva em venda).
* **Consumo (Users):** Dispara notificação de confirmação de pagamento para o cliente.
* **Garantia de Entrega:** Outbox Pattern via `payments.outbox_messages`.



### Evento: PaymentFailedIntegrationEvent

* **Tipo:** Integration Event
* **Módulo de Origem:** Payments
* **Módulos Envolvidos:** Orders, Catalog
* **Fluxo Passo a Passo:**
* **Gatilho:** O gateway retorna um erro definitivo (Cartão recusado, Pix expirado, Fraude detectada).
* **Persistência:** Registro de falha no Outbox para sincronização de estado.
* **Consumo (Orders):** O módulo **Orders** reage movendo o pedido para `PaymentFailed` ou `Cancelled` (dependendo da política de retentativa).
* **Consumo (Catalog):** O módulo **Catalog** libera imediatamente as reservas de estoque para que outros clientes possam comprar os produtos.
* **Garantia de Entrega:** Outbox Pattern garantindo que o estoque não fique "preso" em pagamentos que falharam.



### Evento: RefundProcessedIntegrationEvent

* **Tipo:** Integration Event
* **Módulo de Origem:** Payments
* **Módulos Envolvidos:** Orders, Users
* **Fluxo Passo a Passo:**
* **Gatilho:** Uma solicitação de estorno é confirmada pelo gateway de pagamento após a execução do `RefundPaymentCommand`.
* **Persistência:** O evento é serializado contendo o `RefundId` e o `PaymentAmount` (Money VO) estornado.
* **Consumo (Orders):** O módulo **Orders** atualiza o status do pedido para `Refunded` e gera uma Nota de Crédito se necessário.
* **Consumo (Users):** Envia um e-mail de confirmação de estorno informando o prazo para o crédito aparecer na fatura do cliente.
* **Garantia de Entrega:** Outbox Pattern para garantir a integridade do fluxo de devolução.



---

### Nota do Arquiteto sobre Segurança:

No módulo de **Payments**, todos os eventos de integração carregam uma chave de **Idempotência**. Isso garante que, se o módulo de **Orders** receber o `PaymentCompletedIntegrationEvent` duas vezes (devido a uma retentativa da infraestrutura de mensagens), ele processará o pagamento apenas uma vez, evitando faturamentos duplicados ou erros de estado na máquina de vendas.

---

### 6. Módulo Coupons (Promoções)

Gere a utilização de benefícios e limites.

* **Domain Events:**
* `CouponCreatedDomainEvent`: Validação de regras de negócio.
* `CouponUsageLimitedDomainEvent`: Alerta de proximidade do fim da campanha.


* **Integration Events (Outbox):**
* `CouponAppliedIntegrationEvent`: Notifica o **Cart/Order** sobre o valor de desconto aprovado.
* `CouponUsageConfirmedIntegrationEvent`: Confirmado após o pagamento do pedido, incrementando o contador global de uso.
* `CouponUsageReleasedIntegrationEvent`: Em caso de cancelamento de pedido, devolve o uso ao cupom.

Como Arquiteto de Software Líder, apresento o detalhamento técnico dos eventos do **Módulo de Coupons (Cupons de Desconto)**. Este design é fundamental para garantir a integridade das campanhas de marketing, evitando o uso indevido de descontos e mantendo a consistência financeira entre o carrinho, o pedido e o limite de uso dos cupons.

---

# 6. Módulo Coupons: Detalhamento de Eventos

## 6.1 Domain Events (Eventos de Domínio)

### Evento: CouponCreatedDomainEvent

* **Tipo:** Domain Event
* **Módulo de Origem:** Coupons
* **Fluxo Passo a Passo:**
* **Gatilho:** Um administrador cria um novo cupom através do `CreateCouponCommand`; a entidade `Coupon` é instanciada e salva.
* **Ações Imediatas:** A entidade registra o evento em sua coleção interna `_domainEvents`. O `AuditableEntityInterceptor` captura metadados de quem criou e quando.
* **Reações Assíncronas:** Um handler local (`CouponCacheWarmerHandler`) adiciona os detalhes básicos do cupom ao Redis para que a query `GetCouponByCode` seja ultra-rápida durante o checkout.
* **Garantia de Entrega:** Capturado pelo `DomainEventInterceptor` e processado via MediatR após o `IUnitOfWork.SaveChangesAsync()`.



### Evento: CouponUsageLimitedDomainEvent

* **Tipo:** Domain Event
* **Módulo de Origem:** Coupons
* **Fluxo Passo a Passo:**
* **Gatilho:** Durante o incremento do contador de uso, o sistema detecta que o `UsedCount` atingiu um limite crítico (ex: 90% do `UsageLimit`).
* **Ações Imediatas:** O cupom é marcado internamente com um alerta de "Esgotamento Próximo".
* **Reações Assíncronas:** Um handler de notificação interna dispara um alerta para o dashboard do time de Marketing, permitindo a renovação da campanha ou ajuste de estoque.
* **Garantia de Entrega:** Processado de forma síncrona/assíncrona localmente via MediatR dentro da transação do módulo.



---

## 6.2 Integration Events (Eventos de Integração)

### Evento: CouponAppliedIntegrationEvent

* **Tipo:** Integration Event
* **Módulo de Origem:** Coupons
* **Módulos Envolvidos:** Cart, Orders
* **Fluxo Passo a Passo:**
* **Gatilho:** O comando `ValidateAndApplyCouponCommand` valida as regras de elegibilidade e o cupom é vinculado a um `CartId` ou `OrderId`.
* **Persistência:** O evento é serializado contendo o `CouponId`, o código e o valor do desconto (VO `Money`) e inserido na tabela `coupons.outbox_messages`.
* **Consumo (Cart):** O módulo **Cart** recebe o evento e atualiza o snapshot de desconto do carrinho, recalculando o total final para exibição ao usuário.
* **Consumo (Orders):** Caso o usuário já esteja no checkout, o módulo **Orders** valida se o desconto impacta regras de frete grátis.
* **Garantia de Entrega:** Utiliza o **Outbox Pattern** para garantir que o desconto seja aplicado nos outros módulos apenas se a reserva do cupom for confirmada no banco.



### Evento: CouponUsageConfirmedIntegrationEvent

* **Tipo:** Integration Event
* **Módulo de Origem:** Coupons
* **Módulos Envolvidos:** Orders, Analytics
* **Fluxo Passo a Passo:**
* **Gatilho:** O módulo **Orders** confirma o pagamento do pedido e sinaliza ao módulo de Cupons que o uso temporário deve se tornar definitivo.
* **Persistência:** Inserção no Outbox do módulo Coupons.
* **Consumo (Analytics):** O módulo de inteligência de dados atualiza o ROI (Retorno sobre Investimento) da campanha, registrando o valor real economizado pelo cliente e a conversão gerada.
* **Ação Local:** O contador `UsedCount` é incrementado atomicamente e o status da `CouponReservation` muda para `Consumed`.
* **Garantia de Entrega:** Outbox Pattern garantindo que o limite de uso seja atualizado de forma resiliente.



### Evento: CouponUsageReleasedIntegrationEvent

* **Tipo:** Integration Event
* **Módulo de Origem:** Coupons
* **Módulos Envolvidos:** Orders
* **Fluxo Passo a Passo:**
* **Gatilho:** Um pedido que utilizava um cupom é cancelado ou o pagamento falha após o prazo de expiração do Pix/Boleto.
* **Persistência:** Registro no Outbox do módulo Coupons.
* **Reação Local:** O sistema estorna o uso: decrementa o `UsedCount` do cupom e remove a trava vinculada ao CPF do usuário, permitindo que ele use o código novamente em uma nova compra.
* **Consumo (Analytics):** Ajusta os relatórios de performance de cupom, removendo a venda cancelada das métricas de sucesso.
* **Garantia de Entrega:** Outbox Pattern através da infraestrutura MassTransit para garantir a consistência eventual.



---

### Nota do Arquiteto sobre a Camada de Cupons:

A segurança contra "Race Conditions" (vários usuários tentando usar o último cupom disponível ao mesmo tempo) é garantida pelo **OptimisticLockInterceptor** no momento do gatilho dos eventos de domínio. Além disso, a separação em **Integration Events** permite que o módulo de Cupons seja altamente disponível para consultas de validação, enquanto o processamento pesado de contagem e auditoria ocorre de forma assíncrona e resiliente.

Com este detalhamento, concluímos o mapeamento de **todos os eventos críticos** dos 6 módulos do BCommerce. O sistema agora possui um rastro completo de dados e uma comunicação entre módulos baseada em padrões de excelência.

---

### Componentes de Infraestrutura (Building Blocks)

Para que todos os eventos acima funcionem, utilizamos as seguintes estruturas compartilhadas no namespace `BuildingBlocks.Messaging`:

1. **IntegrationEvent (Classe Base):** Contém `Id`, `CreationDate` e `CorrelationId`.
2. **OutboxMessage:** Tabela em cada schema de módulo (`users.outbox_messages`, `catalog.outbox_messages`, etc.) para persistência atómica com a transação do banco.
3. **InboxMessage:** Tabela para garantir o padrão **Idempotent Consumer**, evitando processar o mesmo evento de integração duas vezes.
4. **IIntegrationEventPublisher:** Interface para publicar eventos de forma transparente, abstraindo o MassTransit.

**Nota do Arquiteto:** Todos os eventos de integração que envolvam valores monetários utilizam obrigatoriamente o Value Object `Money` (incluindo o código da moeda) para evitar erros de conversão entre módulos.