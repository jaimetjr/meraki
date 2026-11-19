using Application.Interfaces;
using Application.Mapping;
using Application.Services;
using Azure.Storage.Blobs;
using Domain.Interfaces;
using Infrastructure.Data;
using Infrastructure.HttpClients;
using Infrastructure.Repositories;
using Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IoC
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDependencyInjection(this IServiceCollection services, string connectionString)
        {
            // Register CurrentUserService and AuditService first (needed by interceptor)
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IAuditService, AuditService>();

            // Register DbContext with interceptor
            services.AddDbContext<AppDbContext>((serviceProvider, options) =>
            {
                options.UseNpgsql(connectionString);
                var currentUserService = serviceProvider.GetRequiredService<ICurrentUserService>();
                options.AddInterceptors(new AuditInterceptor(currentUserService));
            });

            // Register Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register generic repository (optional if used directly)
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // Register domain-specific repositories
            services.AddScoped<IServiceRepository, ServiceRepository>();
            services.AddScoped<ITherapistRepository, TherapistRepository>();
            services.AddScoped<ICourseRepository, CourseRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IBenefitRepository, BenefitRepository>();
            services.AddScoped<ISpecialtyRepository, SpecialtyRepository>();
            services.AddScoped<ITestimonialRepository, TestimonialRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();

            // Automapper configuration
            services.AddAutoMapper(c => c.AddProfile<AutoMapperProfile>());

            // Services layer registrations 
            services.AddScoped<IServiceManager, ServiceManager>();
            services.AddScoped<ITherapistService, TherapistService>();
            services.AddScoped<ICourseService, CourseService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IBenefitService, BenefitService>();
            services.AddScoped<ISpecialtyService, SpecialtyService>();
            services.AddScoped<ITestimonialService, TestimonialService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();


            // Azure
            services.AddScoped<IFileStorageService, AzureBlobStorageService>();
            services.AddSingleton(serviceProvider =>
            {
                var config = serviceProvider.GetRequiredService<IConfiguration>();
                var connectionString = config["AzureBlobStorage:ConnectionString"];
                var containerName = config["AzureBlobStorage:ContainerName"];

                var serviceClient = new BlobServiceClient(connectionString);
                return serviceClient.GetBlobContainerClient(containerName);
            });

            // ConsultorioME API
            services.AddHttpClient("ConsultorioME", (serviceProvider, client) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var baseUrl = configuration["ConsultorioME:BaseUrl"] 
                    ?? "https://api.consultoriome.com/v1/api";
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.Timeout = TimeSpan.FromSeconds(100);
            });

            services.AddScoped<IConsultorioMEService, ConsultorioMEService>();

            return services;
        }
    }

}
