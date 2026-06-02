using AcademicDocumentRagSystem.DataAccess.Models;
using AcademicDocumentRagSystem.DataAccess.Repositories.Implementations;
using AcademicDocumentRagSystem.DataAccess.Repositories.Interfaces;
using AcademicDocumentRagSystem.Services.Chunking;
using AcademicDocumentRagSystem.Services.Implementations;
using AcademicDocumentRagSystem.Services.Interfaces;
using AcademicDocumentRagSystem.Services.RagIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AcademicDocumentRagSystem.Services
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<AcademicRagDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddHttpClient<IRagClient, RagApiClient>(client =>
            {
                var baseUrl = configuration["RagService:BaseUrl"];

                client.BaseAddress = new Uri(baseUrl!);
                client.Timeout = TimeSpan.FromSeconds(180);
            });

            services.AddScoped<ICourseRepository, CourseRepository>();
            services.AddScoped<ICourseService, CourseService>();

            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IAccountService, AccountService>();

            services.AddScoped<IDocumentRepository, DocumentRepository>();
            services.AddScoped<IDocumentChunkRepository, DocumentChunkRepository>();
            services.AddScoped<IDocumentIndexLogRepository, DocumentIndexLogRepository>();
            services.AddScoped<IChunkPreviewGenerator, ChunkPreviewGenerator>();
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<IChatRepository, ChatRepository>();
            services.AddScoped<IChatService, ChatService>();

            return services;
        }
    }
}