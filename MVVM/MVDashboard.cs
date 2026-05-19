using Microsoft.EntityFrameworkCore;
using ProyectoRuben.Backen.Modelo;
using ProyectoRuben.Backend.Servicios;
using pruebaNavegacion.MVVM;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoRuben.MVVM
{
    public class MVDashboard : MVBase
    {
        // ── Repositorios ──────────────────────────────────────────────────────
        private readonly IReservaRepository _reservaRepository;
        private readonly IFacturaRepository _facturaRepository;
        private readonly IProductoRepository _productoRepository;
        private readonly IClienteRepository _clienteRepository;

        // ── Propiedades enlazadas al Dashboard ────────────────────────────────
        private string _reservasHoy = "-";
        public string ReservasHoy
        {
            get => _reservasHoy;
            set => SetProperty(ref _reservasHoy, value);
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

        private string _productosBajoStock = "-";
        public string ProductosBajoStock
        {
            get => _productosBajoStock;
            set => SetProperty(ref _productosBajoStock, value);
        }

        private List<CitaItem> _proximasCitas = new();
        public List<CitaItem> ProximasCitas
        {
            get => _proximasCitas;
            set => SetProperty(ref _proximasCitas, value);
        }

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
        }

        // ── Carga principal ───────────────────────────────────────────────────
        public async Task Inicializa()
        {
            try
            {
                var hoy = DateTime.Today;
                var primerDiaMes = new DateTime(hoy.Year, hoy.Month, 1);

                // ──────────────────────────────────────────────────────────────
                // 1. Reservas de hoy CON INCLUDES (la clave del fix)
                //    Usamos Query() con las propiedades de navegación para que
                //    Cliente, Servicio y Empleado no lleguen como null.
                // ──────────────────────────────────────────────────────────────
                var reservasHoy = await _reservaRepository
                    .Query(asNoTracking: true,
                           r => r.Cliente,
                           r => r.Servicio,
                           r => r.Empleado)
                    .Where(r => r.Fecha == hoy)          // comparación directa de date sin .Date
                    .OrderBy(r => r.Hora)
                    .ToListAsync();

                // ──────────────────────────────────────────────────────────────
                // 2. Clientes únicos atendidos este mes
                // ──────────────────────────────────────────────────────────────
                var clientesMes = await _reservaRepository
                    .Query(asNoTracking: true)
                    .Where(r => r.Fecha >= primerDiaMes && r.Estado == "Completada")
                    .Select(r => r.ClienteId)
                    .Distinct()
                    .CountAsync();

                // ──────────────────────────────────────────────────────────────
                // 3. Ingresos de hoy (facturas pagadas)
                // ──────────────────────────────────────────────────────────────
                var ingresos = await _facturaRepository
                    .Query(asNoTracking: true)
                    .Where(f => f.Fecha == hoy && f.Estado == "Pagada")
                    .SumAsync(f => (decimal?)f.Total) ?? 0m;

                // ──────────────────────────────────────────────────────────────
                // 4. Productos con stock bajo
                // ──────────────────────────────────────────────────────────────
                var stockBajo = await _productoRepository
                    .Query(asNoTracking: true)
                    .Where(p => p.Activo == true && p.Cantidad <= (p.StockMinimo ?? 0))
                    .CountAsync();

                // ──────────────────────────────────────────────────────────────
                // 5. Construir filas de la tabla de citas
                // ──────────────────────────────────────────────────────────────
                var citas = reservasHoy.Select(r => new CitaItem
                {
                    Hora = r.Hora.ToString(@"hh\:mm"),
                    Cliente = r.Cliente?.Nombre ?? "Sin cliente",
                    Servicio = r.Servicio?.Nombre ?? "Sin servicio",
                    Empleado = r.Empleado?.Nombre ?? "Sin asignar",
                    Estado = r.Estado
                }).ToList();

                // ──────────────────────────────────────────────────────────────
                // 6. Actualizar UI en el hilo principal
                // ──────────────────────────────────────────────────────────────
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

    /// <summary>
    /// Modelo de vista para cada fila de la tabla "Próximas Citas del Día".
    /// </summary>
    public class CitaItem
    {
        public string Hora { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public string Servicio { get; set; } = string.Empty;
        public string Empleado { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }
}