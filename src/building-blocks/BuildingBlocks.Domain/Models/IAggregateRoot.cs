namespace BuildingBlocks.Domain.Models;

/// <summary>
/// Interface marcadora para identificar raízes de agregado no DDD
/// Um agregado é um cluster de objetos de domínio que podem ser tratados como uma única unidade
/// A raiz do agregado é o único ponto de entrada para modificações no agregado
/// </summary>
public interface IAggregateRoot
{
    // Interface vazia - serve apenas como marcador
    // Usada para identificar entidades que são raízes de agregados
    // e que devem gerenciar seus próprios eventos de domínio
}