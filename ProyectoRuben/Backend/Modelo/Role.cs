using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoRuben.Backen.Modelo;

[Table("roles")]
[Index("Nombre", Name = "Nombre_UNIQUE", IsUnique = true)]
public partial class Role
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    [Column(TypeName = "text")]
    public string? Descripcion { get; set; }

    [InverseProperty("RolNavigation")]
    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();

    [ForeignKey("RolesId")]
    [InverseProperty("Roles")]
    public virtual ICollection<RolesPermisos> RolesPermisos { get; set; } = new List<RolesPermisos>();
}
