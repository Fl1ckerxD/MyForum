using FluentValidation;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Minio;
using Minio.AspNetCore;
using Minio.AspNetCore.HealthChecks;
using MyForum.Core.Interfaces.Repositories;
using MyForum.Core.Interfaces.Services;
using MyForum.Core.MappingProfiles;
using MyForum.Core.Validations;
using MyForum.Infrastructure.Data;
using MyForum.Infrastructure.Repositories;
using MyForum.Infrastructure.Services;
using Serilog;

namespace MyForum
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            try
            {
                var builder = WebApplication.CreateBuilder(args);

                builder.Host.UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .WriteTo.Console());

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
                builder.Services.AddMinio(options =>
                {
                    options.Endpoint = builder.Configuration["MinIO:Endpoint"] ?? "localhost:9000";
                    options.AccessKey = builder.Configuration["MinIO:AccessKey"] ?? "minioadmin";
                    options.SecretKey = builder.Configuration["MinIO:SecretKey"] ?? "minioadmin";
                    options.ConfigureClient(client => client.WithSSL(bool.Parse(builder.Configuration["MinIO:WithSSL"] ?? "false")));
                });

                var conString = builder.Configuration.GetConnectionString(nameof(ForumDbContext)) ??
                        throw new InvalidOperationException($"Connection string not found.");
                builder.Services.AddDbContext<ForumDbContext>(options => options.UseNpgsql(conString));

                builder.Services.AddHealthChecks()
                    .AddNpgSql(conString)
                    .AddMinio(sp => sp.GetRequiredService<IMinioClient>());

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
