namespace WebApplication1.Security;

public sealed class ServiceClientOptions
{
    public Dictionary<string, ServiceClient> Clients { get; set; } = new();
}

public sealed class ServiceClient
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public List<string> Scopes { get; set; } = new();
}

