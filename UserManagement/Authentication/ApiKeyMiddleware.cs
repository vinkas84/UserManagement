using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Infrastructure
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private const string ApiKeyHeaderName = "X-API-Key";
        private readonly IServiceScopeFactory _scopeFactory;

        public ApiKeyMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _scopeFactory = scopeFactory;
        }

        public async Task Invoke(HttpContext context)
        {
            // Skip API key validation for Swagger UI and any other public endpoints
            if (context.Request.Path.StartsWithSegments("/swagger") || context.Request.Path.StartsWithSegments("/health"))
            {
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                var response = new
                {
                    error = "API Key is missing",
                    statusCode = context.Response.StatusCode
                };

                var jsonResponse = JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(jsonResponse);
                return;
            }

            using (var scope = _scopeFactory.CreateScope())
            {
                var apiKeyValidator = scope.ServiceProvider.GetRequiredService<IApiKeyValidator>();

                var client = await apiKeyValidator.GetValidApiKeyAsync(extractedApiKey);
                if (client == null)
                {
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = StatusCodes.Status403Forbidden; var response = new
                    {
                        error = "Invalid API Key",
                        statusCode = context.Response.StatusCode
                    };

                    var jsonResponse = JsonSerializer.Serialize(response);
                    await context.Response.WriteAsync(jsonResponse);
                    return;
                }

                context.Items["ClientName"] = client.Name;
            }

            // Allow request to continue
            await _next(context);
        }
    }
}
