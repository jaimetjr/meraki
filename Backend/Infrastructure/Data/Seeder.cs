using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data;

/// <summary>Seed:SuperAdmin configuration (keep it in user-secrets / environment, never in a committed file).</summary>
public sealed class SuperAdminSeed
{
    public string? Email { get; set; }
    public string? Password { get; set; }
}

/// <summary>
/// Startup seeding. Always: make sure an active SuperAdmin exists (from configuration), which also
/// recovers from a lockout. Development only: demo content so the site is not empty. Idempotent.
/// </summary>
public static class Seeder
{
    public static async Task SeedAsync(AppDbContext db, SuperAdminSeed superAdmin, bool includeDemoContent, ILogger logger)
    {
        await EnsureSuperAdminAsync(db, superAdmin, logger);
        if (includeDemoContent) await SeedDemoContentAsync(db);
    }

    private static async Task EnsureSuperAdminAsync(AppDbContext db, SuperAdminSeed seed, ILogger logger)
    {
        if (await db.Users.AnyAsync(u => u.Role == UserRole.SuperAdmin && u.IsActive)) return;

        if (string.IsNullOrWhiteSpace(seed.Email) || string.IsNullOrWhiteSpace(seed.Password))
        {
            logger.LogWarning("There is no active SuperAdmin and Seed:SuperAdmin:Email/Password are not configured; " +
                              "nobody can manage users until one is configured.");
            return;
        }

        var email = seed.Email.ToLowerInvariant();
        var existing = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (existing != null)
        {
            // Recover the configured account without touching its password.
            existing.UpdateRole(UserRole.SuperAdmin);
            existing.Activate();
            logger.LogWarning("No active SuperAdmin found; promoted and reactivated {Email} from configuration.", email);
        }
        else
        {
            db.Users.Add(new User(Guid.NewGuid(), email, BCrypt.Net.BCrypt.HashPassword(seed.Password), UserRole.SuperAdmin));
            logger.LogInformation("Created SuperAdmin {Email} from configuration.", email);
        }
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Creates a SuperAdmin with this e-mail or, if the e-mail already exists, sets its password and makes it an
    /// active SuperAdmin. Used by the <c>--create-superadmin</c> command: the first admin in production and the
    /// way back in after a forgotten password. Returns true when the account was created.
    /// </summary>
    public static async Task<bool> SetSuperAdminAsync(AppDbContext db, string email, string firstName, string password)
    {
        email = email.Trim().ToLowerInvariant();
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
        var created = user is null;
        if (user is null)
        {
            user = new User(Guid.NewGuid(), email, passwordHash, UserRole.SuperAdmin);
            db.Users.Add(user);
        }
        else
        {
            user.UpdatePassword(passwordHash);
            user.UpdateRole(UserRole.SuperAdmin);
            user.Activate();
        }
        user.UpdateProfile(firstName.Trim(), user.LastName);
        await db.SaveChangesAsync();
        return created;
    }

    private static async Task SeedDemoContentAsync(AppDbContext db)
    {
        // Services first: therapists are linked to the services they offer.
        if (!await db.Services.AnyAsync())
        {
            var reiki = new Service("Reiki", "Terapia energética pela imposição das mãos", null, new Money(120, "BRL"));
            reiki.UpdateDetails("Sessão de Reiki para equilíbrio energético e relaxamento profundo", "60 min");
            reiki.SetBenefits(["Relaxamento", "Alívio do estresse", "Equilíbrio energético"]);

            // No price: the site shows it as "sob consulta".
            var constellation = new Service("Constelação Familiar", "Abordagem sistêmica para questões familiares", null, null);
            constellation.UpdateDetails("Atendimento individual ou em grupo com a abordagem sistêmica", "2 horas");
            constellation.SetBenefits(["Compreensão das dinâmicas familiares", "Reconciliação com a própria história"]);

            db.Services.AddRange(reiki, constellation);
            await db.SaveChangesAsync();
        }

        if (!await db.Therapists.AnyAsync())
        {
            var services = await db.Services.ToListAsync();
            Service Named(string name) => services.First(s => s.Name == name);

            var ana = new Therapist("Dra. Ana Souza", "Terapeuta sistêmica com mais de 10 anos de experiência", null,
                "10 anos conduzindo constelações familiares", "Formação em Constelação Familiar");
            ana.SetServices([Named("Constelação Familiar")]);

            var carlos = new Therapist("Carlos Lima", "Reikiano e instrutor de meditação", null,
                "8 anos atendendo com Reiki", "Mestre em Reiki Usui");
            carlos.SetServices([Named("Reiki")]);

            db.Therapists.AddRange(ana, carlos);
            await db.SaveChangesAsync();
        }

        if (!await db.Courses.AnyAsync())
        {
            var mindfulness = new Course("Fundamentos do Mindfulness", "Aprenda os princípios básicos da atenção plena", null,
                "Carlos Lima", CourseType.Curso, CourseStatus.Disponivel);
            mindfulness.Schedule(DateTime.UtcNow.AddDays(10), null, Modality.Online, new Money(150, "BRL"), "https://exemplo.com/mindfulness");

            var trauma = new Course("Superando o Trauma", "Curso guiado para recuperação emocional", null,
                "Dra. Ana Souza", CourseType.Curso, CourseStatus.EmBreve);
            trauma.Schedule(DateTime.UtcNow.AddDays(20), null, Modality.Presencial, new Money(300, "BRL"), null);

            db.Courses.AddRange(mindfulness, trauma);
            await db.SaveChangesAsync();
        }

        if (!await db.Testimonials.AnyAsync())
        {
            db.Testimonials.AddRange(
                new Testimonial("Juliana Costa", null, 5, "Esse serviço transformou minha vida!", "Verificado"),
                new Testimonial("Marcelo Tavares", null, 4, "Muito profissional e acolhedor."),
                new Testimonial("Fernanda Rocha", null, 5, "Recomendo fortemente para quem busca apoio emocional.", "Top Reviewer"));
            await db.SaveChangesAsync();
        }
    }
}
