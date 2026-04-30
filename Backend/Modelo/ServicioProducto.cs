using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoRuben.Backen.Modelo;

[Table("servicio_producto")]
[Index("ProductoId", Name = "fk_SERVICIO_PRODUCTO_PRODUCTOS_idx")]
[Index("ServicioId", Name = "fk_SERVICIO_PRODUCTO_SERVICIOS_idx")]
public partial class ServicioProducto
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("Servicio_ID")]
    public int ServicioId { get; set; }

    [Column("Producto_ID")]
    public int ProductoId { get; set; }

    /// <summary>
    /// Cantidad de producto usada
    /// </summary>
    public int Cantidad { get; set; }

    [ForeignKey("ProductoId")]
    [InverseProperty("ServicioProductos")]
    public virtual Producto Producto { get; set; } = null!;

    [ForeignKey("ServicioId")]
    [InverseProperty("ServicioProductos")]
    public virtual Servicio Servicio { get; set; } = null!;
}
