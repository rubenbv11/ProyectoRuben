using Microsoft.EntityFrameworkCore;
using ProyectoRuben.Backen.Modelo;
using ProyectoRuben.Backend.Servicios;
using pruebaNavegacion.MVVM; // Namespace donde está MVBase
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoRuben.MVVM
{
    public class MVDashboard : MVBase
    {
        // --- Repositorios (Inyectados) ---
        private readonly IReservaRepository _reservaRepository;
        private readonly IFacturaRepository _facturaRepository;
        private readonly IProductoRepository _productoRepository;
        private readonly IClienteRepository _clienteRepository;

        // --- Variables Privadas ---
        private string _reservasHoy;
        private string _clientesAtendidosMes;
        private string _ingresosHoy;
        private string _productosBajoStock;
        private List<CitaItem> _proximasCitas;

        // --- Propiedades Públicas (con notificación SetProperty) ---
        public string ReservasHoy
        {
            get => _reservasHoy;
            set => SetProperty(ref _reservasHoy, value);
        }

        public string ClientesAtendidosMes
        {
            get => _clientesAtendidosMes;
            set => SetProperty(ref _clientesAtendidosMes, value);
        }

        public string IngresosHoy
        {
            get => _ingresosHoy;
            set => SetProperty(ref _ingresosHoy, value);
        }

        public string ProductosBajoStock
        {
            get => _productosBajoStock;
            set => SetProperty(ref _productosBajoStock, value);
        }

        public List<CitaItem> ProximasCitas
        {
            get => _proximasCitas;
            set => SetProperty(ref _proximasCitas, value);
        }

        // --- Constructor con Inyección de Dependencias ---
        public MVDashboard(IReservaRepository reservaRepository,
                           IFacturaRepository facturaRepository,
                           IProductoRepository productoRepository,
                           IClienteRepository clienteRepository)
        {
            _reservaRepository = reservaRepository;
            _facturaRepository = facturaRepository;
            _productoRepository = productoRepository;
            _clienteRepository = clienteRepository;

            // Valores por defecto
            _reservasHoy = "-";
            _clientesAtendidosMes = "-";
            _ingresosHoy = "€0,00";
            _productosBajoStock = "-";
            _proximasCitas = new List<CitaItem>();
        }

        // --- Método Inicializa (Lógica de Negocio) ---
        public async Task Inicializa()
        {
            try
            {
                var hoy = DateTime.Today;

                // 1. Reservas de Hoy
                var reservas = await _reservaRepository.GetReservasByFechaAsync(hoy);
                ReservasHoy = reservas.Count().ToString();

                // 2. Clientes Atendidos (Este Mes)
                var primerDiaMes = new DateTime(hoy.Year, hoy.Month, 1);
                var totalClientes = await _reservaRepository.Query()
                    .Where(r => r.Fecha >= primerDiaMes && r.Estado == "Completada")
                    .Select(r => r.ClienteId)
                    .Distinct()
                    .CountAsync();
                ClientesAtendidosMes = totalClientes.ToString();

                // 3. Ingresos Hoy
                var ingresos = await _facturaRepository.Query()
                    .Where(f => f.Fecha.Date == hoy)
                    .SumAsync(f => f.Total);
                IngresosHoy = ingresos.ToString("C", CultureInfo.CurrentCulture);

                // 4. Stock Bajo (Productos activos con stock <= mínimo)
                var stockBajo = await _productoRepository.Query()
                    .Where(p => p.Activo == true && p.Cantidad <= (p.StockMinimo ?? 0))
                    .CountAsync();
                ProductosBajoStock = stockBajo.ToString();

                // 5. Tabla de Próximas Citas
                ProximasCitas = reservas
                    .Where(r => r.Fecha.Date == hoy)
                    .OrderBy(r => r.Hora)
                    .Select(r => new CitaItem
                    {
                        Hora = r.Hora.ToString(@"hh\:mm"),
                        Cliente = r.Cliente?.Nombre ?? "Desconocido",
                        Servicio = r.Servicio?.Nombre ?? "Varios",
                        Empleado = r.Empleado?.Nombre ?? "Sin asignar",
                        Estado = r.Estado
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                // Si algo falla, usamos el Snackbar que hereda de MVBase
                SnackbarMessageQueue.Enqueue($"Error cargando datos: {ex.Message}");
            }
        }
    }

    // Clase auxiliar para el DataGrid
    public class CitaItem
    {
        public string Hora { get; set; }
        public string Cliente { get; set; }
        public string Servicio { get; set; }
        public string Empleado { get; set; }
        public string Estado { get; set; }
    }
}