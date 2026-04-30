using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Rodentia.Web.Filters;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class RateLimitByIpAttribute : ActionFilterAttribute
{
    private static readonly ConcurrentDictionary<string, ConcurrentQueue<DateTime>> RequestsByIp = new();

    public RateLimitByIpAttribute(int maxRequestsPerMinute)
    {
        MaxRequestsPerMinute = maxRequestsPerMinute;
    }

    public int MaxRequestsPerMinute { get; }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var ipAddress = ResolveIpAddress(context.HttpContext);
        var now = DateTime.UtcNow;
        var threshold = now.AddMinutes(-1);
        var requestQueue = RequestsByIp.GetOrAdd(ipAddress, static _ => new ConcurrentQueue<DateTime>());

        while (requestQueue.TryPeek(out var timestamp) && timestamp < threshold)
        {
            requestQueue.TryDequeue(out _);
        }

        if (requestQueue.Count >= MaxRequestsPerMinute)
        {
            context.Result = new RedirectToActionResult(
                actionName: "Error",
                controllerName: "Home",
                routeValues: new { message = $"Перевищено ліміт запитів: не більше {MaxRequestsPerMinute} за хвилину з однієї IP-адреси." });
            return;
        }

        requestQueue.Enqueue(now);
    }

    private static string ResolveIpAddress(HttpContext httpContext)
    {
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwardedFor))
            return forwardedFor.Split(',')[0].Trim();

        return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
