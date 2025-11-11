using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using MyForum.Core.Interfaces;
using MyForum.Infrastructure.Data;
using MyForum.Infrastructure.Repositories;
using MyForum.Infrastructure.Services.PostServices;
using MyForum.Infrastructure.Services.UserServices;
using Serilog;

namespace MyForum
{
    public class Program
    {
        public static void Main(string[] args)
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
                builder.Services.AddResponseCompression(opt =>
                {
                    opt.EnableForHttps = true;
                    opt.Providers.Add(new GzipCompressionProvider(new GzipCompressionProviderOptions()));
                });

                var conString = builder.Configuration.GetConnectionString("ForumDatabase") ??
                        throw new InvalidOperationException("Connection string 'ForumDatabase' not found.");
                builder.Services.AddDbContext<ForumContext>(options => options.UseMySQL(conString));

                builder.Services.AddScoped<IUserService, UserService>();
                builder.Services.AddScoped<IPostService, PostService>();
                builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
                builder.Services.AddScoped<ILikeRepository, LikeRepository>();
                builder.Services.AddScoped<IPostRepository, PostRepository>();
                builder.Services.AddScoped<ITopicRepository, TopicRepository>();
                builder.Services.AddScoped<IUserRepository, UserRepository>();
                builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

                builder.Services.AddAuthentication("Cookies").AddCookie("Cookies", options =>
                {
                    options.Cookie.Name = "MyForum.Auth";
                    options.LoginPath = "/Users/Login";
                    options.AccessDeniedPath = "/Users/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromDays(1);
                });

                builder.Services.AddMemoryCache();

                var app = builder.Build();

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

                app.MapControllerRoute(
                    name: "topic",
                    pattern: "{categoryName}/{topicId:int}",
                    defaults: new { controller = "Topics", action = "Index" }
                    );

                app.MapControllerRoute(
                    name: "category",
                    pattern: "{categoryName}",
                    defaults: new { controller = "Categories", action = "Index" }
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
