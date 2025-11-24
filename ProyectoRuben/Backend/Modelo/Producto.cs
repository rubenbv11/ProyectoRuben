using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoRuben.Backen.Modelo;

[Table("productos")]
[Index("Activo", Name = "idx_activo")]
[Index("Nombre", Name = "idx_nombre")]
public partial class Producto
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    [Column(TypeName = "text")]
    public string? Descripcion { get; set; }

    public int Cantidad { get; set; }

    public int? StockMinimo { get; set; }

    public int? StockMaximo { get; set; }

    [Column("Fecha_Vencimiento", TypeName = "date")]
    public DateTime? FechaVencimiento { get; set; }

    [Precision(10)]
    public decimal Precio { get; set; }

    [StringLength(100)]
    public string? Proveedor { get; set; }

    public bool? Activo { get; set; }

    [Column("ImagenURL")]
    [StringLength(255)]
    public string? ImagenUrl { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaCreacion { get; set; }

    [InverseProperty("Producto")]
    public virtual ICollection<ServicioProducto> ServicioProductos { get; set; } = new List<ServicioProducto>();
}
