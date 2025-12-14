# System Context Diagram

Visão de alto nível do sistema BCommerce e suas integrações externas.

## Diagrama C4 - System Context

```mermaid
C4Context
    title System Context - BCommerce

    Person(customer, "Cliente", "Usuário final que compra produtos")
    Person(admin, "Administrador", "Gerencia produtos, pedidos e usuários")
    
    System(bcommerce, "BCommerce", "Plataforma de e-commerce modular monolith")
    
    System_Ext(payment_gateway, "Gateway de Pagamento", "Processa pagamentos (Stripe, PagSeguro)")
    System_Ext(email_service, "Serviço de Email", "Envia emails transacionais")
    System_Ext(storage, "Cloud Storage", "Armazena imagens de produtos")
    System_Ext(shipping, "Correios/Transportadoras", "Calcula frete e rastreia entregas")
    
    Rel(customer, bcommerce, "Navega, compra", "HTTPS")
    Rel(admin, bcommerce, "Gerencia", "HTTPS")
    Rel(bcommerce, payment_gateway, "Processa pagamentos", "HTTPS/API")
    Rel(bcommerce, email_service, "Envia emails", "SMTP/API")
    Rel(bcommerce, storage, "Upload/Download imagens", "HTTPS")
    Rel(bcommerce, shipping, "Consulta frete", "HTTPS/API")
```

## Diagrama Simplificado

```
┌─────────────┐     ┌─────────────┐
│   Cliente   │     │    Admin    │
│   (Web)     │     │    (Web)    │
└──────┬──────┘     └──────┬──────┘
       │                   │
       ▼                   ▼
┌──────────────────────────────────┐
│                                  │
│           BCommerce API          │
│     (Modular Monolith .NET 8)    │
│                                  │
├──────────────────────────────────┤
│  Users │ Catalog │ Cart │ Orders │
│ Payments │ Coupons │ Shared      │
└──────────────────────────────────┘
       │
       ▼
┌──────────────────────────────────┐
│         PostgreSQL               │
│   (Schemas por módulo)           │
└──────────────────────────────────┘
       │
       ├──────────────────────────────┐
       ▼                              ▼
┌──────────────┐              ┌──────────────┐
│   Payment    │              │    Email     │
│   Gateway    │              │   Service    │
└──────────────┘              └──────────────┘
```

## Atores

| Ator | Descrição | Interações |
|------|-----------|------------|
| **Cliente** | Usuário final | Navegar catálogo, adicionar ao carrinho, finalizar compra |
| **Administrador** | Staff interno | Gerenciar produtos, processar pedidos, relatórios |

## Sistemas Externos

| Sistema | Propósito | Protocolo |
|---------|-----------|-----------|
| **Payment Gateway** | Processar transações | REST API |
| **Email Service** | Notificações transacionais | SMTP/API |
| **Cloud Storage** | Imagens de produtos | S3-compatible |
| **Shipping API** | Cálculo de frete e tracking | REST API |
