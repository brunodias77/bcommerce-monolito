# API Endpoints

Referência rápida dos endpoints da API BCommerce.

## Base URL

- **Development**: `http://localhost:5000/api`
- **Production**: `https://api.bcommerce.com`

## Autenticação

Todos os endpoints marcados com 🔒 requerem autenticação via JWT Bearer token:

```
Authorization: Bearer <access_token>
```

---

## 🔑 Auth

| Método | Endpoint | Descrição | Auth |
|--------|----------|-----------|------|
| `POST` | `/auth/register` | Registrar novo usuário | ❌ |
| `POST` | `/auth/login` | Login | ❌ |
| `POST` | `/auth/refresh` | Renovar token | ❌ |
| `POST` | `/auth/logout` | Logout (revogar sessão) | 🔒 |
| `POST` | `/auth/forgot-password` | Solicitar reset de senha | ❌ |
| `POST` | `/auth/reset-password` | Resetar senha | ❌ |
| `POST` | `/auth/confirm-email` | Confirmar email | ❌ |

---

## 👤 Users

| Método | Endpoint | Descrição | Auth |
|--------|----------|-----------|------|
| `GET` | `/users/me` | Obter perfil atual | 🔒 |
| `PUT` | `/users/me` | Atualizar perfil | 🔒 |
| `PUT` | `/users/me/password` | Alterar senha | 🔒 |
| `GET` | `/users/me/addresses` | Listar endereços | 🔒 |
| `POST` | `/users/me/addresses` | Adicionar endereço | 🔒 |
| `PUT` | `/users/me/addresses/{id}` | Atualizar endereço | 🔒 |
| `DELETE` | `/users/me/addresses/{id}` | Remover endereço | 🔒 |
| `GET` | `/users/me/sessions` | Listar sessões ativas | 🔒 |
| `DELETE` | `/users/me/sessions/{id}` | Revogar sessão | 🔒 |
| `GET` | `/users/me/notifications` | Listar notificações | 🔒 |
| `PUT` | `/users/me/notifications/{id}/read` | Marcar como lida | 🔒 |

---

## 📦 Catalog

### Products

| Método | Endpoint | Descrição | Auth |
|--------|----------|-----------|------|
| `GET` | `/products` | Listar produtos | ❌ |
| `GET` | `/products/{id}` | Obter produto por ID | ❌ |
| `GET` | `/products/slug/{slug}` | Obter produto por slug | ❌ |
| `GET` | `/products/{id}/reviews` | Listar avaliações | ❌ |
| `POST` | `/products/{id}/reviews` | Adicionar avaliação | 🔒 |

### Categories

| Método | Endpoint | Descrição | Auth |
|--------|----------|-----------|------|
| `GET` | `/categories` | Listar categorias | ❌ |
| `GET` | `/categories/{id}` | Obter categoria | ❌ |
| `GET` | `/categories/{id}/products` | Produtos da categoria | ❌ |
| `GET` | `/categories/tree` | Árvore de categorias | ❌ |

### Brands

| Método | Endpoint | Descrição | Auth |
|--------|----------|-----------|------|
| `GET` | `/brands` | Listar marcas | ❌ |
| `GET` | `/brands/{id}` | Obter marca | ❌ |
| `GET` | `/brands/{id}/products` | Produtos da marca | ❌ |

---

## 🛒 Cart

| Método | Endpoint | Descrição | Auth |
|--------|----------|-----------|------|
| `GET` | `/cart` | Obter carrinho | 🔒 |
| `POST` | `/cart/items` | Adicionar item | 🔒 |
| `PUT` | `/cart/items/{productId}` | Atualizar quantidade | 🔒 |
| `DELETE` | `/cart/items/{productId}` | Remover item | 🔒 |
| `DELETE` | `/cart` | Limpar carrinho | 🔒 |
| `POST` | `/cart/coupon` | Aplicar cupom | 🔒 |
| `DELETE` | `/cart/coupon` | Remover cupom | 🔒 |

---

## 📋 Orders

| Método | Endpoint | Descrição | Auth |
|--------|----------|-----------|------|
| `POST` | `/orders` | Criar pedido (checkout) | 🔒 |
| `GET` | `/orders` | Listar meus pedidos | 🔒 |
| `GET` | `/orders/{id}` | Obter pedido | 🔒 |
| `POST` | `/orders/{id}/cancel` | Cancelar pedido | 🔒 |
| `GET` | `/orders/{id}/tracking` | Rastreamento | 🔒 |

---

## 💳 Payments

| Método | Endpoint | Descrição | Auth |
|--------|----------|-----------|------|
| `GET` | `/payments/methods` | Listar métodos salvos | 🔒 |
| `POST` | `/payments/methods` | Adicionar método | 🔒 |
| `DELETE` | `/payments/methods/{id}` | Remover método | 🔒 |
| `GET` | `/orders/{orderId}/payment` | Status do pagamento | 🔒 |

---

## 🎟️ Coupons

| Método | Endpoint | Descrição | Auth |
|--------|----------|-----------|------|
| `POST` | `/coupons/validate` | Validar cupom | 🔒 |

---

## 🔧 Admin (futuro)

| Método | Endpoint | Descrição | Auth |
|--------|----------|-----------|------|
| `GET` | `/admin/users` | Listar usuários | 🔒 Admin |
| `GET` | `/admin/orders` | Listar todos pedidos | 🔒 Admin |
| `PUT` | `/admin/orders/{id}/status` | Atualizar status | 🔒 Admin |
| `POST` | `/admin/products` | Criar produto | 🔒 Admin |
| `PUT` | `/admin/products/{id}` | Atualizar produto | 🔒 Admin |
| `DELETE` | `/admin/products/{id}` | Remover produto | 🔒 Admin |

---

## Paginação

Endpoints que retornam listas suportam paginação:

```
GET /products?pageNumber=1&pageSize=20&orderBy=name
```

| Parâmetro | Tipo | Default | Descrição |
|-----------|------|---------|-----------|
| `pageNumber` | int | 1 | Número da página |
| `pageSize` | int | 10 | Itens por página (max: 100) |
| `orderBy` | string | - | Campo de ordenação |
| `search` | string | - | Termo de busca |

## Formato de Erro

Erros seguem o padrão [RFC 7807](https://tools.ietf.org/html/rfc7807):

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Resource Not Found",
  "status": 404,
  "detail": "Product with ID xxx was not found",
  "instance": "/api/products/xxx",
  "errorCode": "PRODUCT_NOT_FOUND",
  "traceId": "00-abc123..."
}
```
