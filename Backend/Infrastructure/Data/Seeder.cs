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
        if (!await db.Categories.AnyAsync())
        {
            db.Categories.AddRange(new Category("Terapia"), new Category("Mindfulness"), new Category("Cursos"));
            await db.SaveChangesAsync();
        }

        if (!await db.Benefits.AnyAsync())
        {
            db.Benefits.AddRange(
                new Benefit("Alívio do Estresse", "Ajuda a reduzir o estresse e a ansiedade"),
                new Benefit("Foco Aprimorado", "Melhora a concentração e a clareza mental"),
                new Benefit("Equilíbrio Emocional", "Auxilia na regulação das emoções"),
                new Benefit("Melhora do Sono", "Promove um sono mais tranquilo"),
                new Benefit("Autoconhecimento", "Estimula a reflexão e o crescimento pessoal"));
            await db.SaveChangesAsync();
        }

        if (!await db.Specialties.AnyAsync())
        {
            db.Specialties.AddRange(
                new Specialty("Ansiedade", "Apoio para lidar com crises de ansiedade"),
                new Specialty("Depressão", "Ajuda no tratamento de sintomas depressivos"),
                new Specialty("Autoestima", "Fortalecimento da confiança e valorização pessoal"),
                new Specialty("Trauma", "Processo terapêutico para experiências traumáticas"));
            await db.SaveChangesAsync();
        }

        if (!await db.Therapists.AnyAsync())
        {
            var specialties = await db.Specialties.OrderBy(s => s.Name).ToListAsync();
            Specialty Named(string name) => specialties.First(s => s.Name == name);

            var ana = new Therapist("Dra. Ana Souza", "Psicóloga clínica com mais de 10 anos de experiência", null,
                "10 anos atuando com TCC e trauma", "Doutorado em Psicologia");
            ana.SetSpecialties([Named("Ansiedade"), Named("Trauma")]);

            var carlos = new Therapist("Carlos Lima", "Instrutor de mindfulness e meditação", null,
                "8 anos ensinando práticas de atenção plena", "Certificação Internacional em Mindfulness");
            carlos.SetSpecialties([Named("Autoestima")]);

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

        if (!await db.Services.AnyAsync())
        {
            var therapy = await db.Categories.FirstAsync(c => c.Name == "Terapia");
            var benefits = await db.Benefits.OrderBy(b => b.Title).Take(3).ToListAsync();

            var breathing = new Service("Respiração Consciente", "Técnica de relaxamento com foco na respiração", null, new Money(120, "BRL"));
            breathing.UpdateDetails("Exercício respiratório para aliviar ansiedade", "30 minutos");
            breathing.UpdateCategory(therapy);
            breathing.SetBenefits(benefits);

            var cbt = new Service("Sessão de Terapia Cognitiva", "Atendimento estruturado para reprogramação de pensamentos", null, new Money(200, "BRL"));
            cbt.UpdateDetails("Sessão de TCC com terapeuta especializado", "1 hora");
            cbt.UpdateCategory(therapy);
            cbt.SetBenefits(benefits.Skip(1));

            db.Services.AddRange(breathing, cbt);
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
