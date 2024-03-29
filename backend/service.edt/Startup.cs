
namespace edt.service;

using System.Reflection;
using System.Text.Json;
using edt.service.Data;
using edt.service.HttpClients;
using edt.service.Kafka;
using edt.service.ServiceEvents.UserAccountCreation.ConsumerRetry;
using edt.service.ServiceEvents.UserAccountCreation.Handler;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using NodaTime;
using Serilog;
using Swashbuckle.AspNetCore.Filters;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
        StaticConfig = configuration;
    }

    public static IConfiguration StaticConfig { get; private set; }


    public void ConfigureServices(IServiceCollection services)
    {
        var config = this.InitializeConfiguration(services);
        services
          .AddAutoMapper(typeof(Startup))
          .AddKafkaConsumer(config)
          .AddSingleton(new RetryPolicy(config))
          .AddHttpClients(config)
          .AddSingleton<IClock>(SystemClock.Instance)
          .AddSingleton<Microsoft.Extensions.Logging.ILogger>(svc => svc.GetRequiredService<ILogger<UserProvisioningHandler>>());

        services.AddAuthorization(options =>
        {
            //options.AddPolicy("Administrator", policy => policy.Requirements.Add(new RealmAccessRoleRequirement("administrator")));
        });

        services.AddDbContext<EdtDataStoreDbContext>(options => options
            .UseSqlServer(config.ConnectionStrings.EdtDataStore, sql => sql.UseNodaTime())
            .EnableSensitiveDataLogging(sensitiveDataLoggingEnabled: false));

        services.AddHealthChecks()
                .AddCheck("liveliness", () => HealthCheckResult.Healthy())
                .AddSqlServer(config.ConnectionStrings.EdtDataStore, tags: new[] { "services" });

        services.AddControllers();
        services.AddHttpClient();

        //services.AddSingleton<ProblemDetailsFactory, UserManagerProblemDetailsFactory>();
        //services.AddHealthChecks();

        services.AddApiVersioning(options =>
        {
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = new HeaderApiVersionReader("api-version");
        });

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Notification Service API", Version = "v1" });
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Standard Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
                In = ParameterLocation.Header,
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,

                        },
                        new List<string>()
                    }
                });
            options.OperationFilter<SecurityRequirementsOperationFilter>();
            options.CustomSchemaIds(x => x.FullName);
        });
        services.AddFluentValidationRulesToSwagger();

        //services.AddKafkaConsumer(config);

    }
    private EdtServiceConfiguration InitializeConfiguration(IServiceCollection services)
    {
        var config = new EdtServiceConfiguration();
        this.Configuration.Bind(config);
        services.AddSingleton(config);

        Log.Logger.Information("### App Version:{0} ###", Assembly.GetExecutingAssembly().GetName().Version);
        Log.Logger.Information("### Noticification Service Configuration:{0} ###", JsonSerializer.Serialize(config));

        return config;
    }
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();

        }
        //app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseExceptionHandler("/error");
        app.UseSwagger();
        app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Notification Service API"));

        app.UseSerilogRequestLogging(options => options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            //var userId = httpContext.User.GetUserId();
            //if (!userId.Equals(Guid.Empty))
            //{
            //    diagnosticContext.Set("User", userId);
            //}
        });
        app.UseRouting();
        app.UseCors("CorsPolicy");
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHealthChecks("/health/liveness").AllowAnonymous();
        });

    }
}
