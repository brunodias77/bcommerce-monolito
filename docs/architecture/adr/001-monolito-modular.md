# ADR 001: Adoção de Arquitetura Monolito Modular

## Status

**Aceito** - Dezembro 2024

## Contexto

O projeto BCommerce precisa de uma arquitetura que:
- Permita desenvolvimento rápido inicial
- Seja facilmente mantida por uma equipe pequena
- Possibilite evolução futura para microserviços se necessário
- Mantenha separação clara entre domínios de negócio

### Opções Consideradas

1. **Monolito Tradicional**: Simples, mas difícil de escalar e manter
2. **Microserviços**: Escalável, mas complexo para equipe pequena
3. **Monolito Modular**: Balanceado, com separação lógica e caminho para evolução

## Decisão

Adotamos **Monolito Modular** com as seguintes características:

### Estrutura de Módulos

```
src/
├── modules/
│   ├── users/        # Autenticação, perfis, sessões
│   ├── catalog/      # Produtos, categorias, estoque
│   ├── cart/         # Carrinho de compras
│   ├── orders/       # Pedidos e processamento
│   ├── payments/     # Pagamentos e transações
│   └── coupons/      # Cupons e promoções
└── building-blocks/  # Código compartilhado
```

### Princípios

1. **Isolamento de Dados**: Cada módulo tem seu próprio schema no PostgreSQL
2. **Comunicação via Eventos**: Módulos se comunicam via Integration Events (Outbox Pattern)
3. **Dependências Unidirecionais**: Módulos dependem apenas de building-blocks
4. **API Gateway Único**: Uma única API expõe todos os endpoints

## Consequências

### Positivas
- ✅ Deploy simplificado (único artefato)
- ✅ Transações locais mais simples
- ✅ Debugging mais fácil
- ✅ Menor overhead de infraestrutura
- ✅ Caminho claro para microserviços (extrair módulo)

### Negativas
- ❌ Escala horizontal limitada (toda aplicação escala junto)
- ❌ Risco de acoplamento se disciplina não for mantida
- ❌ Single point of failure

## Referências

- [Modular Monolith Primer](https://www.kamilgrzybek.com/design/modular-monolith-primer/)
- [.NET Modular Monolith with DDD](https://github.com/kgrzybek/modular-monolith-with-ddd)
