using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoRuben.Backen.Modelo;

[Table("usuarios")]
[Index("Email", Name = "Email_UNIQUE", IsUnique = true)]
[Index("RolId", Name = "fk_USUARIOS_ROLES_idx")]
public partial class Usuario
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    /// <summary>
    /// Debe guardarse hasheada con BCrypt
    /// </summary>
    [StringLength(255)]
    public string Contrasena { get; set; } = null!;

    [Column(TypeName = "enum('Administrador','Empleado')")]
    public string Rol { get; set; } = null!;

    [Column("Rol_ID")]
    public int? RolId { get; set; }

    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(20)]
    public string? Telefono { get; set; }

    public bool? Activo { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaCreacion { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UltimoAcceso { get; set; }

    [InverseProperty("Usuario")]
    public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();

    [InverseProperty("Empleado")]
    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

    [ForeignKey("RolId")]
    [InverseProperty("Usuarios")]
    public virtual Role? RolNavigation { get; set; }
}
