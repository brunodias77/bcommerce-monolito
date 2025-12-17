using Bcommerce.BuildingBlocks.Infrastructure.Inbox.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bcommerce.BuildingBlocks.Infrastructure.Inbox.Configuration;

/// <summary>
/// Configuração do Entity Framework para a tabela de Invoice Messages.
/// </summary>
/// <remarks>
/// Mapeia a entidade <see cref="InboxMessage"/> para o banco de dados.
/// - Define chaves primárias e propriedades obrigatórias
/// - Configura o nome da tabela como "InboxMessages"
/// - Otimiza colunas para consulta pelo processador
/// 
/// Exemplo de uso:
/// <code>
/// modelBuilder.ApplyConfiguration(new InboxConfiguration());
/// </code>
/// </remarks>
public class InboxConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("InboxMessages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Content).IsRequired();
        builder.Property(x => x.Type).IsRequired();
    }
}
