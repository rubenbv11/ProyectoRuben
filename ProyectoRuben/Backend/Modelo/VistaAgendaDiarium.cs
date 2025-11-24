using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoRuben.Backen.Modelo;

[Keyless]
public partial class VistaAgendaDiarium
{
    [Column("Reserva_ID")]
    public int ReservaId { get; set; }

    [Column(TypeName = "date")]
    public DateTime Fecha { get; set; }

    [Column(TypeName = "time")]
    public TimeSpan Hora { get; set; }

    [StringLength(100)]
    public string Cliente { get; set; } = null!;

    [Column("Telefono_Cliente")]
    [StringLength(20)]
    public string? TelefonoCliente { get; set; }

    [StringLength(100)]
    public string Servicio { get; set; } = null!;

    /// <summary>
    /// Duración en minutos
    /// </summary>
    public int Duracion { get; set; }

    [StringLength(100)]
    public string Empleado { get; set; } = null!;

    [Column(TypeName = "enum('Pendiente','Confirmada','Cancelada','Completada')")]
    public string Estado { get; set; } = null!;

    [Column(TypeName = "text")]
    public string? Observaciones { get; set; }
}
