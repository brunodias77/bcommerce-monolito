# Product Requirements Document (PRD)
## E-commerce Modular Monolith Platform

---

## 1. Visão Geral do Produto

### 1.1 Resumo Executivo
Plataforma de e-commerce desenvolvida como monolito modular utilizando .NET 8+, PostgreSQL e arquitetura DDD (Domain-Driven Design). O sistema implementa separação lógica em 6 módulos de domínio principais, mantendo integridade referencial e possibilitando evolução futura para microserviços.

### 1.2 Objetivos do Produto
- Fornecer plataforma completa de e-commerce B2C
- Garantir performance, escalabilidade e manutenibilidade
- Implementar práticas modernas de arquitetura e desenvolvimento
- Possibilitar evolução gradual da arquitetura
- Suportar múltiplos métodos de pagamento brasileiros (PIX, Boleto, Cartões)

### 1.3 Público-Alvo
- **Clientes finais**: Usuários que realizam compras online
- **Administradores**: Equipe interna que gerencia produtos, pedidos e operações
- **Gerentes**: Supervisores que acessam relatórios e métricas
- **Suporte**: Equipe de atendimento ao cliente

---

## 2. Arquitetura e Stack Tecnológica

### 2.1 Arquitetura
- **Padrão**: Modular Monolith com DDD
- **Estrutura**: 6 módulos independentes com separação por schemas
- **Camadas por módulo**:
  - **Core**: Domínio (Entities, Value Objects, Events, Repositories)
  - **Application**: Casos de uso (Commands, Queries, DTOs, Services)
  - **Infrastructure**: Implementações (Persistence, External Services, Caching)
  - **Contracts**: Eventos de integração entre módulos

### 2.2 Stack Tecnológica
- **Backend**: .NET 8+ / ASP.NET Core
- **Banco de Dados**: PostgreSQL 15+
- **ORM**: Entity Framework Core 8+
- **Autenticação**: ASP.NET Core Identity
- **Mensageria**: Outbox/Inbox Pattern (eventos internos)
- **Caching**: Redis (implícito)
- **Busca**: ElasticSearch (para catálogo)
- **Validação**: FluentValidation
- **Mapeamento**: AutoMapper
- **Mediator**: MediatR (CQRS)

### 2.3 Extensões PostgreSQL
- `uuid-ossp`: Geração de UUIDs
- `citext`: Texto case-insensitive
- `pg_trgm`: Busca por similaridade textual

---

## 3. Módulos do Sistema

### 3.1 Módulo Users (Usuários)
**Responsabilidade**: Gerenciamento de usuários, autenticação, perfis e notificações

#### 3.1.1 Funcionalidades Principais
- Registro e autenticação de usuários
- Login social (OAuth providers)
- Gerenciamento de perfil pessoal
- Gerenciamento de endereços (entrega/cobrança)
- Sessões multi-dispositivo
- Sistema de notificações in-app
- Preferências de notificação
- Histórico de login
- Autenticação de dois fatores (2FA)
- Recuperação de senha

#### 3.1.2 Entidades Principais
- **User** (ASP.NET Identity)
- **Profile**: Dados estendidos do usuário
- **Address**: Endereços de entrega/cobrança
- **Session**: Sessões ativas e refresh tokens
- **Notification**: Notificações do sistema
- **NotificationPreference**: Configurações de notificação
- **LoginHistory**: Auditoria de acessos

#### 3.1.3 Regras de Negócio
- Email único por usuário
- CPF validado e único (formato: XXX.XXX.XXX-XX)
- CEP validado (formato: XXXXX-XXX)
- Apenas um endereço padrão por usuário
- Sessões expiram após período de inatividade
- Bloqueio após múltiplas tentativas de login falhas
- Termos de uso e política de privacidade devem ser aceitos

#### 3.1.4 APIs
```
POST   /api/users/register
POST   /api/users/login
POST   /api/users/logout
POST   /api/users/refresh-token
GET    /api/users/profile
PUT    /api/users/profile
GET    /api/users/addresses
POST   /api/users/addresses
PUT    /api/users/addresses/{id}
DELETE /api/users/addresses/{id}
GET    /api/users/notifications
PUT    /api/users/notifications/{id}/read
GET    /api/users/sessions
DELETE /api/users/sessions/{id}
```

---

