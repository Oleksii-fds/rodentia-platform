using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rodentia.Data;

namespace Rodentia.IntegrationTests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing"); 
        
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<RodentiaDbContext>));
            services.RemoveAll(typeof(DbContextOptions));
            services.RemoveAll(typeof(DbConnection));

            services.AddDbContext<RodentiaDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting");
            });
        });
    }
}