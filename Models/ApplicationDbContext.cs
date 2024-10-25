using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace controlAcademico_web_api.Models;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<alumno> alumno { get; set; }

    public virtual DbSet<maestro> maestro { get; set; }

    public virtual DbSet<rol> rol { get; set; }

    public virtual DbSet<usuario> usuario { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<alumno>(entity =>
        {
            entity.HasKey(e => e.codigoAlumno).HasName("pk_alumno_codigoAlumno");

            entity.Property(e => e.grado).HasMaxLength(128);
            entity.Property(e => e.nombreAlumno).HasMaxLength(128);

            entity.HasOne(d => d.codigoUsuarioNavigation).WithMany(p => p.alumno)
                .HasForeignKey(d => d.codigoUsuario)
                .HasConstraintName("fk_alumno_codigoUsuario");
        });

        modelBuilder.Entity<maestro>(entity =>
        {
            entity.HasKey(e => e.codigoMaestro).HasName("pk_maestro_codigoMaestro");

            entity.Property(e => e.asignatura).HasMaxLength(128);
            entity.Property(e => e.nombreMaestro).HasMaxLength(128);

            entity.HasOne(d => d.codigoUsuarioNavigation).WithMany(p => p.maestro)
                .HasForeignKey(d => d.codigoUsuario)
                .HasConstraintName("fk_maestro_codigoUsuario");
        });

        modelBuilder.Entity<rol>(entity =>
        {
            entity.HasKey(e => e.codigoRol).HasName("pk_rol_codigoRol");

            entity.HasIndex(e => e.nombreRol, "uq_rol_nombreRol").IsUnique();

            entity.Property(e => e.nombreRol).HasMaxLength(128);
        });

        modelBuilder.Entity<usuario>(entity =>
        {
            entity.HasKey(e => e.codigoUsuario).HasName("pk_usuario_codigoUsuario");

            entity.HasIndex(e => e.clave, "uq_usuario_clave").IsUnique();

            entity.HasIndex(e => e.correo, "uq_usuario_correo").IsUnique();

            entity.Property(e => e.clave).HasMaxLength(255);
            entity.Property(e => e.correo).HasMaxLength(128);
            entity.Property(e => e.fechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.codigoRolNavigation).WithMany(p => p.usuario)
                .HasForeignKey(d => d.codigoRol)
                .HasConstraintName("fk_usuario_codigoRol");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