### 3.2 Módulo Catalog (Catálogo)
**Responsabilidade**: Gerenciamento de produtos, categorias, estoque e avaliações

#### 3.2.1 Funcionalidades Principais
- CRUD de produtos
- Gerenciamento de categorias hierárquicas
- Upload e gerenciamento de imagens
- Controle de estoque (movimentações e reservas)
- Sistema de avaliações e comentários
- Lista de favoritos
- Busca e filtros avançados
- Produtos em destaque
- Gerenciamento de SKUs
- Produtos digitais vs físicos

#### 3.2.2 Entidades Principais
- **Product**: Produto completo com preços e estoque
- **Category**: Categorias hierárquicas (até 5 níveis)
- **ProductImage**: Imagens em múltiplos tamanhos
- **StockMovement**: Histórico de movimentações
- **StockReservation**: Reservas temporárias durante checkout
- **ProductReview**: Avaliações com rating 1-5
- **UserFavorite**: Produtos favoritados

#### 3.2.3 Regras de Negócio
- SKU único por produto
- Slug único e SEO-friendly
- Estoque nunca negativo
- Estoque reservado ≤ estoque disponível
- Reservas expiram após período configurável
- Apenas uma imagem principal por produto
- Avaliações apenas de compras verificadas
- Rating entre 1-5 estrelas
- Status do produto: DRAFT, ACTIVE, INACTIVE, OUT_OF_STOCK, DISCONTINUED
- Alerta de estoque baixo configurável
- Preço de comparação > preço de venda

#### 3.2.4 APIs
```
GET    /api/products
GET    /api/products/{id}
POST   /api/products
PUT    /api/products/{id}
DELETE /api/products/{id}
GET    /api/products/search?q={query}&category={id}
GET    /api/products/{id}/reviews
POST   /api/products/{id}/reviews
GET    /api/categories
GET    /api/categories/{id}/products
POST   /api/categories
PUT    /api/categories/{id}
DELETE /api/categories/{id}
POST   /api/favorites/{productId}
DELETE /api/favorites/{productId}
GET    /api/favorites
```

#### 3.2.5 Movimentações de Estoque
- **IN**: Entrada de mercadoria
- **OUT**: Saída de mercadoria
- **ADJUSTMENT**: Ajuste manual
- **RESERVE**: Reserva durante checkout
- **RELEASE**: Liberação de reserva

---

### 3.3 Módulo Cart (Carrinho)
**Responsabilidade**: Gerenciamento de carrinhos de compras

#### 3.3.1 Funcionalidades Principais
- Carrinho para usuários logados e anônimos
- Adicionar/remover/atualizar itens
- Aplicação de cupons de desconto
- Merge de carrinhos (anônimo → logado)
- Carrinhos salvos para compra futura
- Detecção de carrinhos abandonados
- Sincronização de preços
- Validação de estoque em tempo real
- Log de atividades para analytics

#### 3.3.2 Entidades Principais
- **Cart**: Carrinho principal
- **CartItem**: Itens com snapshot do produto
- **SavedCart**: Carrinhos salvos
- **CartActivityLog**: Log de ações para analytics

#### 3.3.3 Regras de Negócio
- Carrinho identificado por `user_id` (logado) ou `session_id` (anônimo)
- Apenas um carrinho ativo por usuário/sessão
- Snapshot do produto salvo no item (preço, nome, imagem)
- Detecção de mudança de preço
- Reserva de estoque durante checkout
- Carrinhos expiram após período de inatividade
- Status: ACTIVE, MERGED, CONVERTED, ABANDONED, EXPIRED
- Merge automático ao fazer login

#### 3.3.4 APIs
```
GET    /api/cart
POST   /api/cart/items
PUT    /api/cart/items/{id}
DELETE /api/cart/items/{id}
POST   /api/cart/apply-coupon
DELETE /api/cart/coupon
POST   /api/cart/merge
GET    /api/cart/saved
POST   /api/cart/save
```

#### 3.3.5 Detecção de Abandono
- Carrinho sem atualização há mais de 1 hora
- Utilizado para campanhas de remarketing
- View `v_abandoned_carts` para consultas

---

### 3.4 Módulo Orders (Pedidos)
**Responsabilidade**: Gerenciamento do ciclo de vida dos pedidos

