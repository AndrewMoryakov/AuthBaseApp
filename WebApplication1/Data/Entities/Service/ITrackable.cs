namespace WebApplication1.Data.Entities.Service;

public interface ITrackable
{
    DateTimeOffset CreatedDateTime { get; set; }

    DateTimeOffset? UpdatedDateTime { get; set; }   
}