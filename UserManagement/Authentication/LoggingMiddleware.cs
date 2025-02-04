using Microsoft.AspNetCore.Http;

namespace Infrastructure
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public LoggingMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip logging for Swagger UI and any other public endpoints
            if (!context.Request.Path.StartsWithSegments("/api/users"))
            {
                await _next(context);
                return;
            }

            // Store the request data in HttpContext.Items
            var requestData = await LoggingHelper.GetRequestData(context);

            // Continue to the next middleware (or controller)
            await _next(context);

            // Log the request and response data together
            LoggingHelper.LogRequestAndResponse(context, requestData);            
        }
       }
}