#### 3.4.1 Funcionalidades Principais
- Criação de pedidos a partir do carrinho
- Gerenciamento de status do pedido
- Cálculo de frete
- Integração com transportadoras
- Rastreamento de entrega
- Emissão de notas fiscais
- Cancelamentos e reembolsos
- Histórico completo de status
- Estimativa de entrega

#### 3.4.2 Entidades Principais
- **Order**: Pedido completo com totais
- **OrderItem**: Itens com snapshot do produto
- **OrderStatusHistory**: Histórico de mudanças
- **TrackingEvent**: Eventos de rastreamento
- **Invoice**: Notas fiscais (PDF/XML)
- **Refund**: Reembolsos processados

#### 3.4.3 Fluxo de Status
```
PENDING → PAYMENT_PROCESSING → PAID → PREPARING → 
SHIPPED → OUT_FOR_DELIVERY → DELIVERED

Fluxos alternativos:
PENDING/PAID → CANCELLED
DELIVERED → REFUNDED
* → FAILED
```

#### 3.4.4 Regras de Negócio
- Número do pedido único (formato: YY-XXXXXX)
- Snapshot de endereços (não altera com mudanças do usuário)
- Snapshot de cupons utilizados
- Desconto ≤ subtotal
- Total = subtotal - desconto + frete + impostos
- Cancelamento apenas em status permitidos
- Reembolso apenas após pagamento capturado
- Histórico de status automático via trigger
- Métodos de envio: STANDARD, EXPRESS, SAME_DAY, PICKUP

#### 3.4.5 APIs
```
POST   /api/orders
GET    /api/orders
GET    /api/orders/{id}
PUT    /api/orders/{id}/cancel
GET    /api/orders/{id}/tracking
GET    /api/orders/{id}/invoice
POST   /api/orders/{id}/refund
GET    /api/orders/user/{userId}
```

#### 3.4.6 Motivos de Cancelamento
- CUSTOMER_REQUEST
- PAYMENT_FAILED
- OUT_OF_STOCK
- FRAUD_SUSPECTED
- SHIPPING_ISSUE
- OTHER

---

### 3.5 Módulo Payments (Pagamentos)
**Responsabilidade**: Processamento de pagamentos e transações

#### 3.5.1 Funcionalidades Principais
- Processamento de múltiplos métodos de pagamento
- Integração com gateways (Stripe, PagarMe, Mercado Pago)
- Salvamento de métodos de pagamento (tokenizados)
- Pagamento parcelado
- PIX (QR Code dinâmico)
- Boleto bancário
- Análise antifraude
- Webhooks de gateways
- Gestão de chargebacks
- Reembolsos totais e parciais

#### 3.5.2 Métodos de Pagamento
- **CREDIT_CARD**: Até 24x
- **DEBIT_CARD**: À vista
- **PIX**: À vista (expira em 30min)
- **BOLETO**: À vista (expira em 3 dias)
- **WALLET**: Carteiras digitais
- **BANK_TRANSFER**: Transferência bancária

#### 3.5.3 Entidades Principais
- **Payment**: Pagamento principal
- **PaymentMethod**: Métodos salvos (tokenizados)
- **Transaction**: Operações com gateway
- **Refund**: Reembolsos
- **Chargeback**: Contestações
- **Webhook**: Eventos de gateways

#### 3.5.4 Fluxo de Pagamento
```
Cartão de Crédito:
PENDING → PROCESSING → AUTHORIZED → CAPTURED

PIX/Boleto:
PENDING → (aguarda pagamento) → CAPTURED

Fluxos de erro:
* → FAILED
* → CANCELLED
* → EXPIRED
CAPTURED → REFUNDED/PARTIALLY_REFUNDED
CAPTURED → CHARGEBACK
```

#### 3.5.5 Regras de Negócio
- Idempotência via `idempotency_key`
- Cartões tokenizados (nunca armazenar número completo)
- Apenas últimos 4 dígitos visíveis
- PIX expira em 30 minutos
- Boleto expira em 3 dias úteis
- Análise de fraude para transações suspeitas
- Fee calculado por gateway
- Net amount = amount - fee
- Apenas um método padrão por usuário
- Reembolso ≤ valor capturado

#### 3.5.6 APIs
```
POST   /api/payments/process
POST   /api/payments/{id}/capture
POST   /api/payments/{id}/cancel
POST   /api/payments/{id}/refund
GET    /api/payments/{id}
POST   /api/payments/methods
GET    /api/payments/methods
DELETE /api/payments/methods/{id}
POST   /api/payments/webhooks/stripe
POST   /api/payments/webhooks/pagarme
POST   /api/payments/webhooks/mercadopago
```

