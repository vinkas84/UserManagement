using Infrastructure.Interfaces;
using DataAccess;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class ApiKeyValidator : IApiKeyValidator
    {
        private readonly ApplicationDbContext _context;

        public ApiKeyValidator(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Client?> GetValidApiKeyAsync(string? apiKey)
        {
            return string.IsNullOrEmpty(apiKey) ? default : await _context.Clients.FirstOrDefaultAsync(c => c.ApiKey == apiKey);
        }
    }
}
