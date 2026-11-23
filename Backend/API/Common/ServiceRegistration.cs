using API.Features.Auth;
using API.Features.Courses;
using API.Features.Services;
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
            // Therapist↔Service links point at soft-deletable rows on both sides; the query filters
            // hiding a deleted service or therapist from the other's list is exactly what we want.
            .ConfigureWarnings(w => w.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning)));

        // Features (one line per feature)
        services.AddScoped<AuthService>();
        services.AddScoped<UserService>();
        services.AddScoped<TherapistService>();
        services.AddScoped<ClinicServicesService>();
        services.AddScoped<CourseService>();
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
