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
                options.LoginPath = "/Users/Login"; // �������� �����
                options.AccessDeniedPath = "/Users/AccesDenied"; // �������� ������ � �������
                options.ExpireTimeSpan = TimeSpan.FromDays(1); // ����� ����� ����
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

            // ������������� �������������� � �����������
            app.UseAuthentication();
            app.UseAuthorization();

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
