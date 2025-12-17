using FluentValidation;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Minio;
using MyForum.Core.Interfaces.Repositories;
using MyForum.Core.Interfaces.Services;
using MyForum.Core.MappingProfiles;
using MyForum.Core.Validations;
using MyForum.Infrastructure.Data;
using MyForum.Infrastructure.HealthChecks;
using MyForum.Infrastructure.Repositories;
using MyForum.Infrastructure.Services;
using OpenTelemetry.Metrics;
using Serilog;

namespace MyForum
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                var builder = WebApplication.CreateBuilder(args);

                var conString = builder.Configuration.GetConnectionString(nameof(ForumDbContext)) ??
                            throw new InvalidOperationException($"Connection string not found.");

                var minioEndpoint = builder.Configuration["MinIO:Endpoint"];
                var minioAccessKey = builder.Configuration["MinIO:AccessKey"];
                var minioSecretKey = builder.Configuration["MinIO:SecretKey"];
                var minioWithSsl = builder.Configuration.GetValue<bool>("MinIO:WithSSL");

                builder.Host.UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services));

                builder.Services.AddControllersWithViews().AddRazorOptions(options =>
                {
                    options.ViewLocationFormats.Add("/Web/Views/{1}/{0}" + RazorViewEngine.ViewExtension);
                    options.ViewLocationFormats.Add("/Web/Views/Shared/{0}" + RazorViewEngine.ViewExtension);
                });

                // Add services to the container.
                builder.Services.AddControllersWithViews();
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

                builder.Services.AddDbContext<ForumDbContext>(options => options.UseNpgsql(conString));

                builder.Services.AddHealthChecks()
                    .AddNpgSql(conString)
                    .AddCheck<MinioHealthCheck>("MinIO_Health_Check");

                builder.Services.AddOpenTelemetry()
                    .WithMetrics(metrics =>
                    {
                        metrics.AddPrometheusExporter();

                        // Встроенные метрики ASP.NET Core
                        metrics.AddAspNetCoreInstrumentation();
                        metrics.AddHttpClientInstrumentation();
                        metrics.AddRuntimeInstrumentation();

                        // Собственные метрики для форума
                        metrics.AddMeter("MyForum.Metrics");
                    });

                builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
                builder.Services.AddScoped<IBoardService, BoardService>();
                builder.Services.AddScoped<IThreadService, ThreadService>();
                builder.Services.AddScoped<IPostService, PostService>();
                builder.Services.AddScoped<IIPHasher, SHA256IPHasher>();
                builder.Services.AddScoped<IFileService, MinioFileService>();

                builder.Services.AddAuthentication("Cookies").AddCookie("Cookies", options =>
                {
                    options.Cookie.Name = "MyForum.Auth";
                    options.LoginPath = "/Users/Login";
                    options.AccessDeniedPath = "/Users/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromDays(1);
                });

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
                    app.UseExceptionHandler("/Home/Error");
                    app.UseHsts();
                }

                app.UseHttpsRedirection();
                app.UseStaticFiles(new StaticFileOptions()
                {
                    //OnPrepareResponse = ctx =>
                    //{
                    //    ctx.Context.Response.Headers.Add("Cache-Control", "public,max-age=600");
                    //}
                });

                app.UseRouting();

                app.UseAuthentication();
                app.UseAuthorization();

                app.UseResponseCompression();

                app.UseStatusCodePages(async statusCodeContext =>
                {
                    var response = statusCodeContext.HttpContext.Response;
                    response.ContentType = "text/html; charset=UTF-8";
                    if (response.StatusCode == 404)
                        await response.SendFileAsync("Views/Shared/NotFound.cshtml");
                });

                app.MapHealthChecks("/health", new HealthCheckOptions
                {
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });

                app.UseOpenTelemetryPrometheusScrapingEndpoint();

                app.MapControllerRoute(
                    name: "thread",
                    pattern: "{boardShortName}/{threadId:int}",
                    defaults: new { controller = "Threads", action = "Index" }
                    );

                app.MapControllerRoute(
                    name: "board",
                    pattern: "{boardShortName}",
                    defaults: new { controller = "Boards", action = "Index" }
                    );

                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

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
