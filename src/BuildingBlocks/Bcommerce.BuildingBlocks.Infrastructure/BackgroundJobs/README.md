# Background Jobs Infrastructure

Este diretório contém a infraestrutura para execução detarefas em segundo plano (Background Jobs), utilizando **Quartz.NET** como motor de agendamento e execução. Também inclui implementações padrão de jobs de manutenção do sistema.

## `BackgroundJobRunner.cs`
**Responsabilidade:** Fornecer uma fachada (Facade) para o agendamento dinâmico de jobs (Fire-and-Forget ou Delayed) sem expor a complexidade do `IScheduler` do Quartz diretamente para a camada de aplicação.
**Por que existe:** Para desacoplar a aplicação da biblioteca de Jobs. Se amanhã trocarmos Quartz por Hangfire, apenas esta classe precisa mudar.
**Em que situação usar:** Quando o usuário realizar uma ação que dispara um processamento pesado assíncrono (ex: "Gerar Relatório PDF agora").
**O que pode dar errado:** Se o Quartz não estiver configurado corretamente no DI, o Runner pode lançar exceções de serviço não encontrado.
**Exemplo real de uso:**
```csharp
// Dispara um job imediatamente
_jobRunner.Enqueue<ProcessarPedidoJob>(pedidoId);
```

---

## `IBackgroundJob.cs`
**Responsabilidade:** Definir um contrato agnóstico para jobs, desacoplado da interface `IJob` do Quartz.
**Por que existe:** Para permitir criar jobs que possam ser testados unitariamente sem dependências do contexto de execução do Quartz (`IJobExecutionContext`) e para portabilidade entre bibliotecas.
**Em que situação usar:** Para jobs simples que não precisam de configurações avançadas do Quartz.
**O que pode dar errado:** Esta interface precisa de um adaptador para rodar dentro do Quartz. Se implementada diretamente sem o adaptador registrado, o job nunca rodará.
**Exemplo real de uso:**
```csharp
public class EnviarEmailJob : IBackgroundJob {
    public async Task ExecuteAsync(CancellationToken ct) {
        // Lógica pura, sem Quartz context
    }
}
```

---

## `Jobs/AbandonedCartsJob.cs`
**Responsabilidade:** Identificar e processar carrinhos de compra que não foram finalizados após um certo período.
**Por que existe:** Para recuperar vendas perdidas (Revenue Recovery) enviando lembretes aos clientes.
**Em que situação usar:** Agendado via CRON (ex: a cada hora).
**O que pode dar errado:** Enviar e-mails repetidos se o status do carrinho não for atualizado ou se a "janela de tempo" de busca for mal definida.
**Exemplo real de uso:**
```csharp
// Lógica interna:
var carts = await _repo.GetAbandoned(TimeSpan.FromHours(24));
foreach(var cart in carts) _email.SendCleanup(cart);
```

---

## `Jobs/ExpiredCouponsJob.cs`
**Responsabilidade:** Invalidar cupons cuja data de validade já passou.
**Por que existe:** Embora a validação ocorra no uso, este job mantém a base limpa e atualiza status para relatórios, garantindo que o backoffice veja o estado real.
**Em que situação usar:** Execução diária (ex: meia-noite).
**O que pode dar errado:** Invalidar cupons que ainda são válidos devido a problemas de fuso horário (Timezone mismatch entre servidor e banco).
**Exemplo real de uso:**
```csharp
// SQL: UPDATE Coupons SET Status = 'Expired' WHERE ValidUntil < GETDATE()
```

---

## `Jobs/ExpiredPaymentsJob.cs`
**Responsabilidade:** Cancelar pedidos aguardando pagamento (PIX/Boleto) que excederam o tempo limite.
**Por que existe:** Para liberar a reserva de estoque dos itens presos nesses pedidos, permitindo que outros clientes comprem.
**Em que situação usar:** Execução frequente (`0/15 * * * * ?` - a cada 15 min).
**O que pode dar errado:** Cancelar um pedido onde o pagamento (webhook) chegou com atraso de milissegundos. É necessário uma margem de tolerância.
**Exemplo real de uso:**
```csharp
// Busca pedidos 'AguardandoPagamento' com CreatedAt < Now - 30min
```

---

## `Jobs/ExpiredReservationsCleanupJob.cs`
**Responsabilidade:** Limpeza "grossa" de reservas de estoque órfãs (que por algum bug não estão atreladas a pedidos).
**Por que existe:** Como rede de segurança (Safety Net). Se o `ExpiredPaymentsJob` falhar ou se o app cair no meio de uma transação, este job garante a consistência eventual do estoque.
**Em que situação usar:** Execução menos frequente (ex: horária).
**O que pode dar errado:** Deletar reservas ativas de um checkout em andamento muito longo.
**Exemplo real de uso:**
```csharp
// Remove entradas na tabela InventoryReservations sem pedido atrelado há > 1h
```

---

## `Jobs/MaterializedViewRefreshJob.cs`
**Responsabilidade:** Atualizar tabelas de leitura (Views Materializadas) usadas por dashboards e relatórios.
**Por que existe:** Para evitar que o Dashboard do Admin faça `COUNT(*)` pesados na tabela de Vendas em tempo real. O job pré-calcula esses dados.
**Em que situação usar:** Em horários de baixo tráfego (madrugada) ou janelas específicas.
**O que pode dar errado:** Bloquear as tabelas (Table Lock) durante o refresh, causando timeout na operação da loja.
**Exemplo real de uso:**
```csharp
// Executa proc: REFESH MATERIALIZED VIEW SalesSummary
```
