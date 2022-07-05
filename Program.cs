using AllocationSystem.WebApi;
using AllocationSystem.WebApi.Controllers;
using AllocationSystem.WebApi.Data;
using AllocationSystem.WebApi.Models;
using FluentValidation.AspNetCore;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using AllocationSystem.WebApi.Business;
using System.Reflection;

try
{

    var builder = WebApplication.CreateBuilder(args);
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
    builder.Logging.AddDebug();

    // Add services to the container.

    builder.Services.AddControllers()
          .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<CreateUserValidator>());
    builder.Services.Configure<IISOptions>(options => { options.AutomaticAuthentication = false; });
    ConfigurationManager configuration = builder.Configuration;

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwagger();
    builder.Services.AddConfiguration();
    builder.Services.AddCustomAuthentication();
    builder.Services.AddHealthChecks(configuration);
    
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("CorsPolicy",
            builder => builder
            .SetIsOriginAllowed((host) => true)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
    });

    builder.Services.AddDbContext<AllocationSystemDbContext>(options =>
    {
        options.UseSqlServer(configuration["ConnectionStrings:Default"],
            sqlServerOptionsAction: sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(BaseController).GetTypeInfo().Assembly.GetName().Name);
                sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
            });
    }, ServiceLifetime.Scoped
    );
    
    builder.Services.AddAutoMapper(typeof(MappingProfile));
    builder.Services.AddMemoryCache();
    builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
    builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
    builder.Services.AddScoped<IAutoAllocation, AutoAllocation>();
    builder.Services.AddScoped<IBusiness, Business>();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseMigrationsEndPoint();
    }
    
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseHttpsRedirection();

    app.UseRouting();

    app.UseCors("CorsPolicy");

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
        endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        endpoints.MapHealthChecks("/liveness", new HealthCheckOptions
        {
            Predicate = r => r.Name.Contains("self")
        });
        endpoints.MapFallbackToFile("index.html");
    });
    //app.AddEfDiagrams<AllocationSystemDbContext>();
    using (IServiceScope scope = app.Services.CreateScope())
    {
        IServiceProvider services = scope.ServiceProvider;
        ILoggerFactory loggerFactory = services.GetRequiredService<ILoggerFactory>();

        AllocationSystemDbContext linkGoDimesContext = services.GetRequiredService<AllocationSystemDbContext>();
        await AllocationSystemDbContextSeed.SeedAsync(linkGoDimesContext, loggerFactory);
    }

    app.Run();

}
catch (Exception ex)
{
    string type = ex.GetType().Name;
    if (type.Equals("StopTheHostException", StringComparison.Ordinal))
    {
        throw;
    }
    Console.Write(ex.ToString());
}