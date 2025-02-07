using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Webhooks.API;
public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }


    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddAppInsight(Configuration)
            .AddCustomRouting(Configuration)
            .AddCustomDbContext(Configuration)
            .AddSwagger(Configuration)
            .AddCustomHealthCheck(Configuration)
            .AddDevspaces()
            .AddHttpClientServices(Configuration)
            .AddCustomAuthentication(Configuration)
            .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
            .AddTransient<IIdentityService, IdentityService>()
            .AddTransient<IGrantUrlTesterService, GrantUrlTesterService>()
            .AddTransient<IWebhooksRetriever, WebhooksRetriever>()
            .AddTransient<IWebhooksSender, WebhooksSender>();
    }

    public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
    {        
        var pathBase = Configuration["PATH_BASE"];

        if (!string.IsNullOrEmpty(pathBase))
        {
            loggerFactory.CreateLogger("init").LogDebug("Using PATH BASE '{PathBase}'", pathBase);
            app.UsePathBase(pathBase);
        }

        app.UseRouting();
        app.UseCors("CorsPolicy");
        ConfigureAuth(app);

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapDefaultControllerRoute();
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
        });

        app.UseSwagger()
            .UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"{ (!string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty) }/swagger/v1/swagger.json", "Webhooks.API V1");
                c.OAuthClientId("webhooksswaggerui");
                c.OAuthAppName("WebHooks Service Swagger UI");
            });
    }

    protected virtual void ConfigureAuth(IApplicationBuilder app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
    }
}

internal static class CustomExtensionMethods
{
    public static IServiceCollection AddAppInsight(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplicationInsightsTelemetry(configuration);
        services.AddApplicationInsightsKubernetesEnricher();

        return services;
    }

    public static IServiceCollection AddCustomRouting(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers(options =>
        {
            options.Filters.Add(typeof(HttpGlobalExceptionFilter));
        });

        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy",
                builder => builder
                .SetIsOriginAllowed((host) => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
        });

        return services;
    }

    public static IServiceCollection AddCustomDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEntityFrameworkSqlServer()
            .AddDbContext<WebhooksContext>(options =>
        {
            options.UseSqlServer(configuration["ConnectionString"],
                                    sqlServerOptionsAction: sqlOptions =>
                                    {
                                        sqlOptions.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
                                        //Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
                                        sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                                    });
        });

        return services;
    }

    public static IServiceCollection AddSwagger(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSwaggerGen(options =>
        {            
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "eShopOnContainers - Webhooks HTTP API",
                Version = "v1",
                Description = "The Webhooks Microservice HTTP API. This is a simple webhooks CRUD registration entrypoint"
            });

            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows()
                {
                    Implicit = new OpenApiOAuthFlow()
                    {
                        AuthorizationUrl = new Uri($"{configuration.GetValue<string>("IdentityUrlExternal")}/connect/authorize"),
                        TokenUrl = new Uri($"{configuration.GetValue<string>("IdentityUrlExternal")}/connect/token"),
                        Scopes = new Dictionary<string, string>()
                        {
                            {  "webhooks", "Webhooks API" }
                        }
                    }
                }
            });

            options.OperationFilter<AuthorizeCheckOperationFilter>();
        });

        return services;
    }

    public static IServiceCollection AddCustomHealthCheck(this IServiceCollection services, IConfiguration configuration)
    {
        var hcBuilder = services.AddHealthChecks();

        hcBuilder
            .AddCheck("self", () => HealthCheckResult.Healthy())
            .AddSqlServer(
                configuration["ConnectionString"],
                name: "WebhooksApiDb-check",
                tags: new string[] { "webhooksdb" });

        return services;
    }

    public static IServiceCollection AddHttpClientServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddHttpClient("extendedhandlerlifetime").SetHandlerLifetime(Timeout.InfiniteTimeSpan);
        //add http client services
        services.AddHttpClient("GrantClient")
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                .AddDevspacesSupport();
        return services;
    }
    
    public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var identityUrl = configuration.GetValue<string>("IdentityUrl");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

        }).AddJwtBearer(options =>
        {
            options.Authority = identityUrl;
            options.RequireHttpsMetadata = false;
            options.Audience = "webhooks";
            options.MapInboundClaims = false;
            options.TokenValidationParameters.ValidateAudience = false;
        });

        return services;
    }


    public static IServiceCollection AddCustomAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("ApiScope", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", "webhooks");
            });
        });
        return services;
    }
}
