using System.Diagnostics;
using System.Text.Json;

namespace Presentation_API.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;
        private readonly string _logDirectory;
        private static readonly object _lock = new object();

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger, IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _logDirectory = Path.Combine(env.ContentRootPath, "Logs");
            
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        public async Task Invoke(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            
            // Capture request details
            var method = context.Request.Method;
            var path = context.Request.Path;
            var queryString = context.Request.QueryString.ToString();

            // Check if this request should be logged
            var shouldLog = !path.StartsWithSegments("/hub/notifications") && 
                             !path.StartsWithSegments("/api/health") &&
                             !path.StartsWithSegments("/api/notifications");

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                var statusCode = context.Response.StatusCode;
                var elapsedMs = stopwatch.ElapsedMilliseconds;

                if (shouldLog)
                {
                    // Console Logging
                    _logger.LogInformation(
                        "Request: {Method} {Path}{QueryString} responded {StatusCode} in {Elapsed}ms",
                        method, path, queryString, statusCode, elapsedMs);

                    // File Logging
                    try 
                    {
                        var logEntry = new
                        {
                            Timestamp = DateTime.UtcNow,
                            Method = method,
                            Path = path,
                            QueryString = queryString,
                            StatusCode = statusCode,
                            DurationMs = elapsedMs
                        };

                        var jsonLine = JsonSerializer.Serialize(logEntry) + Environment.NewLine;
                        var fileName = $"api_requests_{DateTime.UtcNow:yyyyMMdd}.json";
                        var filePath = Path.Combine(_logDirectory, fileName);

                        lock (_lock)
                        {
                            File.AppendAllText(filePath, jsonLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to write API request log to file");
                    }
                }
            }
        }
    }
}
