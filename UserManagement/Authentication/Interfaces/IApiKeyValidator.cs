using Domain.Models;

namespace Infrastructure.Interfaces
{
    public interface IApiKeyValidator
    {
        Task<Client?> GetValidApiKeyAsync(string? apiKey);
    }
}
