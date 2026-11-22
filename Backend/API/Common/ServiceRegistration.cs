using API.Features.Auth;
using API.Features.Benefits;
using API.Features.Categories;
using API.Features.Courses;
using API.Features.Services;
using API.Features.Specialties;
using API.Features.Testimonials;
using API.Features.Therapists;
using API.Features.Users;
using Infrastructure.ConsultorioME;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace API.Common;

public static class ServiceRegistration
{
    public static IServiceCollection AddMerakiServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var connectionString = DatabaseUrl.ToConnectionString(configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured"));

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddDbContext<AppDbContext>((provider, options) => options
            .UseNpgsql(connectionString)
            .AddInterceptors(new AuditInterceptor(provider.GetRequiredService<ICurrentUser>()))
            // Service→Category is required while Category has a soft-delete filter; deleting a
            // category in use is blocked (409), so the filtered-required case cannot arise.
            .ConfigureWarnings(w => w.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning)));

        // Features (one line per feature)
        services.AddScoped<AuthService>();
        services.AddScoped<UserService>();
        services.AddScoped<TherapistService>();
        services.AddScoped<ClinicServicesService>();
        services.AddScoped<CourseService>();
        services.AddScoped<CategoryService>();
        services.AddScoped<BenefitService>();
        services.AddScoped<SpecialtyService>();
        services.AddScoped<TestimonialService>();

        services.AddFileStorage(configuration, environment);
        services.AddContactForm(configuration);

        // ConsultorioME (external scheduling platform)
        services.AddHttpClient("ConsultorioME", (provider, client) =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            client.BaseAddress = new Uri(config["ConsultorioME:BaseUrl"] ?? "https://api.consultoriome.com/v1/api");
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            client.Timeout = TimeSpan.FromSeconds(100);
        });
        services.AddScoped<IConsultorioMEService, ConsultorioMEService>();

        return services;
    }
}
