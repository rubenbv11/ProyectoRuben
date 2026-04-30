using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoRuben.Backen.Modelo;

[Table("factura")]
[Index("ClienteId", Name = "fk_FACTURA_CLIENTES_idx")]
[Index("ReservaId", Name = "fk_FACTURA_RESERVAS_idx")]
[Index("UsuarioId", Name = "fk_FACTURA_USUARIOS_idx")]
[Index("Estado", Name = "idx_estado")]
[Index("Fecha", Name = "idx_fecha")]
public partial class Factura
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("Cliente_ID")]
    public int ClienteId { get; set; }

    [Column("Usuario_ID")]
    public int UsuarioId { get; set; }

    /// <summary>
    /// Opcional: vincula con reserva
    /// </summary>
    [Column("Reserva_ID")]
    public int? ReservaId { get; set; }

    [Column(TypeName = "date")]
    public DateTime Fecha { get; set; }

    [Precision(10)]
    public decimal Total { get; set; }

    [Precision(10)]
    public decimal? Subtotal { get; set; }

    [Precision(10)]
    public decimal? Impuesto { get; set; }

    [Precision(10)]
    public decimal? Descuento { get; set; }

    [Column("Metodo_Pago", TypeName = "enum('Efectivo','Tarjeta','Transferencia','Mixto')")]
    public string MetodoPago { get; set; } = null!;

    [Column(TypeName = "text")]
    public string? Detalle { get; set; }

    [Column(TypeName = "enum('Pendiente','Pagada','Anulada')")]
    public string? Estado { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaCreacion { get; set; }

    [ForeignKey("ClienteId")]
    [InverseProperty("Facturas")]
    public virtual Cliente Cliente { get; set; } = null!;

    [ForeignKey("ReservaId")]
    [InverseProperty("Facturas")]
    public virtual Reserva? Reserva { get; set; }

    [ForeignKey("UsuarioId")]
    [InverseProperty("Facturas")]
    public virtual Usuario Usuario { get; set; } = null!;
}
