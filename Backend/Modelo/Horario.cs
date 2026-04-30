using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoRuben.Backen.Modelo;

[Table("horarios")]
[Index("ReservaId", Name = "fk_HORARIOS_RESERVAS_idx")]
public partial class Horario
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("Reserva_ID")]
    public int ReservaId { get; set; }

    [Column(TypeName = "date")]
    public DateTime Fecha { get; set; }

    [Column("Hora_Inicio", TypeName = "time")]
    public TimeSpan HoraInicio { get; set; }

    [Column("Hora_Fin", TypeName = "time")]
    public TimeSpan HoraFin { get; set; }

    [ForeignKey("ReservaId")]
    [InverseProperty("Horarios")]
    public virtual Reserva Reserva { get; set; } = null!;
}
