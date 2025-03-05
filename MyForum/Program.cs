using Microsoft.EntityFrameworkCore;
using MyForum.Models;
using MyForum.Services;

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
