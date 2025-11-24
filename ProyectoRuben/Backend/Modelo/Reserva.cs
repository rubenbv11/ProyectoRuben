using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoRuben.Backen.Modelo;

[Table("reservas")]
[Index("ClienteId", Name = "fk_RESERVAS_CLIENTES_idx")]
[Index("EmpleadoId", Name = "fk_RESERVAS_EMPLEADOS_idx")]
[Index("ServicioId", Name = "fk_RESERVAS_SERVICIOS_idx")]
[Index("Estado", Name = "idx_estado")]
[Index("Fecha", Name = "idx_fecha")]
public partial class Reserva
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("Cliente_ID")]
    public int ClienteId { get; set; }

    [Column("Servicio_ID")]
    public int ServicioId { get; set; }

    [Column("Empleado_ID")]
    public int EmpleadoId { get; set; }

    [Column(TypeName = "date")]
    public DateTime Fecha { get; set; }

    [Column(TypeName = "time")]
    public TimeSpan Hora { get; set; }

    [Column(TypeName = "enum('Pendiente','Confirmada','Cancelada','Completada')")]
    public string Estado { get; set; } = null!;

    [Column(TypeName = "text")]
    public string? Observaciones { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaCreacion { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaModificacion { get; set; }

    [ForeignKey("ClienteId")]
    [InverseProperty("Reservas")]
    public virtual Cliente Cliente { get; set; } = null!;

    [ForeignKey("EmpleadoId")]
    [InverseProperty("Reservas")]
    public virtual Usuario Empleado { get; set; } = null!;

    [InverseProperty("Reserva")]
    public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();

    [InverseProperty("Reserva")]
    public virtual ICollection<Horario> Horarios { get; set; } = new List<Horario>();

    [ForeignKey("ServicioId")]
    [InverseProperty("Reservas")]
    public virtual Servicio Servicio { get; set; } = null!;
}
