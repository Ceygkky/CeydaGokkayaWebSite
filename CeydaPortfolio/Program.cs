using CeydaPortfolio.Data;
using CeydaPortfolio.Models;
using Microsoft.EntityFrameworkCore;

namespace CeydaPortfolio
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();

            // SQL Server veritabaný baðlantýsý
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            var app = builder.Build();
            // Uygulama açýlýrken eksik veritabaný tablolarýný oluþturur
            using (var scope = app.Services.CreateScope())
            {
                var startupLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                try
                {
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    db.Database.Migrate();
                }
                catch (Exception ex)
                {
                    startupLogger.LogError(ex, "Veritabaný güncellenemedi, site veritabaný olmadan çalýþmaya devam edecek.");
                }
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            // Sayfa görüntülenme sayacý
            app.Use(async (context, next) =>
            {
                await next();

                var path = context.Request.Path.Value ?? "/";
                var userAgent = context.Request.Headers.UserAgent.ToString();

                var isPage = context.Request.Method == HttpMethods.Get
                    && context.Response.StatusCode == 200
                    && (context.Response.ContentType?.StartsWith("text/html") ?? false);

                var isBot = userAgent.Contains("bot", StringComparison.OrdinalIgnoreCase)
                    || userAgent.Contains("crawl", StringComparison.OrdinalIgnoreCase)
                    || userAgent.Contains("spider", StringComparison.OrdinalIgnoreCase);

                var isError = path.StartsWith("/Error", StringComparison.OrdinalIgnoreCase)
    ||          path.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase);    
                if (isPage && !isBot && !isError)
                {
                    var scopeFactory = context.RequestServices.GetRequiredService<IServiceScopeFactory>();
                    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                    var pageName = path.Length > 200 ? path[..200] : path;

                    // Arka planda kaydet, sayfa yavaþlamasýn
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            using var scope = scopeFactory.CreateScope();
                            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                            db.PageViews.Add(new PageView { PageName = pageName });
                            await db.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            logger.LogWarning(ex, "Sayfa görüntülenmesi kaydedilemedi.");
                        }
                    });
                }
            });

            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapRazorPages()
               .WithStaticAssets();

            app.Run();
        }
    }
}