using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoRuben.Backen.Modelo;

[Table("ofertas")]
public partial class Oferta
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    [Column(TypeName = "text")]
    public string? Descripcion { get; set; }

    [Precision(5)]
    public decimal Descuento { get; set; }

    [Column(TypeName = "date")]
    public DateTime? FechaInicio { get; set; }

    [Column(TypeName = "date")]
    public DateTime? FechaFin { get; set; }

    public bool? Activa { get; set; }
}
