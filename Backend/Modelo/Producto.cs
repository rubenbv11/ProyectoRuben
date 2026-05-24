using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;

namespace ProyectoRuben.Backen.Modelo;

[Table("productos")]
[Index("Activo", Name = "idx_activo")]
[Index("Nombre", Name = "idx_nombre")]
public partial class Producto : INotifyPropertyChanged
{
    // ── INotifyPropertyChanged ────────────────────────────────────────────────
    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // ── Propiedades mapeadas a BD ─────────────────────────────────────────────
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    [Column(TypeName = "text")]
    public string? Descripcion { get; set; }

    /// <summary>
    /// Notifica a WPF cuando cambia para que el binding se actualice al instante.
    /// </summary>
    private int _cantidad;
    public int Cantidad
    {
        get => _cantidad;
        set
        {
            if (_cantidad == value) return;
            _cantidad = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(AlertaStock)); // alerta depende de cantidad
        }
    }

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

    /// <summary>
    /// Categoría del producto para agrupar en UCCaja.
    /// No mapeada a BD.
    /// </summary>
    [Column("Categoria")]
    [StringLength(50)]
    public string? Categoria { get; set; }

    /// <summary>
    /// true cuando el stock está por debajo o igual al mínimo configurado.
    /// No mapeada a BD — se recalcula automáticamente al cambiar Cantidad.
    /// </summary>
    [NotMapped]
    public bool AlertaStock => _cantidad <= (StockMinimo ?? 0);
}