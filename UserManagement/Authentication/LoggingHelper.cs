using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Text;

namespace Infrastructure
{
    public static class LoggingHelper
    {
        // Log both request and response data together
        public static void LogRequestAndResponse(HttpContext context, string requestData)
        {
            var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
            var clientName = context.Items["ClientName"] != null ? context.Items["ClientName"].ToString() : "Unknown Name";

            var logLevel = context.Response.StatusCode >= 400 ? LogLevel.Error : LogLevel.Information;
            string? responseMessage = context.Response.StatusCode switch
            {
                200 or 201 => $"Success - Status {context.Response.StatusCode}",
                400 => $"Bad Request - Status {context.Response.StatusCode}",
                401 => $"Unauthorized - Status {context.Response.StatusCode}",
                403 => $"Forbidden - Status {context.Response.StatusCode}",
                404 => $"Not Found - Status {context.Response.StatusCode}",
                500 => $"Internal Server Error - Status {context.Response.StatusCode}",
                _ => $"Other Status - Status {context.Response.StatusCode}",
            };

            var logMessage = $"{DateTime.UtcNow} - Client IP: {clientIp} - Client Name: {clientName} - Host: {Environment.MachineName} - Method: {context.Request.Method} - Request Path: {context.Request.Path} - Request parameters: {requestData} - Message: {responseMessage}";

            // Log request and response data together
            if (logLevel == LogLevel.Error)
            {
                Log.ForContext("ApiCall", true).Error(logMessage);
            }
            else
            {
                Log.ForContext("ApiCall", true).Information(logMessage);
            }
        }

        // Capture request data
        public static async Task<string> GetRequestData(HttpContext context)
        {
            var requestQueryParams = ReadRequestQueryParams(context.Request);

            var requestBody = string.Empty;
            // Log request body
            if (context.Request.Method == "POST" || context.Request.Method == "PUT")
            {
                requestBody = await ReadRequestBody(context.Request);
            }

            return $"{requestQueryParams}, {requestBody}";
        }

        // Read request query parameters
        private static string ReadRequestQueryParams(HttpRequest request)
        {
            var requestInfo = new StringBuilder();

            if (request.Query.Count != 0)
            {
                requestInfo.Append("Query Params [");
                foreach (var queryParam in request.Query)
                {
                    requestInfo.Append($"{queryParam.Key} = {queryParam.Value}, ");
                }
                requestInfo.Remove(requestInfo.Length - 2, 2);
                requestInfo.Append(']');
            }

            return requestInfo.ToString();
        }

        // Read request body
        private static async Task<string> ReadRequestBody(HttpRequest request)
        {
            request.EnableBuffering(); // Allows reading the request body multiple times
            request.Body.Position = 0; // Ensure we start reading from the beginning

            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            string body = await reader.ReadToEndAsync();

            request.Body.Position = 0; // Reset stream position so other middleware can read it again
            return string.IsNullOrEmpty(body) ? string.Empty : $"Body: {body}";
        }                
    }
}
