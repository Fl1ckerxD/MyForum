using FluentValidation;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Minio;
using MyForum.Api.Application.Factories;
using MyForum.Api.Core.Entities;
using MyForum.Api.Core.Interfaces.Factories;
using MyForum.Api.Core.Interfaces.Metrics;
using MyForum.Api.Core.Interfaces.Repositories;
using MyForum.Api.Core.Interfaces.Services;
using MyForum.Api.Core.MappingProfiles;
using MyForum.Api.Core.Metrics;
using MyForum.Api.Core.Validations;
using MyForum.Api.Infrastructure.Data;
using MyForum.Api.Infrastructure.HealthChecks;
using MyForum.Api.Infrastructure.Repositories;
using MyForum.Api.Infrastructure.Services;
using Npgsql;
using OpenTelemetry.Metrics;
using Serilog;
using StackExchange.Redis;

namespace MyForum.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                var builder = WebApplication.CreateBuilder(args);

                var dbConnectionString = builder.Configuration.GetConnectionString(nameof(ForumDbContext)) ??
                    throw new InvalidOperationException($"Connection string not found.");

                var csb = new NpgsqlConnectionStringBuilder(dbConnectionString)
                {
                    Password = builder.Configuration["POSTGRES_PASSWORD"] ??
                        throw new InvalidOperationException("POSTGRES_PASSWORD is not set"),
                    Username = builder.Configuration["POSTGRES_USER"] ??
                        throw new InvalidOperationException("POSTGRES_USER is not set")
                };

                var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ??
                    throw new InvalidOperationException($"Redis connection string not found.");

                var minioEndpoint = builder.Configuration["MinIO:InternalEndpoint"];
                var minioAccessKey = builder.Configuration["MINIO_ACCESS_KEY"];
                var minioSecretKey = builder.Configuration["MINIO_SECRET_KEY"];
                var minioWithSsl = builder.Configuration.GetValue<bool>("MinIO:WithSSL");

                builder.Host.UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services));

                builder.Services.AddControllers();
                builder.Services.AddAutoMapper(typeof(AppMappingProfile));
                builder.Services.AddValidatorsFromAssemblyContaining<CreatePostRequestValidator>();

                builder.Services.AddResponseCompression(opt =>
                {
                    opt.EnableForHttps = true;
                    opt.Providers.Add(new GzipCompressionProvider(new GzipCompressionProviderOptions()));
                });

                builder.Services.AddMinio(configureClient => configureClient
                    .WithEndpoint(minioEndpoint)
                    .WithCredentials(minioAccessKey, minioSecretKey)
                    .WithSSL(minioWithSsl)
                    .Build());

                builder.Services.AddDbContext<ForumDbContext>(options => options.UseNpgsql(csb.ConnectionString));

                builder.Services.AddHealthChecks()
                    .AddNpgSql(csb.ConnectionString)
                    .AddRedis(redisConnectionString)
                    .AddCheck<MinioHealthCheck>("MinIO");

                builder.Services.AddOpenTelemetry()
                    .WithMetrics(metrics =>
                    {
                        metrics.AddPrometheusExporter();

                        metrics.AddAspNetCoreInstrumentation();
                        metrics.AddHttpClientInstrumentation();
                        metrics.AddRuntimeInstrumentation();

                        metrics.AddMeter("MyForum.Metrics");
                    });

                builder.Services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnectionString;
                    options.InstanceName = "MyForum_";
                });

                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowFrontend", policy =>
                    {
                        policy.WithOrigins("http://localhost:5173")
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials();
                    });
                });

                builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
                builder.Services.AddScoped<IBoardService, BoardService>();
                builder.Services.AddScoped<IThreadService, ThreadService>();
                builder.Services.AddScoped<IPostService, PostService>();
                builder.Services.AddScoped<IIPHasher, SHA256IPHasher>();
                builder.Services.AddScoped<IObjectStorageService, MinioObjectStorageService>();
                builder.Services.AddScoped<IFileDtoFactory, FileDtoFactory>();
                builder.Services.AddScoped<IPostDtoFactory, PostDtoFactory>();
                builder.Services.AddScoped<IThreadDtoFactory, ThreadDtoFactory>();
                builder.Services.AddScoped<ICreatePostResponseFactory, CreatePostResponseFactory>();
                builder.Services.AddScoped<IPasswordHasher<StaffAccount>, PasswordHasher<StaffAccount>>();
                builder.Services.AddScoped<IStaffAuthService, StaffAuthService>();
                builder.Services.AddScoped<IAdminBoardService, AdminBoardService>();
                builder.Services.AddScoped<IAdminThreadService, AdminThreadService>();
                builder.Services.AddScoped<IAdminPostService, AdminPostService>();
                builder.Services.AddScoped<IBanService, BanService>();

                builder.Services.AddSingleton<IForumMetrics, ForumMetrics>();
                builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
                    ConnectionMultiplexer.Connect(redisConnectionString));

                builder.Services.AddMemoryCache();

                // Admin authentication using cookies
                builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(options =>
                    {
                        options.Cookie.Name = "myforum_admin";
                        options.Cookie.HttpOnly = true;
                        options.Cookie.SameSite = SameSiteMode.Strict;
                        options.ExpireTimeSpan = TimeSpan.FromHours(8);
                    });

                builder.Services.AddAuthorization(options =>
                {
                    options.AddPolicy("AdminOnly",
                        p => p.RequireRole("Admin"));

                    options.AddPolicy("Moderator",
                        p => p.RequireRole("Admin", "Moderator"));
                });

                var app = builder.Build();

                using (var scope = app.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    try
                    {
                        var context = services.GetRequiredService<ForumDbContext>();
                        var logger = services.GetRequiredService<ILogger<SeedData>>();
                        var ipHasher = services.GetRequiredService<IIPHasher>();
                        await SeedData.SeedAsync(context, logger, ipHasher);
                    }
                    catch (Exception ex)
                    {
                        var logger = services.GetRequiredService<ILogger<Program>>();
                        logger.LogError(ex, "Error migrating database");
                    }
                }

                app.UseHttpsRedirection();

                app.UseRouting();

                app.UseAuthentication();
                app.UseAuthorization();

                app.UseResponseCompression();
                app.UseCors("AllowFrontend");

                app.UseHealthChecksPrometheusExporter("/healthmetrics");

                app.MapHealthChecks("/health", new HealthCheckOptions
                {
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });

                app.MapPrometheusScrapingEndpoint();

                app.MapControllers();

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
