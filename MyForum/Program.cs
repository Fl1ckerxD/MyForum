using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using MyForum.Core.Interfaces;
using MyForum.Infrastructure.Data;
using MyForum.Infrastructure.Repositories;
using MyForum.Infrastructure.Services;
using MyForum.Infrastructure.Services.CategoryServices;
using MyForum.Infrastructure.Services.PostServices;
using MyForum.Infrastructure.Services.TopicServices;
using MyForum.Infrastructure.Services.UserServices;
using System.Security.Claims;

namespace MyForum
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

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
                options.LoginPath = "/Users/Login"; // Страница входа
                options.AccessDeniedPath = "/Users/AccesDenied"; // Страница отказа в доступе
                options.ExpireTimeSpan = TimeSpan.FromDays(1); // Время жизни куки
            });

            builder.Services.AddMemoryCache();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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

            // Использование аутентификации и авторизации
            app.UseAuthentication();
            app.UseAuthorization();

            // Использование сжатия ответов
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
    }
}
