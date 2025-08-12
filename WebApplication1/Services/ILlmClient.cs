using System.Threading;
using System.Threading.Tasks;

namespace WebApplication1.Services
{
    public interface ILlmClient
    {
        Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default);
    }
}
