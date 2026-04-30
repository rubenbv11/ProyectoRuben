using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoRuben.Backen.Modelo;

[Table("configuracion")]
[Index("Clave", Name = "Clave_UNIQUE", IsUnique = true)]
public partial class Configuracion
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(100)]
    public string Clave { get; set; } = null!;

    [Column(TypeName = "text")]
    public string Valor { get; set; } = null!;

    [StringLength(255)]
    public string? Descripcion { get; set; }

    [Column(TypeName = "enum('String','Number','Boolean','JSON')")]
    public string? Tipo { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaModificacion { get; set; }
}
