using ProyectoRuben.Backen.Modelo;
using ProyectoRuben.Backend.Servicios;
using pruebaNavegacion.MVVM;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ProyectoRuben.MVVM
{
    // ── Fila enriquecida para el DataGrid ─────────────────────────────────────
    public class FacturaVista
    {
        public int Id { get; set; }
        public string Fecha { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public string MetodoPago { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; } = string.Empty;

        public string SubtotalTexto => Subtotal.ToString("F2", new CultureInfo("es-ES")) + " €";
        public string TotalTexto => Total.ToString("F2", new CultureInfo("es-ES")) + " €";
        public string DescuentoTexto => Descuento > 0 ? "-" + Descuento.ToString("F2", new CultureInfo("es-ES")) + " €" : "—";
    }

    public class MVReportes : MVBase
    {
        private readonly IFacturaRepository _facturaRepository;
        private readonly IClienteRepository _clienteRepository;

        // ── Filtros ───────────────────────────────────────────────────────────
        public ObservableCollection<string> Meses { get; } = new()
        {
            "Enero","Febrero","Marzo","Abril","Mayo","Junio",
            "Julio","Agosto","Septiembre","Octubre","Noviembre","Diciembre"
        };

        public ObservableCollection<int> Anios { get; } = new();

        private string _mesSeleccionado;
        public string MesSeleccionado
        {
            get => _mesSeleccionado;
            set { SetProperty(ref _mesSeleccionado, value); _ = CargarReportes(); }
        }

        private int _anioSeleccionado;
        public int AnioSeleccionado
        {
            get => _anioSeleccionado;
            set { SetProperty(ref _anioSeleccionado, value); _ = CargarReportes(); }
        }

        // ── KPIs ──────────────────────────────────────────────────────────────
        private decimal _ingresosMes;
        public decimal IngresosMes { get => _ingresosMes; set { SetProperty(ref _ingresosMes, value); OnPropertyChanged(nameof(IngresosMesTexto)); } }
        public string IngresosMesTexto => IngresosMes.ToString("N2", new CultureInfo("es-ES")) + " €";

        private int _totalFacturas;
        public int TotalFacturas { get => _totalFacturas; set => SetProperty(ref _totalFacturas, value); }

        private decimal _ticketMedio;
        public decimal TicketMedio { get => _ticketMedio; set { SetProperty(ref _ticketMedio, value); OnPropertyChanged(nameof(TicketMedioTexto)); } }
        public string TicketMedioTexto => TicketMedio.ToString("F2", new CultureInfo("es-ES")) + " €";

        private decimal _totalDescuentos;
        public decimal TotalDescuentos { get => _totalDescuentos; set { SetProperty(ref _totalDescuentos, value); OnPropertyChanged(nameof(TotalDescuentosTexto)); } }
        public string TotalDescuentosTexto => TotalDescuentos.ToString("F2", new CultureInfo("es-ES")) + " €";

        private string _metodoPagoTop = "—";
        public string MetodoPagoTop { get => _metodoPagoTop; set => SetProperty(ref _metodoPagoTop, value); }

        // ── Facturas ──────────────────────────────────────────────────────────
        private ObservableCollection<FacturaVista> _facturas = new();
        public ObservableCollection<FacturaVista> Facturas { get => _facturas; set => SetProperty(ref _facturas, value); }

        private bool _sinDatos;
        public bool SinDatos { get => _sinDatos; set => SetProperty(ref _sinDatos, value); }

        private bool _cargando;
        public bool Cargando { get => _cargando; set => SetProperty(ref _cargando, value); }

        // ── Comandos ──────────────────────────────────────────────────────────
        public ICommand RefrescarCommand { get; }

        // ═════════════════════════════════════════════════════════════════════
        public MVReportes(IFacturaRepository facturaRepository,
                          IClienteRepository clienteRepository)
        {
            _facturaRepository = facturaRepository;
            _clienteRepository = clienteRepository;

            // Años disponibles: 3 años atrás hasta hoy
            var hoy = DateTime.Now;
            for (int y = hoy.Year; y >= hoy.Year - 3; y--)
                Anios.Add(y);

            _anioSeleccionado = hoy.Year;
            _mesSeleccionado = Meses[hoy.Month - 1];

            RefrescarCommand = new RelayCommand(async _ => await CargarReportes());

            _ = CargarReportes();
        }

        // ── Carga ─────────────────────────────────────────────────────────────
        public async Task CargarReportes()
        {
            try
            {
                Cargando = true;
                SinDatos = false;

                int mes = Meses.IndexOf(MesSeleccionado) + 1;
                int anio = AnioSeleccionado;

                var todasFacturas = await GetAllAsync(_facturaRepository);
                var todosClientes = await GetAllAsync(_clienteRepository);

                var facturasMes = todasFacturas
                    .Where(f => f.Fecha.Month == mes && f.Fecha.Year == anio)
                    .OrderByDescending(f => f.Fecha)
                    .ToList();

                if (!facturasMes.Any())
                {
                    SinDatos = true;
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        IngresosMes = 0; TotalFacturas = 0;
                        TicketMedio = 0; TotalDescuentos = 0;
                        MetodoPagoTop = "—";
                        Facturas = new ObservableCollection<FacturaVista>();
                    });
                    return;
                }

                var ingresos = facturasMes.Where(f => f.Estado == "Pagada").Sum(f => f.Total);
                var pagadas = facturasMes.Count(f => f.Estado == "Pagada");
                var ticket = pagadas > 0 ? ingresos / pagadas : 0;
                var descuentos = facturasMes.Sum(f => f.Descuento ?? 0);
                var metodoPagTop = facturasMes.GroupBy(f => f.MetodoPago)
                                              .OrderByDescending(g => g.Count())
                                              .FirstOrDefault()?.Key ?? "—";

                var filas = facturasMes.Select(f => new FacturaVista
                {
                    Id = f.Id,
                    Fecha = f.Fecha.ToString("dd/MM/yyyy"),
                    Cliente = todosClientes.FirstOrDefault(c => c.Id == f.ClienteId)?.Nombre ?? "Desconocido",
                    MetodoPago = f.MetodoPago,
                    Subtotal = f.Subtotal ?? f.Total,
                    Descuento = f.Descuento ?? 0,
                    Total = f.Total,
                    Estado = f.Estado ?? "Pagada"
                }).ToList();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    IngresosMes = ingresos;
                    TotalFacturas = facturasMes.Count;
                    TicketMedio = ticket;
                    TotalDescuentos = descuentos;
                    MetodoPagoTop = metodoPagTop;
                    Facturas = new ObservableCollection<FacturaVista>(filas);
                    SinDatos = false;
                });
            }
            catch (Exception ex)
            {
                SnackbarMessageQueue.Enqueue($"Error cargando reportes: {ex.Message}");
                SinDatos = true;
            }
            finally
            {
                Cargando = false;
            }
        }
    }
}