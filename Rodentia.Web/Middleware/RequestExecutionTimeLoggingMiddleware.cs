using System.Diagnostics;

namespace Rodentia.Web.Middleware;

public class RequestExecutionTimeLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestExecutionTimeLoggingMiddleware> _logger;

    public RequestExecutionTimeLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestExecutionTimeLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;
        var method = request.Method;
        var url = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";

        var stopwatch = Stopwatch.StartNew();
        await _next(context);
        stopwatch.Stop();

        _logger.LogInformation(
            "HTTP Request Execution Time: Method={Method}; Url={Url}; StatusCode={StatusCode}; ElapsedMs={ElapsedMs}",
            method,
            url,
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds);
    }
}
