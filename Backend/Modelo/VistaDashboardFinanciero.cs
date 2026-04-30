using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoRuben.Backen.Modelo;

[Keyless]
public partial class VistaDashboardFinanciero
{
    [StringLength(7)]
    public string? Mes { get; set; }

    [Column("Total_Transacciones")]
    public long TotalTransacciones { get; set; }

    [Precision(32)]
    public decimal? Subtotal { get; set; }

    [Column("IVA")]
    [Precision(32)]
    public decimal? Iva { get; set; }

    [Precision(32)]
    public decimal? Descuentos { get; set; }

    [Column("Total_Neto")]
    [Precision(32)]
    public decimal? TotalNeto { get; set; }
}
