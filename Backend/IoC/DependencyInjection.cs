using Application.DTOs;
using Application.Interfaces;
using Application.Mapping;
using Application.Services;
using Application.Validators;
using Azure.Storage.Blobs;
using Domain.Interfaces;
using FluentValidation;
using Infrastructure.Data;
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

            // Validators
            services.AddScoped<IValidator<ServiceDto>, ServiceValidator>();
            services.AddScoped<IValidator<TherapistDto>, TherapistValidator>();
            services.AddScoped<IValidator<CourseDto>, CourseValidator>();
            services.AddScoped<IValidator<CategoryDto>, CategoryValidator>();
            services.AddScoped<IValidator<BenefitDto>, BenefitValidator>();
            services.AddScoped<IValidator<SpecialtyDto>, SpecialtyValidator>();
            services.AddScoped<IValidator<TestimonialDto>, TestimonialValidator>();

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

            return services;
        }
    }

}
