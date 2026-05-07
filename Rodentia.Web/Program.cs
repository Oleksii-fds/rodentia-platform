using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Rodentia.Core.Entities;
using Rodentia.Core.Interfaces;
using Rodentia.Core.Models;
using Rodentia.Core.Services;
using Rodentia.Data;
using Rodentia.Data.Repositories;
using Rodentia.Web.Hubs;
using Rodentia.Web.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.Seq("http://localhost:5341")
    .CreateLogger();

builder.Host.UseSerilog();


if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Services.Configure<RodentiaOptions>(
    builder.Configuration.GetSection(RodentiaOptions.SectionName));
builder.Services.Configure<GeoTimeZoneOptions>(
    builder.Configuration.GetSection(GeoTimeZoneOptions.SectionName));


if (builder.Environment.EnvironmentName != "Testing")
{
    builder.Services.AddDbContext<RodentiaDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}

builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<RodentiaDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddHttpClient<IGeoTimeZoneClient, IpWhoIsGeoTimeZoneClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<GeoTimeZoneOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("RodentiaPlatform/1.0");
});
builder.Services.AddScoped<IGeoTimeZoneService, GeoTimeZoneService>();
builder.Services.AddScoped<ILessonRepository, LessonRepository>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<ILessonRescheduleRequestRepository, LessonRescheduleRequestRepository>();
builder.Services.AddScoped<ILessonRescheduleRequestService, LessonRescheduleRequestService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddSignalR();
builder.Services.AddHostedService<NotificationEventsBackgroundService>();

builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();
builder.Services.AddWebOptimizer(pipeline =>
{
    pipeline.AddCssBundle("/bundles/site.min.css",
        "lib/bootstrap/dist/css/bootstrap.min.css",
        "css/site.css",
        "css/profile-modal.css",
        "css/home.css",
        "css/schedule.css",
        "css/debt.css",
        "css/teacher.css",
        "css/student-profile-modal.css");

    pipeline.AddCssBundle("/bundles/auth.min.css",
        "lib/bootstrap/dist/css/bootstrap.min.css",
        "css/site.css",
        "css/auth.css");

    pipeline.AddJavaScriptBundle("/bundles/site.min.js",
        "lib/jquery/dist/jquery.min.js",
        "lib/bootstrap/dist/js/bootstrap.bundle.min.js",
        "js/site.js",
        "js/profile-modal.js",
        "js/notifications.js",
        "js/schedule-create-lesson.js",
        "js/home-timezones.js",
        "js/debt.js",
        "js/teacher-students.js");

    pipeline.AddJavaScriptBundle("/bundles/auth.min.js",
        "lib/jquery/dist/jquery.min.js",
        "lib/bootstrap/dist/js/bootstrap.bundle.min.js",
        "js/register.js");
});

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    foreach (var role in new[] { "Teacher", "Student" })
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole<Guid>(role));
    }
}

app.UseMiddleware<Rodentia.Web.Middleware.ExceptionHandlingMiddleware>();
app.UseWebOptimizer();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else if (app.Environment.IsStaging())
{
    // Staging
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    // Production
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseMiddleware<Rodentia.Web.Middleware.RequestExecutionTimeLoggingMiddleware>();
app.UseAuthentication();
app.UseMiddleware<Rodentia.Web.Middleware.RequestLoggingMiddleware>();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();

public partial class Program { }