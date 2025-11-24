using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ProyectoRuben.Backen.Modelo;

public partial class GestioninventarioyserviciosContext : DbContext
{
    public GestioninventarioyserviciosContext()
    {
    }

    public GestioninventarioyserviciosContext(DbContextOptions<GestioninventarioyserviciosContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<Configuracion> Configuracions { get; set; }

    public virtual DbSet<Factura> Facturas { get; set; }

    public virtual DbSet<Horario> Horarios { get; set; }

    public virtual DbSet<Oferta> Ofertas { get; set; }

    public virtual DbSet<Permiso> Permisos { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<Reserva> Reservas { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Servicio> Servicios { get; set; }

    public virtual DbSet<ServicioProducto> ServicioProductos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<VistaAgendaDiarium> VistaAgendaDiaria { get; set; }

    public virtual DbSet<VistaClientesVip> VistaClientesVips { get; set; }

    public virtual DbSet<VistaDashboardFinanciero> VistaDashboardFinancieros { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySQL("server=127.0.0.1;port=3306;database=gestioninventarioyservicios;user=root;password=mysql");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Activo).HasDefaultValueSql("'1'");
            entity.Property(e => e.Contacto).HasComment("Email o teléfono");
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<Configuracion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.FechaModificacion)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Tipo).HasDefaultValueSql("'String'");
        });

        modelBuilder.Entity<Factura>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Descuento).HasDefaultValueSql("'0.00'");
            entity.Property(e => e.Estado).HasDefaultValueSql("'Pendiente'");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ReservaId).HasComment("Opcional: vincula con reserva");

            entity.HasOne(d => d.Cliente).WithMany(p => p.Facturas).HasConstraintName("fk_FACTURA_CLIENTES");

            entity.HasOne(d => d.Reserva).WithMany(p => p.Facturas)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_FACTURA_RESERVAS");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Facturas).HasConstraintName("fk_FACTURA_USUARIOS");
        });

        modelBuilder.Entity<Horario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Reserva).WithMany(p => p.Horarios).HasConstraintName("fk_HORARIOS_RESERVAS");
        });

        modelBuilder.Entity<Oferta>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Activa).HasDefaultValueSql("'1'");
        });

        modelBuilder.Entity<Permiso>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Activo).HasDefaultValueSql("'1'");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.StockMaximo).HasDefaultValueSql("'100'");
            entity.Property(e => e.StockMinimo).HasDefaultValueSql("'10'");
        });

        modelBuilder.Entity<Reserva>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Estado).HasDefaultValueSql("'Pendiente'");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.FechaModificacion)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Cliente).WithMany(p => p.Reservas).HasConstraintName("fk_RESERVAS_CLIENTES");

            entity.HasOne(d => d.Empleado).WithMany(p => p.Reservas).HasConstraintName("fk_RESERVAS_EMPLEADOS");

            entity.HasOne(d => d.Servicio).WithMany(p => p.Reservas).HasConstraintName("fk_RESERVAS_SERVICIOS");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasMany(d => d.Permisos).WithMany(p => p.Roles)
                .UsingEntity<Dictionary<string, object>>(
                    "RolesPermiso",
                    r => r.HasOne<Permiso>().WithMany()
                        .HasForeignKey("PermisosId")
                        .HasConstraintName("fk_ROLES_PERMISOS_PERMISOS"),
                    l => l.HasOne<Role>().WithMany()
                        .HasForeignKey("RolesId")
                        .HasConstraintName("fk_ROLES_PERMISOS_ROLES"),
                    j =>
                    {
                        j.HasKey("RolesId", "PermisosId").HasName("PRIMARY");
                        j.ToTable("roles_permisos");
                        j.HasIndex(new[] { "PermisosId" }, "fk_ROLES_PERMISOS_PERMISOS_idx");
                        j.IndexerProperty<int>("RolesId").HasColumnName("ROLES_ID");
                        j.IndexerProperty<int>("PermisosId").HasColumnName("PERMISOS_ID");
                    });
        });

        modelBuilder.Entity<Servicio>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Activo).HasDefaultValueSql("'1'");
            entity.Property(e => e.Duracion).HasComment("Duración en minutos");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<ServicioProducto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Cantidad)
                .HasDefaultValueSql("'1'")
                .HasComment("Cantidad de producto usada");

            entity.HasOne(d => d.Producto).WithMany(p => p.ServicioProductos).HasConstraintName("fk_SERVICIO_PRODUCTO_PRODUCTOS");

            entity.HasOne(d => d.Servicio).WithMany(p => p.ServicioProductos).HasConstraintName("fk_SERVICIO_PRODUCTO_SERVICIOS");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Activo).HasDefaultValueSql("'1'");
            entity.Property(e => e.Contrasena).HasComment("Debe guardarse hasheada con BCrypt");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Rol).HasDefaultValueSql("'Empleado'");

            entity.HasOne(d => d.RolNavigation).WithMany(p => p.Usuarios)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_USUARIOS_ROLES");
        });

        modelBuilder.Entity<VistaAgendaDiarium>(entity =>
        {
            entity.ToView("vista_agenda_diaria");

            entity.Property(e => e.Duracion).HasComment("Duración en minutos");
            entity.Property(e => e.Estado).HasDefaultValueSql("'Pendiente'");
        });

        modelBuilder.Entity<VistaClientesVip>(entity =>
        {
            entity.ToView("vista_clientes_vip");
        });

        modelBuilder.Entity<VistaDashboardFinanciero>(entity =>
        {
            entity.ToView("vista_dashboard_financiero");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
