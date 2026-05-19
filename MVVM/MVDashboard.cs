using Microsoft.EntityFrameworkCore;
using ProyectoRuben.Backen.Modelo;
using ProyectoRuben.Backend.Servicios;
using pruebaNavegacion.MVVM;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProyectoRuben.MVVM
{
    public class MVDashboard : MVBase
    {
        // ── Repositorios ──────────────────────────────────────────────────────
        private readonly IReservaRepository _reservaRepository;
        private readonly IFacturaRepository _facturaRepository;
        private readonly IProductoRepository _productoRepository;
        private readonly IClienteRepository _clienteRepository;

        // ── KPIs ──────────────────────────────────────────────────────────────
        private string _reservasHoy = "-";
        public string ReservasHoy
        {
            get => _reservasHoy;
            set { SetProperty(ref _reservasHoy, value); OnPropertyChanged(nameof(SinCitasHoy)); }
        }

        private string _clientesAtendidosMes = "-";
        public string ClientesAtendidosMes
        {
            get => _clientesAtendidosMes;
            set => SetProperty(ref _clientesAtendidosMes, value);
        }

        private string _ingresosHoy = "0,00 €";
        public string IngresosHoy
        {
            get => _ingresosHoy;
            set => SetProperty(ref _ingresosHoy, value);
        }

        private string _productosBajoStock = "0";
        public string ProductosBajoStock
        {
            get => _productosBajoStock;
            set { SetProperty(ref _productosBajoStock, value); OnPropertyChanged(nameof(HayStockBajo)); }
        }

        /// <summary>True cuando hay productos con stock bajo → la card se pone en naranja.</summary>
        public bool HayStockBajo => int.TryParse(ProductosBajoStock, out var n) && n > 0;

        /// <summary>True cuando no hay reservas hoy → muestra el estado vacío en la tabla.</summary>
        public bool SinCitasHoy => ReservasHoy == "0" || ReservasHoy == "-";

        // ── Tabla de citas ────────────────────────────────────────────────────
        private List<CitaItem> _proximasCitas = new();
        public List<CitaItem> ProximasCitas
        {
            get => _proximasCitas;
            set => SetProperty(ref _proximasCitas, value);
        }

        // ── Comandos ──────────────────────────────────────────────────────────
        public ICommand RefrescarCommand { get; }

        // ── Constructor ───────────────────────────────────────────────────────
        public MVDashboard(IReservaRepository reservaRepository,
                           IFacturaRepository facturaRepository,
                           IProductoRepository productoRepository,
                           IClienteRepository clienteRepository)
        {
            _reservaRepository = reservaRepository;
            _facturaRepository = facturaRepository;
            _productoRepository = productoRepository;
            _clienteRepository = clienteRepository;

            RefrescarCommand = new RelayCommand(async _ => await Inicializa());
        }

        // ── Carga principal ───────────────────────────────────────────────────
        public async Task Inicializa()
        {
            try
            {
                var hoy = DateTime.Today;
                var primerDiaMes = new DateTime(hoy.Year, hoy.Month, 1);

                // 1. Reservas de hoy con navegaciones incluidas
                var reservasHoy = await _reservaRepository
                    .Query(asNoTracking: true,
                           r => r.Cliente,
                           r => r.Servicio,
                           r => r.Empleado)
                    .Where(r => r.Fecha == hoy)
                    .OrderBy(r => r.Hora)
                    .ToListAsync();

                // 2. Clientes únicos atendidos este mes
                //    Usamos cualquier reserva no cancelada para no depender de "Completada"
                var clientesMes = await _reservaRepository
                    .Query(asNoTracking: true)
                    .Where(r => r.Fecha >= primerDiaMes && r.Estado != "Cancelada")
                    .Select(r => r.ClienteId)
                    .Distinct()
                    .CountAsync();

                // 3. Ingresos de hoy (facturas pagadas)
                var ingresos = await _facturaRepository
                    .Query(asNoTracking: true)
                    .Where(f => f.Fecha == hoy && f.Estado == "Pagada")
                    .SumAsync(f => (decimal?)f.Total) ?? 0m;

                // 4. Productos con stock bajo
                var stockBajo = await _productoRepository
                    .Query(asNoTracking: true)
                    .Where(p => p.Activo == true && p.Cantidad <= (p.StockMinimo ?? 0))
                    .CountAsync();

                // 5. Construir filas de la tabla
                var citas = reservasHoy.Select(r => new CitaItem
                {
                    ReservaId = r.Id,
                    Hora = r.Hora.ToString(@"hh\:mm"),
                    Cliente = r.Cliente?.Nombre ?? "Sin cliente",
                    Servicio = r.Servicio?.Nombre ?? "Sin servicio",
                    Empleado = r.Empleado?.Nombre ?? "Sin asignar",
                    Estado = r.Estado
                }).ToList();

                // 6. Actualizar UI
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ReservasHoy = reservasHoy.Count.ToString();
                    ClientesAtendidosMes = clientesMes.ToString();
                    IngresosHoy = ingresos.ToString("N2", new CultureInfo("es-ES")) + " €";
                    ProductosBajoStock = stockBajo.ToString();
                    ProximasCitas = citas;
                });
            }
            catch (Exception ex)
            {
                SnackbarMessageQueue.Enqueue($"Error cargando dashboard: {ex.Message}");
            }
        }
    }

    /// <summary>Fila de la tabla de citas del día.</summary>
    public class CitaItem
    {
        public int ReservaId { get; set; }
        public string Hora { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public string Servicio { get; set; } = string.Empty;
        public string Empleado { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }
}