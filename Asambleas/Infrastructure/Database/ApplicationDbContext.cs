using Microsoft.EntityFrameworkCore;
using Asambleas.Features.Auth.Entities;
using Asambleas.Features.Docentes.Entities;
using Asambleas.Features.Asambleas.Entities;
using Asambleas.Features.Vacantes.Entities;
using Asambleas.Features.Designaciones.Entities;
using Asambleas.Features.Listados.Entities;
using Asambleas.Features.DeclaracionJurada.Entities;

namespace Asambleas.Infrastructure.Database;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // Auth
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // Docentes
    public DbSet<DocenteProfile> DocenteProfiles => Set<DocenteProfile>();

    // Asambleas
    public DbSet<Asamblea> Asambleas => Set<Asamblea>();
    public DbSet<VentanaHoraria> VentanasHorarias => Set<VentanaHoraria>();

    // Vacantes
    public DbSet<Vacante> Vacantes => Set<Vacante>();
    public DbSet<Establecimiento> Establecimientos => Set<Establecimiento>();

    // Designaciones
    public DbSet<Designacion> Designaciones => Set<Designacion>();
    public DbSet<Postulacion> Postulaciones => Set<Postulacion>();
    public DbSet<Impugnacion> Impugnaciones => Set<Impugnacion>();

    // Listados
    public DbSet<ListadoOficial> ListadosOficiales => Set<ListadoOficial>();
    public DbSet<PostulanteListado> PostulantesListado => Set<PostulanteListado>();
    public DbSet<AuditLogEntry> AuditLog => Set<AuditLogEntry>();

    // Declaración Jurada
    public DbSet<Features.DeclaracionJurada.Entities.DeclaracionJurada> DeclaracionesJuradas => Set<Features.DeclaracionJurada.Entities.DeclaracionJurada>();
    public DbSet<EmpleoActual> EmpleosActuales => Set<EmpleoActual>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── User ──
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasIndex(u => u.Cuil).IsUnique();
            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.Dni);
            e.Property(u => u.Role).HasConversion<string>().HasMaxLength(30);
            e.HasOne(u => u.DocenteProfile)
                .WithOne(d => d.User)
                .HasForeignKey<DocenteProfile>(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── RefreshToken ──
        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.ToTable("refresh_tokens");
            e.HasIndex(r => r.Token).IsUnique();
            e.HasOne(r => r.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── DocenteProfile ──
        modelBuilder.Entity<DocenteProfile>(e =>
        {
            e.ToTable("docente_profiles");
            e.Property(d => d.NivelEducativo).HasConversion<string>().HasMaxLength(30);
            e.Property(d => d.EstadoRevista).HasConversion<string>().HasMaxLength(30);
            e.Property(d => d.Puntaje).HasPrecision(10, 4);
        });

        // ── Asamblea ──
        modelBuilder.Entity<Asamblea>(e =>
        {
            e.ToTable("asambleas");
            e.Property(a => a.Nivel).HasConversion<string>().HasMaxLength(30);
            e.Property(a => a.Tipo).HasConversion<string>().HasMaxLength(30);
            e.Property(a => a.Estado).HasConversion<string>().HasMaxLength(30);
        });

        // ── VentanaHoraria ──
        modelBuilder.Entity<VentanaHoraria>(e =>
        {
            e.ToTable("ventanas_horarias");
            e.HasOne(v => v.Asamblea)
                .WithMany(a => a.VentanasHorarias)
                .HasForeignKey(v => v.AsambleaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Establecimiento ──
        modelBuilder.Entity<Establecimiento>(e =>
        {
            e.ToTable("establecimientos");
            e.HasIndex(est => est.CodigoFuncional).IsUnique();
        });

        // ── Vacante ──
        modelBuilder.Entity<Vacante>(e =>
        {
            e.ToTable("vacantes");
            e.Property(v => v.Nivel).HasConversion<string>().HasMaxLength(30);
            e.Property(v => v.TipoCargo).HasConversion<string>().HasMaxLength(30);
            e.Property(v => v.Estado).HasConversion<string>().HasMaxLength(30);
            e.Property(v => v.Turno).HasConversion<string>().HasMaxLength(30);
            e.HasOne(v => v.Establecimiento)
                .WithMany(est => est.Vacantes)
                .HasForeignKey(v => v.EstablecimientoId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(v => v.Asamblea)
                .WithMany()
                .HasForeignKey(v => v.AsambleaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Designacion ──
        modelBuilder.Entity<Designacion>(e =>
        {
            e.ToTable("designaciones");
            e.Property(d => d.Estado).HasConversion<string>().HasMaxLength(30);
            e.HasOne(d => d.Docente)
                .WithMany()
                .HasForeignKey(d => d.DocenteId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(d => d.Vacante)
                .WithMany()
                .HasForeignKey(d => d.VacanteId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(d => d.Asamblea)
                .WithMany()
                .HasForeignKey(d => d.AsambleaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Postulacion ──
        modelBuilder.Entity<Postulacion>(e =>
        {
            e.ToTable("postulaciones");
            e.Property(p => p.Puntaje).HasPrecision(10, 4);
            e.HasOne(p => p.Docente)
                .WithMany()
                .HasForeignKey(p => p.DocenteId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(p => p.Vacante)
                .WithMany()
                .HasForeignKey(p => p.VacanteId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(p => new { p.DocenteId, p.VacanteId }).IsUnique();
        });

        // ── Impugnacion ──
        modelBuilder.Entity<Impugnacion>(e =>
        {
            e.ToTable("impugnaciones");
            e.HasOne(i => i.Designacion)
                .WithMany(d => d.Impugnaciones)
                .HasForeignKey(i => i.DesignacionId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(i => i.Reclamante)
                .WithMany()
                .HasForeignKey(i => i.ReclamanteId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── ListadoOficial ──
        modelBuilder.Entity<ListadoOficial>(e =>
        {
            e.ToTable("listados_oficiales");
            e.Property(l => l.Nivel).HasConversion<string>().HasMaxLength(30);
            e.Property(l => l.Tipo).HasConversion<string>().HasMaxLength(30);
        });

        // ── PostulanteListado ──
        modelBuilder.Entity<PostulanteListado>(e =>
        {
            e.ToTable("postulantes_listado");
            e.Property(p => p.Puntaje).HasPrecision(10, 4);
            e.Property(p => p.Estado).HasConversion<string>().HasMaxLength(30);
            e.HasOne(p => p.Docente)
                .WithMany()
                .HasForeignKey(p => p.DocenteId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(p => p.ListadoOficial)
                .WithMany(l => l.Postulantes)
                .HasForeignKey(p => p.ListadoOficialId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── AuditLogEntry ──
        modelBuilder.Entity<AuditLogEntry>(e =>
        {
            e.ToTable("audit_log");
            e.Property(a => a.Accion).HasConversion<string>().HasMaxLength(50);
            e.HasIndex(a => a.Timestamp);
            e.HasIndex(a => a.UserId);
            e.HasIndex(a => a.Entidad);
        });

        // ── DeclaracionJurada ──
        modelBuilder.Entity<Features.DeclaracionJurada.Entities.DeclaracionJurada>(e =>
        {
            e.ToTable("declaraciones_juradas");
            e.Property(d => d.PasoActual).HasConversion<string>().HasMaxLength(30);
            e.HasOne(d => d.Docente)
                .WithMany()
                .HasForeignKey(d => d.DocenteId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── EmpleoActual ──
        modelBuilder.Entity<EmpleoActual>(e =>
        {
            e.ToTable("empleos_actuales");
            e.Property(emp => emp.SituacionRevista).HasConversion<string>().HasMaxLength(30);
            e.HasOne(emp => emp.DeclaracionJurada)
                .WithMany(d => d.EmpleosActuales)
                .HasForeignKey(emp => emp.DeclaracionJuradaId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<Common.Entities.BaseEntity>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
