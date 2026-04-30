using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoRuben.Backen.Modelo;

[Keyless]
public partial class VistaClientesVip
{
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(20)]
    public string? Telefono { get; set; }

    [Column("Total_Visitas")]
    public long TotalVisitas { get; set; }

    [Column("Gasto_Total")]
    [Precision(32)]
    public decimal? GastoTotal { get; set; }

    [Column("Ultima_Visita", TypeName = "date")]
    public DateTime? UltimaVisita { get; set; }
}
