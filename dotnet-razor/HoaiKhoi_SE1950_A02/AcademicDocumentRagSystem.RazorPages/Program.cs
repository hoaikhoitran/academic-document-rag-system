using AcademicDocumentRagSystem.RazorPages.Hubs;
using AcademicDocumentRagSystem.Services;
using AcademicDocumentRagSystem.Services.Maintenance;

namespace AcademicDocumentRagSystem.RazorPages
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Presentation: Razor Pages (not MVC controllers/views). Login is the
            // default start page by mapping the empty route ("/") to it.
            builder.Services
                .AddRazorPages(options =>
                {
                    options.Conventions.AddPageRoute("/Auth/Login", "");
                });

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(2);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // SignalR powers the real-time Course list broadcast to lecturers.
            builder.Services.AddSignalR();

            // Reuse the exact same service/repository/DbContext registrations as the
            // MVC app so business logic and data access are shared, not duplicated.
            builder.Services.AddApplicationServices(builder.Configuration);

            var app = builder.Build();

            // Same idempotent startup migration the MVC app runs: backfill SHA-256
            // content hashes and create the duplicate-blocking unique index. Safe to
            // run again because both apps share the same database.
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
                        "Document file-hash backfill failed during startup. The app will continue.");
                }
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseSession();
            app.UseAuthorization();

            app.MapRazorPages();
            app.MapHub<CourseHub>("/hubs/courses");

            app.Run();
        }
    }
}
