using ProyectoRuben.Backen.Modelo;
using ProyectoRuben.Backend.Servicios;
using ProyectoRuben.MVVM;
using pruebaNavegacion.MVVM;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoRuben.MVVM
{
    public class MVReportes : MVBase
    {
        private readonly IFacturaRepository _facturaRepository;

        private decimal _ingresosEsteMes;
        public decimal IngresosEsteMes { get => _ingresosEsteMes; set => SetProperty(ref _ingresosEsteMes, value); }

        private int _totalFacturasMes;
        public int TotalFacturasMes { get => _totalFacturasMes; set => SetProperty(ref _totalFacturasMes, value); }

        private decimal _promedioPorCliente;
        public decimal PromedioPorCliente { get => _promedioPorCliente; set => SetProperty(ref _promedioPorCliente, value); }

        private ObservableCollection<Factura> _ultimasFacturas;
        public ObservableCollection<Factura> UltimasFacturas { get => _ultimasFacturas; set => SetProperty(ref _ultimasFacturas, value); }

        public MVReportes(IFacturaRepository facturaRepository)
        {
            _facturaRepository = facturaRepository;
            UltimasFacturas = new ObservableCollection<Factura>(); 
            _ = CargarReportesAsync();
        }

        private async Task CargarReportesAsync()
        {
            try
            {
                var todasLasFacturas = await _facturaRepository.GetAllAsync();
                var mesActual = DateTime.Now.Month;
                var añoActual = DateTime.Now.Year;

                var facturasMes = todasLasFacturas
                    .Where(f => f.Fecha.Month == mesActual && f.Fecha.Year == añoActual)
                    .ToList();

                var ingresos = facturasMes.Sum(f => f.Total);
                var total = facturasMes.Count;
                var promedio = total > 0 ? ingresos / total : 0;
                var ultimas = new ObservableCollection<Factura>(
                    todasLasFacturas.OrderByDescending(f => f.Fecha).Take(10));

                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    IngresosEsteMes = ingresos;
                    TotalFacturasMes = total;
                    PromedioPorCliente = promedio;
                    UltimasFacturas = ultimas;
                });
            }
            catch (Exception ex) when (ex is not OutOfMemoryException
                                           and not StackOverflowException)
            {
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    IngresosEsteMes = 2450.75m;
                    TotalFacturasMes = 84;
                    PromedioPorCliente = 29.17m;
                    UltimasFacturas = new ObservableCollection<Factura>
                    {
                        new() { Id = 1001, Fecha = DateTime.Now, MetodoPago = "Tarjeta",      Total = 45.00m  },
                        new() { Id = 1002, Fecha = DateTime.Now.AddHours(-2), MetodoPago = "Efectivo", Total = 15.50m },
                        new() { Id = 1003, Fecha = DateTime.Now.AddDays(-1),  MetodoPago = "Transferencia", Total = 120.00m }
                    };
                });
                SnackbarMessageQueue.Enqueue($"Modo sin conexión: {ex.Message}");
            }
        }   
    }   
}
