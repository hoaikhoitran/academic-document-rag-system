using AcademicDocumentRagSystem.Services;
using AcademicDocumentRagSystem.Services.Maintenance;

namespace AcademicDocumentRagSystem.MVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddSession();
            builder.Services.AddApplicationServices(builder.Configuration);

            var app = builder.Build();

            // One-time, idempotent migration: backfill real SHA-256 content hashes
            // for existing documents and create the duplicate-blocking unique index.
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var backfiller = services.GetRequiredService<DocumentFileHashBackfiller>();
                    backfiller.RunAsync().GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex,
                        "Document file-hash backfill failed during startup. The app will continue; " +
                        "new uploads are still protected by the service-level duplicate check.");
                }
            }

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
            app.UseSession();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