#### 3.5.7 Bandeiras de Cartão
- VISA, MASTERCARD, AMEX, ELO
- HIPERCARD, DINERS, DISCOVER, JCB, OTHER

#### 3.5.8 Tipos de Transação
- **AUTHORIZATION**: Autorização inicial
- **CAPTURE**: Captura do valor
- **VOID**: Cancelamento
- **REFUND**: Reembolso
- **CHARGEBACK**: Contestação

---

### 3.6 Módulo Coupons (Cupons)
**Responsabilidade**: Gerenciamento de cupons e promoções

#### 3.6.1 Funcionalidades Principais
- Criação e gerenciamento de cupons
- Tipos variados de desconto
- Regras de elegibilidade
- Limite de uso global e por usuário
- Cupons agendados
- Cupons exclusivos
- Reserva durante checkout
- Validação em tempo real
- Métricas de uso

#### 3.6.2 Tipos de Cupom
- **PERCENTAGE**: Desconto percentual (ex: 10%)
- **FIXED_AMOUNT**: Valor fixo (ex: R$ 50)
- **FREE_SHIPPING**: Frete grátis
- **BUY_X_GET_Y**: Compre X, ganhe Y

#### 3.6.3 Escopos
- **ALL**: Todos os produtos
- **CATEGORIES**: Categorias específicas
- **PRODUCTS**: Produtos específicos
- **FIRST_PURCHASE**: Primeira compra
- **SPECIFIC_USERS**: Usuários selecionados

#### 3.6.4 Entidades Principais
- **Coupon**: Cupom principal
- **EligibleCategory**: Categorias válidas
- **EligibleProduct**: Produtos válidos
- **EligibleUser**: Usuários exclusivos
- **Usage**: Registro de uso
- **Reservation**: Reserva durante checkout

#### 3.6.5 Regras de Negócio
- Código único (case-insensitive, 3-50 caracteres)
- Percentual ≤ 100%
- Validação de período (data início < data fim)
- Compra mínima configurável
- Limite de usos global e por usuário
- Status automático ao atingir limite
- Reserva expira se carrinho não convertido
- Incremento de uso via trigger
- Status: DRAFT, SCHEDULED, ACTIVE, PAUSED, EXPIRED, DEPLETED
- Empilhamento configurável (is_stackable)

#### 3.6.6 APIs
```
POST   /api/coupons
GET    /api/coupons
GET    /api/coupons/{id}
PUT    /api/coupons/{id}
DELETE /api/coupons/{id}
POST   /api/coupons/validate
GET    /api/coupons/{id}/metrics
GET    /api/coupons/active
```

#### 3.6.7 Validações
Função `coupons.validate_coupon_usage()` valida:
- Existência do cupom
- Status ativo
- Período de validade
- Limite global de uso
- Limite por usuário
- Valor mínimo de compra
- Elegibilidade (produtos/categorias/usuários)

---

## 4. Funcionalidades Transversais

### 4.1 Sistema de Eventos
**Outbox Pattern** para eventos de domínio:
- Tabela `shared.domain_events`
- Processamento assíncrono
- Retry em caso de falha
- Módulos se comunicam via eventos de integração

**Inbox Pattern** para idempotência:
- Tabela `shared.processed_events`
- Previne processamento duplicado

### 4.2 Auditoria
**Audit Log unificado**:
- Tabela `shared.audit_logs`
- Rastreia todas as mudanças (old_values/new_values)
- Identifica usuário, IP e User-Agent
- Separado por módulo

### 4.3 Versionamento Otimista
- Campo `version` em entidades principais
- Trigger automático de incremento
- Previne concorrência

### 4.4 Soft Delete
- Campo `deleted_at` em entidades principais
- Dados nunca removidos fisicamente
- Índices filtrados para performance

### 4.5 Timestamps Automáticos
- `created_at`: Criação
- `updated_at`: Atualização automática via trigger
- Timestamps adicionais por contexto (paid_at, shipped_at, etc.)

### 4.6 SEO
- Slugs automáticos a partir de nomes
- Meta title e meta description
- URLs amigáveis
- Suporte a acentos e caracteres especiais

