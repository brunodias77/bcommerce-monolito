# Time Infrastructure

Este diretório contém a implementação concreta para fornecimento de data e hora.

## `DateTimeProvider.cs`
**Responsabilidade:** Implementar a interface `IDateTimeProvider`, retornando o horário atual do sistema em UTC (`DateTime.UtcNow`).
**Por que existe:** Para ser a implementação real usada em tempo de execução (produção/homologação), satisfazendo a dependência definida na camada de `Application`.
**Em que situação usar:** Deve ser registrado no container de Injeção de Dependência da aplicação (`Program.cs` ou `ServiceCollectionExtensions`).
**O que pode dar errado:** Se o relógio do servidor (Host) estiver desconfigurado, todas as datas geradas pela aplicação ("CreatedAt", "OccurredOn") estarão incorretas. Esta classe confia cegamente no `DateTime.UtcNow` do sistema operacional.
**Exemplo real de uso:**
```csharp
// Registro no DI:
services.AddScoped<IDateTimeProvider, DateTimeProvider>();

// Uso indireto via injeção:
public MyService(IDateTimeProvider time) {
    var now = time.UtcNow;
}
```
