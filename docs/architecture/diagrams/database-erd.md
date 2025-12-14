# Database ERD

Entity Relationship Diagram do banco de dados BCommerce.

## Schemas

```
PostgreSQL Database: bcommerce
├── users     # Usuários e autenticação
├── catalog   # Produtos e categorias
├── cart      # Carrinho de compras
├── orders    # Pedidos
├── payments  # Pagamentos
├── coupons   # Cupons e promoções
└── shared    # Outbox, audit log
```

## ERD Principal

```mermaid
erDiagram
    %% Users Schema
    asp_net_users ||--o{ addresses : has
    asp_net_users ||--|| profiles : has
    asp_net_users ||--o{ sessions : has
    asp_net_users ||--o{ notifications : has
    asp_net_users ||--|| notification_preferences : has
    asp_net_users ||--o{ login_history : has
    
    %% Catalog Schema
    categories ||--o{ categories : parent
    categories ||--o{ products : contains
    brands ||--o{ products : owns
    products ||--o{ product_images : has
    products ||--o{ product_reviews : has
    products ||--|| product_stock : has
    products ||--o{ stock_movements : has
    
    %% Cart Schema
    asp_net_users ||--o{ carts : has
    carts ||--o{ cart_items : contains
    products ||--o{ cart_items : "added to"
    
    %% Orders Schema
    asp_net_users ||--o{ orders : places
    orders ||--o{ order_items : contains
    orders ||--o{ order_status_history : has
    orders ||--o{ order_tracking : has
    products ||--o{ order_items : "included in"
    
    %% Payments Schema
    orders ||--|| payments : has
    payments ||--o{ payment_transactions : has
    asp_net_users ||--o{ payment_methods : saves
    
    %% Coupons Schema
    coupons ||--o{ coupon_usages : tracks
    asp_net_users ||--o{ coupon_usages : uses
    orders ||--o{ coupon_usages : applies
```

## Tabelas por Schema

### users (ASP.NET Identity + Custom)

| Tabela | Descrição |
|--------|-----------|
| `asp_net_users` | Usuários (Identity) |
| `asp_net_roles` | Roles |
| `asp_net_user_roles` | User-Role mapping |
| `profiles` | Perfil estendido |
| `addresses` | Endereços |
| `sessions` | Sessões ativas |
| `notifications` | Notificações |
| `notification_preferences` | Preferências |
| `login_history` | Histórico de login |

### catalog

| Tabela | Descrição |
|--------|-----------|
| `categories` | Categorias (hierárquica) |
| `brands` | Marcas |
| `products` | Produtos |
| `product_images` | Imagens |
| `product_reviews` | Avaliações |
| `product_stock` | Estoque |
| `stock_movements` | Movimentações |

### cart

| Tabela | Descrição |
|--------|-----------|
| `carts` | Carrinhos |
| `cart_items` | Itens do carrinho |

### orders

| Tabela | Descrição |
|--------|-----------|
| `orders` | Pedidos |
| `order_items` | Itens do pedido |
| `order_status_history` | Histórico de status |
| `order_tracking` | Rastreamento |

### payments

| Tabela | Descrição |
|--------|-----------|
| `payments` | Pagamentos |
| `payment_transactions` | Transações |
| `payment_methods` | Métodos salvos |

### coupons

| Tabela | Descrição |
|--------|-----------|
| `coupons` | Cupons |
| `coupon_rules` | Regras |
| `coupon_usages` | Uso de cupons |

### shared

| Tabela | Descrição |
|--------|-----------|
| `domain_events` | Outbox/Inbox |
| `audit_log` | Audit trail |

## Foreign Keys Cross-Schema

```sql
-- Orders → Users
orders.user_id → users.asp_net_users.id

-- Orders → Products
order_items.product_id → catalog.products.id

-- Payments → Orders
payments.order_id → orders.orders.id

-- Cart → Users
carts.user_id → users.asp_net_users.id

-- Cart → Products  
cart_items.product_id → catalog.products.id
```

Veja [schema.sql](../db/schema.sql) para definição completa.
