using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoRuben.Backen.Modelo;

[Table("clientes")]
[Index("Email", Name = "idx_email")]
[Index("Telefono", Name = "idx_telefono")]
public partial class Cliente
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    /// <summary>
    /// Email o teléfono
    /// </summary>
    [StringLength(100)]
    public string Contacto { get; set; } = null!;

    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(20)]
    public string? Telefono { get; set; }

    [Column("Historial_Citas", TypeName = "text")]
    public string? HistorialCitas { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaRegistro { get; set; }

    public bool? Activo { get; set; }

    [InverseProperty("Cliente")]
    public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();

    [InverseProperty("Cliente")]
    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
