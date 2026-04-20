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
            CargarReportes();
        }

        private async void CargarReportes()
        {
            try
            {
                var todasLasFacturas = await _facturaRepository.GetAllAsync();
                // ... (el resto de tu código original del try se queda igual)
                var mesActual = DateTime.Now.Month;
                var añoActual = DateTime.Now.Year;

                var facturasMes = todasLasFacturas.Where(f => f.Fecha.Month == mesActual && f.Fecha.Year == añoActual).ToList();
                IngresosEsteMes = facturasMes.Sum(f => f.Total);
                TotalFacturasMes = facturasMes.Count;
                PromedioPorCliente = TotalFacturasMes > 0 ? IngresosEsteMes / TotalFacturasMes : 0;

                var ultimas10 = todasLasFacturas.OrderByDescending(f => f.Fecha).Take(10).ToList();
                UltimasFacturas = new ObservableCollection<Factura>(ultimas10);
            }
            catch
            {
                // DATOS FALSOS PARA VER EL DASHBOARD DE REPORTES
                IngresosEsteMes = 2450.75m;
                TotalFacturasMes = 84;
                PromedioPorCliente = 29.17m;

                UltimasFacturas = new ObservableCollection<Factura>
        {
            new Factura { Id = 1001, Fecha = DateTime.Now, MetodoPago = "Tarjeta", Total = 45.00m },
            new Factura { Id = 1002, Fecha = DateTime.Now.AddHours(-2), MetodoPago = "Efectivo", Total = 15.50m },
            new Factura { Id = 1003, Fecha = DateTime.Now.AddDays(-1), MetodoPago = "Bizum/Mixto", Total = 120.00m }
        };
                SnackbarMessageQueue.Enqueue("Modo sin conexión: Cargando reportes de prueba");
            }
        }
    }
}