using System.Net;
using Rodentia.Web.Models; 

namespace Rodentia.Web.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Виник необроблений виняток: {Message}. Шлях: {Path}", ex.Message, context.Request.Path);

            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.Redirect($"/Home/Error?message={WebUtility.UrlEncode(exception.Message)}");
            return Task.CompletedTask;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        return context.Response.WriteAsJsonAsync(new { 
            error = "Internal Server Error", 
            details = exception.Message 
        });
    }
}