---

## 5. Views e Relatórios

### 5.1 Views Materializadas
- `catalog.mv_product_stats`: Estatísticas agregadas de produtos e avaliações

### 5.2 Views Dinâmicas
- `users.v_active_sessions`: Sessões ativas por usuário
- `users.v_unread_notifications`: Contadores de notificações
- `cart.v_active_carts`: Carrinhos com totais calculados
- `cart.v_abandoned_carts`: Detecção de abandono
- `orders.v_user_order_summary`: Resumo de pedidos por usuário
- `orders.v_orders_pending_action`: Pedidos que precisam atenção
- `payments.v_expiring_payments`: Pagamentos próximos da expiração
- `payments.v_payment_metrics`: Métricas diárias de pagamento
- `coupons.v_active_coupons`: Cupons válidos e disponíveis
- `coupons.v_coupon_metrics`: Performance de cupons

---

## 6. Performance e Escalabilidade

### 6.1 Índices Estratégicos
- Índices únicos para constraints de negócio
- Índices compostos para queries frequentes
- Índices parciais (WHERE) para dados ativos
- GIN para busca textual (pg_trgm)
- GIN para JSONB e arrays

### 6.2 Caching
- Cache de produtos frequentemente acessados
- Cache de categorias (estrutura hierárquica)
- Cache de usuário (perfil e preferências)
- Invalidação via eventos de domínio

### 6.3 Busca
- ElasticSearch para catálogo de produtos
- Sincronização via eventos
- Indexação de nome, descrição, tags e atributos

### 6.4 Background Jobs
- Expiração de carrinhos
- Liberação de reservas de estoque
- Atualização de status de cupons
- Expiração de pagamentos PIX/Boleto
- Refresh de materialized views
- Processamento de eventos do outbox

---

## 7. Segurança

### 7.1 Autenticação
- ASP.NET Core Identity
- JWT Tokens (access + refresh)
- OAuth 2.0 para login social
- 2FA (Two-Factor Authentication)
- Sessões multi-dispositivo com revogação

### 7.2 Autorização
- Role-Based Access Control (RBAC)
- Claims-based authorization
- Roles: Customer, Admin, Manager, Support

### 7.3 Dados Sensíveis
- Cartões tokenizados (PCI-DSS compliance)
- Senhas com hash (ASP.NET Identity)
- CPF validado e armazenado formatado
- Logs nunca contêm dados sensíveis

### 7.4 Fraude
- Score de fraude em pagamentos
- Análise de comportamento suspeito
- Bloqueio de tentativas de login
- Validação de endereço e geolocalização

---

## 8. Monitoramento e Observabilidade

### 8.1 Logs
- Structured logging (Serilog/NLog)
- Níveis: Debug, Information, Warning, Error, Critical
- Contexto: RequestId, UserId, Module, Action

### 8.2 Métricas
- Performance de APIs (latência, throughput)
- Taxa de conversão (carrinho → pedido)
- Taxa de aprovação de pagamentos
- Estoque baixo e rupturas
- Carrinhos abandonados

### 8.3 Health Checks
- Database connectivity
- External services (payment gateways)
- Cache availability
- Search engine status

---

## 9. Casos de Uso Principais

### 9.1 Jornada do Cliente

#### Fluxo Completo de Compra
1. **Navegação**: Usuário busca/navega produtos
2. **Adicionar ao Carrinho**: Produtos adicionados (reserva de estoque)
3. **Visualizar Carrinho**: Validação de preços e estoque
4. **Aplicar Cupom**: Validação e aplicação de desconto
5. **Fazer Login**: Merge de carrinho anônimo → logado
6. **Checkout**: Seleção de endereço e método de pagamento
7. **Processar Pagamento**: Autorização/captura
8. **Criar Pedido**: Conversão do carrinho
9. **Acompanhamento**: Rastreamento e notificações
10. **Entrega**: Confirmação e avaliação

#### Fluxo Anônimo → Logado
```
1. Usuário navega sem login (session_id)
2. Adiciona produtos ao carrinho anônimo
3. Faz login
4. Sistema mescla carrinho anônimo com carrinho do usuário
5. Carrinho anônimo marcado como MERGED
```

### 9.2 Jornada do Admin

