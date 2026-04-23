using System.Security.Claims;
using System.Text;

namespace Rodentia.Web.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;
        var method = request.Method;
        var url = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var headers = request.Headers.ToDictionary(
            h => h.Key,
            h => string.Join(", ", h.Value.ToArray()));
        var body = await ReadRequestBodyAsync(request);
        var userId = context.User?.Identity?.IsAuthenticated == true
            ? context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            : null;

        _logger.LogInformation(
            "HTTP Request: Method={Method}; Url={Url}; Ip={Ip}; Headers={Headers}; Body={Body}; UserId={UserId}",
            method,
            url,
            ip,
            headers,
            body,
            userId);

        await _next(context);
    }

    private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        request.EnableBuffering();
        request.Body.Position = 0;

        using var reader = new StreamReader(request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;

        return body;
    }
}
