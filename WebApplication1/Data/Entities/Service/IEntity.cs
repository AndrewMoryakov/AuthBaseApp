using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Data.Entities.Service;

public interface IEntity<TKey> : IHasKey<TKey>, ITrackable
{
    public TKey Id { get; set; }

    public DateTimeOffset CreatedDateTime { get; set; }

    public DateTimeOffset? UpdatedDateTime { get; set; }
}