#### Gestão de Produtos
1. Criar categoria (hierarquia)
2. Criar produto com imagens
3. Configurar preços e estoque
4. Publicar produto
5. Monitorar estoque baixo
6. Ajustar preços/promoções

#### Gestão de Pedidos
1. Visualizar pedidos pendentes
2. Confirmar pagamento
3. Preparar pedido
4. Enviar pedido (tracking)
5. Gerenciar cancelamentos/reembolsos

#### Gestão de Cupons
1. Criar campanha de cupons
2. Definir regras e elegibilidade
3. Agendar ativação
4. Monitorar uso
5. Pausar/encerrar campanha

---

## 10. Integrações Externas

### 10.1 Payment Gateways
- **Stripe**: Internacional, cartões
- **PagarMe**: Nacional, cartões + PIX + Boleto
- **Mercado Pago**: Nacional, múltiplos métodos

### 10.2 Shipping
- Correios API
- Transportadoras privadas
- Cálculo de frete por CEP

### 10.3 Notifications
- Email (SMTP/SendGrid)
- SMS (Twilio)
- Push notifications

### 10.4 Storage
- AWS S3 / Azure Blob (imagens)
- CDN para assets estáticos

### 10.5 Fiscal
- NFe (Nota Fiscal Eletrônica)
- Emissores fiscais (Focus NFe, Bling)

---

## 11. Requisitos Não-Funcionais

### 11.1 Performance
- Tempo de resposta: < 200ms (p95)
- Throughput: > 1000 req/s
- Busca de produtos: < 100ms
- Checkout completo: < 2s

### 11.2 Disponibilidade
- Uptime: 99.9% (SLA)
- Degradação graceful em falhas
- Circuit breakers para serviços externos

### 11.3 Escalabilidade
- Horizontal scaling (stateless API)
- Database read replicas
- Cache distribuído (Redis)

### 11.4 Manutenibilidade
- Clean Architecture
- SOLID principles
- Test coverage > 80%
- Documentação inline

### 11.5 Compliance
- LGPD (Lei Geral de Proteção de Dados)
- PCI-DSS (dados de cartão)
- Código de Defesa do Consumidor

---

## 12. Roadmap e Evolução

### 12.1 Fase 1 (MVP) - 3 meses
- [ ] Módulos Users, Catalog, Cart
- [ ] Autenticação básica
- [ ] CRUD de produtos
- [ ] Carrinho funcional

### 12.2 Fase 2 - 2 meses
- [ ] Módulos Orders, Payments
- [ ] Integração com 1 gateway
- [ ] PIX e Cartão de Crédito
- [ ] Fluxo completo de checkout

### 12.3 Fase 3 - 2 meses
- [ ] Módulo Coupons
- [ ] Sistema de avaliações
- [ ] Busca avançada (ElasticSearch)
- [ ] Dashboard administrativo

### 12.4 Fase 4 - Contínuo
- [ ] Otimizações de performance
- [ ] Novos métodos de pagamento
- [ ] Analytics avançado
- [ ] Recomendações de produtos (ML)
- [ ] Migração gradual para microserviços

---

## 13. Considerações Finais

### 13.1 Vantagens da Arquitetura Modular
- Separação clara de responsabilidades
- Facilita testes e manutenção
- Permite evolução gradual
- Integridade referencial garantida
- Transações ACID entre módulos

### 13.2 Possível Evolução para Microserviços
- Módulos já isolados logicamente
- Comunicação via eventos preparada
- Schemas separados facilitam split de databases
- Contratos públicos bem definidos

### 13.3 Trade-offs Considerados
- **Monolito**: Simplicidade inicial, transações ACID
- **vs Microserviços**: Maior complexidade, eventual consistency
- Escolha por monolito modular oferece melhor custo/benefício inicial

---

## 14. Glossário

- **Modular Monolith**: Monolito com separação lógica em módulos
- **DDD**: Domain-Driven Design
- **CQRS**: Command Query Responsibility Segregation
- **Outbox Pattern**: Padrão para eventos confiáveis
- **Soft Delete**: Exclusão lógica (não física)
- **Snapshot**: Cópia dos dados em um momento específico
- **Idempotency**: Mesma operação pode ser executada múltiplas vezes sem efeito colateral
- **Optimistic Locking**: Controle de concorrência via versionamento

---

**Documento elaborado em**: 2025-12-12  
**Versão**: 1.0  
**Status**: Draft para revisão