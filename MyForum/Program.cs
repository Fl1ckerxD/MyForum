using Microsoft.EntityFrameworkCore;
using MyForum.Models;
using MyForum.Services;
using MySql.Data.MySqlClient;

namespace MyForum
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var conString = builder.Configuration.GetConnectionString("ForumDatabase") ??
                    throw new InvalidOperationException("Connection string 'ForumDatabase' not found.");
            builder.Services.AddDbContext<ForumContext>(options => options.UseMySQL(conString));

            builder.Services.AddScoped<UserService>();

            builder.Services.AddAuthentication("CookieAuth").AddCookie("CookieAuth", options =>
            {
                options.Cookie.Name = "MyForum.Auth";
                options.LoginPath = "/Users/Login"; // Страница входа
                options.AccessDeniedPath = "/Users/AccesDenied"; // Страница отказа в доступе
                options.ExpireTimeSpan = TimeSpan.FromDays(1); // Время жизни куки
            });

            builder.Services.AddAuthentication();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Использование аутентификации и авторизации
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "category",
                pattern: "{categoryName}",
                defaults: new { controller = "Categories", action = "Index" }
                );

            app.MapControllerRoute(
                name: "topic",
                pattern: "{categoryName}/{topicId:int}",
                defaults: new { controller = "Topics", action = "Index" }
                );

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
