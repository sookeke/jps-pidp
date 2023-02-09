namespace Pidp;

using FluentValidation.AspNetCore;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using Serilog;
using Swashbuckle.AspNetCore.Filters;
using System.Reflection;
using System.Text.Json;

using Pidp.Data;
using Pidp.Extensions;
using Pidp.Features;
using Pidp.Infrastructure;
using Pidp.Infrastructure.Auth;
using Pidp.Infrastructure.HttpClients;
using Pidp.Infrastructure.Services;
using Pidp.Helpers.Middleware;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Pidp.Features.Organization.UserTypeService;
using Pidp.Features.Organization.OrgUnitService;
using Microsoft.Extensions.Configuration;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry;
using System.Diagnostics;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using Pidp.Infrastructure.Telemetry;
using Azure.Monitor.OpenTelemetry.Exporter;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Prometheus;
using Microsoft.Extensions.Diagnostics.HealthChecks;

public class Startup
{



    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        this.Configuration = configuration;
        StaticConfig = configuration;
    }
    public static IConfiguration StaticConfig { get; private set; }


    public void ConfigureServices(IServiceCollection services)
    {
        var config = this.InitializeConfiguration(services);

        var assemblyVersion = Assembly.GetExecutingAssembly()
    .GetName().Version?.ToString() ?? "0.0.0";


        if (!string.IsNullOrEmpty(config.Telemetry.CollectorUrl))
        {

            Action<ResourceBuilder> configureResource = r => r.AddService(
                 serviceName: TelemetryConstants.ServiceName,
                 serviceVersion: Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown",
                serviceInstanceId: Environment.MachineName);

            Log.Logger.Information("Telemetry logging is enabled {0}", config.Telemetry.CollectorUrl);
            var resource = ResourceBuilder.CreateDefault().AddService(TelemetryConstants.ServiceName);

            services.AddOpenTelemetry()
                .ConfigureResource(configureResource)
                .WithTracing(builder =>
                {
                    builder.SetSampler(new AlwaysOnSampler())
                        .AddHttpClientInstrumentation()
                        .AddEntityFrameworkCoreInstrumentation(options => options.SetDbStatementForText = true)
                        .AddAspNetCoreInstrumentation();
                    if (config.Telemetry.LogToConsole)
                    {
                        builder.AddConsoleExporter();
                    }
                    if (config.Telemetry.AzureConnectionString != null)
                    {
                        builder.AddAzureMonitorTraceExporter(o => o.ConnectionString = config.Telemetry.AzureConnectionString);
                    }
                    if (config.Telemetry.CollectorUrl != null)
                    {
                        builder.AddOtlpExporter(options =>
                            {
                                options.Endpoint = new Uri(config.Telemetry.CollectorUrl);
                                options.Protocol = OtlpExportProtocol.HttpProtobuf;
                            });
                    }
                })
                .WithMetrics(builder =>
                    builder.AddHttpClientInstrumentation()
                        .AddAspNetCoreInstrumentation()).StartWithHost();




        }

        services
        .AddAutoMapper(typeof(Startup))
        .AddHttpClients(config)
        .AddKeycloakAuth(config)
        .AddScoped<IEmailService, EmailService>()
        .AddScoped<IPidpAuthorizationService, PidpAuthorizationService>()
        .AddSingleton<IClock>(SystemClock.Instance);

        services.AddSingleton<ProblemDetailsFactory, JpidpProblemDetailsFactory>();

        services.AddControllers(options => options.Conventions.Add(new RouteTokenTransformerConvention(new KabobCaseParameterTransformer())))
            .AddFluentValidation(options => options.RegisterValidatorsFromAssemblyContaining<Startup>())
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            })
            .AddHybridModelBinder();

        services.AddDbContext<PidpDbContext>(options => options
            .UseNpgsql(config.ConnectionStrings.PidpDatabase, npg => npg.UseNodaTime())
            .EnableSensitiveDataLogging(sensitiveDataLoggingEnabled: false));

        services.Scan(scan => scan
            .FromAssemblyOf<Startup>()
            .AddClasses(classes => classes.AssignableTo<IRequestHandler>())
            .AsImplementedInterfaces()
            .WithTransientLifetime());

        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        services.AddScoped<IUserTypeService, UserTypeService>();
        services.AddScoped<IOrgUnitService, OrgUnitService>();


        services.AddHealthChecks()
                .AddCheck("liveliness", () => HealthCheckResult.Healthy())
                .AddNpgSql(config.ConnectionStrings.PidpDatabase, tags: new[] { "services" }).ForwardToPrometheus();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "PIdP Web API", Version = "v1" });
            options.AddSecurityDefinition("OAuth2", new OpenApiSecurityScheme
            {
                Description = "Standard Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
                In = ParameterLocation.Header,
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey
            });
            options.OperationFilter<SecurityRequirementsOperationFilter>();
            options.CustomSchemaIds(x => x.FullName);
        });
        services.AddFluentValidationRulesToSwagger();

        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        Log.Logger.Information("Startup configuration complete");



    }

    private PidpConfiguration InitializeConfiguration(IServiceCollection services)
    {
        var config = new PidpConfiguration();

        this.Configuration.Bind(config);

        services.AddSingleton(config);

        Log.Logger.Information("### App Version:{0} ###", Assembly.GetExecutingAssembly().GetName().Version);
        Log.Logger.De("### PIdP Configuration:{0} ###", System.Text.Json.JsonSerializer.Serialize(config));


        if (Environment.GetEnvironmentVariable("JUSTIN_SKIP_USER_EMAIL_CHECK") is not null and "true")
        {
            Log.Logger.Warning("*** JUSTIN EMAIL VERIFICATION IS DISABLED ***");
        }

        return config;
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }


        //app.UseMiddleware<ExceptionHandlerMiddleware>();
        app.UseExceptionHandler(
            new ExceptionHandlerOptions()
            {
                AllowStatusCode404Response = true,
                ExceptionHandlingPath = "/error"
            }
            );// "/error");

        app.UseSwagger();
        app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "PIdP Web API"));

        app.UseSerilogRequestLogging(options => options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            var userId = httpContext.User.GetUserId();
            if (!userId.Equals(Guid.Empty))
            {
                diagnosticContext.Set("User", userId);
            }
        });
        app.UseRouting();
        app.UseCors("CorsPolicy");
        app.UseMetricServer();
        app.UseHttpMetrics();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapMetrics();
            endpoints.MapHealthChecks("/health/liveness").AllowAnonymous();
        });



    }
}
