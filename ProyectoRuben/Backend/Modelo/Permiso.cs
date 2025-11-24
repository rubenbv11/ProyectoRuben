using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoRuben.Backen.Modelo;

[Table("permisos")]
[Index("Nombre", Name = "Nombre_UNIQUE", IsUnique = true)]
public partial class Permiso
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    [Column(TypeName = "text")]
    public string? Descripcion { get; set; }

    [ForeignKey("PermisosId")]
    [InverseProperty("Permisos")]
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
