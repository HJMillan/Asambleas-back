using Asambleas.Features.Auth.Entities;
using Asambleas.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Asambleas.Infrastructure.Seeding;

/// <summary>
/// Seed data para desarrollo. Crea un administrador inicial si no existe.
/// </summary>
public static class DbSeeder
{
    /// <summary>
    /// Crea el usuario administrador por defecto si no existe.
    /// Solo debe ejecutarse en Development.
    /// Lee la contraseña desde SeedSettings:AdminPassword en configuración.
    /// Si no está configurada, genera una aleatoria y la loguea.
    /// </summary>
    public static async Task SeedAdminAsync(ApplicationDbContext db, ILogger logger, IConfiguration configuration)
    {
        const string adminCuil = "20000000001";

        if (await db.Users.AnyAsync(u => u.Cuil == adminCuil))
        {
            logger.LogDebug("Admin seed: usuario administrador ya existe, omitiendo");
            return;
        }

        // Leer password desde config o generar una aleatoria segura
        var password = configuration["SeedSettings:AdminPassword"];
        if (string.IsNullOrEmpty(password))
        {
            password = Convert.ToBase64String(
                System.Security.Cryptography.RandomNumberGenerator.GetBytes(24));
            logger.LogWarning(
                "Admin seed: no se configuró SeedSettings:AdminPassword. " +
                "Se generó una contraseña aleatoria: {Password}. " +
                "Guardar esta contraseña de forma segura.",
                password);
        }

        var admin = new User
        {
            Cuil = adminCuil,
            Dni = "00000000",
            Nombre = "Admin",
            Apellido = "Sistema",
            Email = "admin@asambleas.edu.ar",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = Role.ADMIN_SISTEMA,
            IsDomelecVerified = true
        };

        db.Users.Add(admin);
        await db.SaveChangesAsync();

        logger.LogInformation(
            "Admin seed: usuario administrador creado. CUIL={Cuil}, Email={Email}.",
            adminCuil, admin.Email);
    }
}
