using FluentValidation;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
// using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Minio;
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

                var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ??
                            throw new InvalidOperationException($"Redis connection string not found.");

                var minioEndpoint = builder.Configuration["MinIO:Endpoint"];
                var minioAccessKey = builder.Configuration["MinIO:AccessKey"];
                var minioSecretKey = builder.Configuration["MinIO:SecretKey"];
                var minioWithSsl = builder.Configuration.GetValue<bool>("MinIO:WithSSL");

                builder.Host.UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services));

                // builder.Services.AddControllersWithViews().AddRazorOptions(options =>
                // {
                //     options.ViewLocationFormats.Add("/Web/Views/{1}/{0}" + RazorViewEngine.ViewExtension);
                //     options.ViewLocationFormats.Add("/Web/Views/Shared/{0}" + RazorViewEngine.ViewExtension);
                // });

                // Add services to the container.
                // builder.Services.AddControllersWithViews();
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

                builder.Services.AddDbContext<ForumDbContext>(options => options.UseNpgsql(dbConnectionString));

                builder.Services.AddHealthChecks()
                    .AddNpgSql(dbConnectionString)
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
                builder.Services.AddScoped<IFileService, MinioFileService>();

                builder.Services.AddSingleton<IForumMetrics, ForumMetrics>();
                builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
                    ConnectionMultiplexer.Connect(redisConnectionString));

                // builder.Services.AddAuthentication("Cookies").AddCookie("Cookies", options =>
                // {
                //     options.Cookie.Name = "MyForum.Auth";
                //     options.LoginPath = "/Users/Login";
                //     options.AccessDeniedPath = "/Users/AccessDenied";
                //     options.ExpireTimeSpan = TimeSpan.FromDays(1);
                // });

                builder.Services.AddMemoryCache();

                var app = builder.Build();

                using (var scope = app.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    try
                    {
                        var context = services.GetRequiredService<ForumDbContext>();
                        var logger = services.GetRequiredService<ILogger<SeedData>>();
                        await SeedData.SeedAsync(context, logger);
                    }
                    catch (Exception ex)
                    {
                        var logger = services.GetRequiredService<ILogger<Program>>();
                        logger.LogError(ex, "Error migrating database");
                    }
                }

                // Configure the HTTP request pipeline.
                if (!app.Environment.IsDevelopment())
                {
                    // app.UseExceptionHandler("/Home/Error");
                    // app.UseHsts();
                }

                app.UseHttpsRedirection();
                // app.UseStaticFiles(new StaticFileOptions()
                // {
                //OnPrepareResponse = ctx =>
                //{
                //    ctx.Context.Response.Headers.Add("Cache-Control", "public,max-age=600");
                //}
                // });

                app.UseRouting();

                app.UseAuthentication();
                app.UseAuthorization();

                app.UseResponseCompression();
                app.UseCors("AllowFrontend");

                // app.UseStatusCodePages(async statusCodeContext =>
                // {
                //     var response = statusCodeContext.HttpContext.Response;
                //     response.ContentType = "text/html; charset=UTF-8";
                //     if (response.StatusCode == 404)
                //         await response.SendFileAsync("Views/Shared/NotFound.cshtml");
                // });

                app.UseHealthChecksPrometheusExporter("/healthmetrics");

                app.MapHealthChecks("/health", new HealthCheckOptions
                {
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });

                app.MapPrometheusScrapingEndpoint();

                app.MapControllers();

                // app.MapControllerRoute(
                //     name: "thread",
                //     pattern: "{boardShortName}/{threadId:int}",
                //     defaults: new { controller = "Threads", action = "Index" }
                //     );

                // app.MapControllerRoute(
                //     name: "board",
                //     pattern: "{boardShortName}",
                //     defaults: new { controller = "Boards", action = "Index" }
                //     );

                // app.MapControllerRoute(
                //     name: "default",
                //     pattern: "{controller=Home}/{action=Index}/{id?}");

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
