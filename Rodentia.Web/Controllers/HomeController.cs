using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Rodentia.Core.Interfaces;
using Rodentia.Web.Models;

namespace Rodentia.Web.Controllers;

public class HomeController : Controller
{
    private readonly IGeoTimeZoneService _geoTimeZoneService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        ILogger<HomeController> logger,
        IGeoTimeZoneService geoTimeZoneService)
    {
        _logger = logger;
        _geoTimeZoneService = geoTimeZoneService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var visitorIp = ResolveClientIpAddress();
        var visitorTimeZone = await _geoTimeZoneService.GetVisitorTimeZoneAsync(visitorIp, cancellationToken);

        ViewData["VisitorTimeZone"] = visitorTimeZone.TimeZoneId;
        ViewData["VisitorLocalTime"] = visitorTimeZone.LocalTime.ToString("yyyy-MM-dd HH:mm");
        ViewData["VisitorLocation"] = $"{visitorTimeZone.City}, {visitorTimeZone.Country}";
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var message = HttpContext.Request.Query["message"];

        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            Message = message
        });
    }

    private string ResolveClientIpAddress()
    {
        var forwardedFor = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwardedFor))
            return forwardedFor.Split(',')[0].Trim();

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
    }
}
