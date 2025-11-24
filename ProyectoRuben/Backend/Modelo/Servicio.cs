using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoRuben.Backen.Modelo;

[Table("servicios")]
[Index("Activo", Name = "idx_activo")]
[Index("Nombre", Name = "idx_nombre")]
public partial class Servicio
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    [Column(TypeName = "text")]
    public string? Descripcion { get; set; }

    /// <summary>
    /// Duración en minutos
    /// </summary>
    public int Duracion { get; set; }

    [Precision(10)]
    public decimal Costo { get; set; }

    public bool? Activo { get; set; }

    [Column("ImagenURL")]
    [StringLength(255)]
    public string? ImagenUrl { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaCreacion { get; set; }

    [InverseProperty("Servicio")]
    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

    [InverseProperty("Servicio")]
    public virtual ICollection<ServicioProducto> ServicioProductos { get; set; } = new List<ServicioProducto>();
}
