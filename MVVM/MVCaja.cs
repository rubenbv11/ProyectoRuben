using di.proyecto.clase._2025.Frontend.Mensajes;
using ProyectoRuben.Backen.Modelo;
using ProyectoRuben.Backend.Servicios;
using ProyectoRuben.Frontend;
using pruebaNavegacion.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ProyectoRuben.MVVM
{
    // ── Grupo de categoría para el acordeón del catálogo ─────────────────────
    public class GrupoServicio : MVBase
    {
        public string Categoria { get; set; } = string.Empty;
        public string Icono { get; set; } = "Scissors";
        public ObservableCollection<Servicio> Items { get; set; } = new();

        private bool _expandido = true;
        public bool Expandido { get => _expandido; set => SetProperty(ref _expandido, value); }

        public ICommand ToggleCommand { get; }
        public GrupoServicio() => ToggleCommand = new RelayCommand(_ => Expandido = !Expandido);
    }

    public class GrupoProducto : MVBase
    {
        public string Categoria { get; set; } = string.Empty;
        public string Icono { get; set; } = "PackageVariant";
        public ObservableCollection<Producto> Items { get; set; } = new();

        private bool _expandido = true;
        public bool Expandido { get => _expandido; set => SetProperty(ref _expandido, value); }

        public ICommand ToggleCommand { get; }
        public GrupoProducto() => ToggleCommand = new RelayCommand(_ => Expandido = !Expandido);
    }

    // ── Línea del ticket ──────────────────────────────────────────────────────
    public class LineaTicket : ValidatableViewModel
    {
        private string _descripcion = string.Empty;
        private int _cantidad = 1;
        private decimal _precioUnitario;
        private string _tipo = "Servicio"; // "Servicio" | "Producto"

        public string Descripcion
        {
            get => _descripcion;
            set { SetProperty(ref _descripcion, value); OnPropertyChanged(nameof(Importe)); }
        }
        public int Cantidad
        {
            get => _cantidad;
            set { SetProperty(ref _cantidad, value < 1 ? 1 : value); OnPropertyChanged(nameof(Importe)); }
        }
        public decimal PrecioUnitario
        {
            get => _precioUnitario;
            set { SetProperty(ref _precioUnitario, value); OnPropertyChanged(nameof(Importe)); }
        }
        public string Tipo { get => _tipo; set => SetProperty(ref _tipo, value); }
        public decimal Importe => Cantidad * PrecioUnitario;
    }

    // ── ViewModel Caja ────────────────────────────────────────────────────────
    public class MVCaja : MVBase
    {
        private readonly IFacturaRepository _facturaRepository;
        private readonly IReservaRepository _reservaRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IServicioRepository _servicioRepository;
        private readonly IProductoRepository _productoRepository;

        // ── Cliente / Reserva ─────────────────────────────────────────────────
        private Cliente? _clienteSeleccionado;
        public Cliente? ClienteSeleccionado
        {
            get => _clienteSeleccionado;
            set
            {
                SetProperty(ref _clienteSeleccionado, value);
                OnPropertyChanged(nameof(TieneCliente));
                OnPropertyChanged(nameof(NombreCliente));
                OnPropertyChanged(nameof(TelefonoCliente));
            }
        }

        private Reserva? _reservaVinculada;
        public Reserva? ReservaVinculada
        {
            get => _reservaVinculada;
            set { SetProperty(ref _reservaVinculada, value); OnPropertyChanged(nameof(TieneReserva)); }
        }

        public bool TieneCliente => ClienteSeleccionado != null;
        public bool TieneReserva => ReservaVinculada != null;
        public string NombreCliente => ClienteSeleccionado?.Nombre ?? string.Empty;
        public string TelefonoCliente => ClienteSeleccionado?.Telefono ?? string.Empty;

        // ── Búsqueda ──────────────────────────────────────────────────────────
        private string _busquedaCliente = string.Empty;
        public string BusquedaCliente
        {
            get => _busquedaCliente;
            set { SetProperty(ref _busquedaCliente, value); FiltrarClientes(); OnPropertyChanged(nameof(HaySugerenciasClientes)); }
        }

        private string _busquedaServicio = string.Empty;
        public string BusquedaServicio
        {
            get => _busquedaServicio;
            set { SetProperty(ref _busquedaServicio, value); FiltrarServicios(); }
        }

        private string _busquedaProducto = string.Empty;
        public string BusquedaProducto
        {
            get => _busquedaProducto;
            set { SetProperty(ref _busquedaProducto, value); FiltrarProductos(); }
        }

        private ObservableCollection<Cliente> _clientesFiltrados = new();
        public ObservableCollection<Cliente> ClientesFiltrados
        {
            get => _clientesFiltrados;
            set { SetProperty(ref _clientesFiltrados, value); OnPropertyChanged(nameof(HaySugerenciasClientes)); }
        }

        public bool HaySugerenciasClientes =>
            !string.IsNullOrEmpty(_busquedaCliente) && _clientesFiltrados.Count > 0;

        private ObservableCollection<Servicio> _serviciosFiltrados = new();
        public ObservableCollection<Servicio> ServiciosFiltrados
        {
            get => _serviciosFiltrados;
            set { SetProperty(ref _serviciosFiltrados, value); ActualizarGruposServicios(); }
        }

        private ObservableCollection<Producto> _productosFiltrados = new();
        public ObservableCollection<Producto> ProductosFiltrados
        {
            get => _productosFiltrados;
            set { SetProperty(ref _productosFiltrados, value); ActualizarGruposProductos(); }
        }

        private ObservableCollection<GrupoServicio> _gruposServicios = new();
        public ObservableCollection<GrupoServicio> GruposServicios
        {
            get => _gruposServicios;
            set => SetProperty(ref _gruposServicios, value);
        }

        private ObservableCollection<GrupoProducto> _gruposProductos = new();
        public ObservableCollection<GrupoProducto> GruposProductos
        {
            get => _gruposProductos;
            set => SetProperty(ref _gruposProductos, value);
        }

        // Listas completas en memoria
        private System.Collections.Generic.List<Cliente> _todosClientes = new();
        private System.Collections.Generic.List<Servicio> _todosServicios = new();
        private System.Collections.Generic.List<Producto> _todosProductos = new();

        // ── Ticket ────────────────────────────────────────────────────────────
        private ObservableCollection<LineaTicket> _lineasTicket = new();
        public ObservableCollection<LineaTicket> LineasTicket
        {
            get => _lineasTicket;
            set => SetProperty(ref _lineasTicket, value);
        }

        // ── Totales ───────────────────────────────────────────────────────────
        private decimal _descuento;
        public decimal Descuento
        {
            get => _descuento;
            set { SetProperty(ref _descuento < 0 ? ref _descuento : ref _descuento, value < 0 ? 0 : value); RecalcularTotales(); }
        }

        private decimal _subtotal;
        public decimal Subtotal { get => _subtotal; private set { SetProperty(ref _subtotal, value); OnPropertyChanged(nameof(SubtotalTexto)); } }
        public string SubtotalTexto => Subtotal.ToString("F2") + " €";

        private decimal _total;
        public decimal Total { get => _total; private set { SetProperty(ref _total, value); OnPropertyChanged(nameof(TotalTexto)); } }
        public string TotalTexto => Total.ToString("F2") + " €";

        private decimal _descuentoAplicado;
        public decimal DescuentoAplicado { get => _descuentoAplicado; private set { SetProperty(ref _descuentoAplicado, value); OnPropertyChanged(nameof(DescuentoTexto)); } }
        public string DescuentoTexto => "-" + DescuentoAplicado.ToString("F2") + " €";

        // ── Pago ──────────────────────────────────────────────────────────────
        private string _metodoPago = "Efectivo";
        public string MetodoPago { get => _metodoPago; set => SetProperty(ref _metodoPago, value); }

        public ObservableCollection<string> MetodosPago { get; } = new()
        {
            "Efectivo", "Tarjeta", "Transferencia", "Mixto"
        };

        // ── Estado carga ──────────────────────────────────────────────────────
        private bool _ticketVacio = true;
        public bool TicketVacio { get => _ticketVacio; set => SetProperty(ref _ticketVacio, value); }

        private bool _cobrando;
        public bool Cobrando { get => _cobrando; set => SetProperty(ref _cobrando, value); }

        // ── Panel activo (servicios / productos) ──────────────────────────────
        private bool _panelServicios = true;
        public bool PanelServicios { get => _panelServicios; set { SetProperty(ref _panelServicios, value); OnPropertyChanged(nameof(PanelProductos)); } }
        public bool PanelProductos => !_panelServicios;

        // ── Comandos ──────────────────────────────────────────────────────────
        public ICommand SeleccionarClienteCommand { get; }
        public ICommand LimpiarClienteCommand { get; }
        public ICommand AgregarServicioCommand { get; }
        public ICommand AgregarProductoCommand { get; }
        public ICommand EliminarLineaCommand { get; }
        public ICommand IncrementarCantidadCommand { get; }
        public ICommand DecrementarCantidadCommand { get; }
        public ICommand CobrarCommand { get; }
        public ICommand LimpiarTicketCommand { get; }
        public ICommand MostrarServiciosCommand { get; }
        public ICommand MostrarProductosCommand { get; }
        public ICommand SeleccionarMetodoPagoCommand { get; }

        // ═════════════════════════════════════════════════════════════════════
        public MVCaja(IFacturaRepository facturaRepository,
                      IReservaRepository reservaRepository,
                      IClienteRepository clienteRepository,
                      IServicioRepository servicioRepository,
                      IProductoRepository productoRepository)
        {
            _facturaRepository = facturaRepository;
            _reservaRepository = reservaRepository;
            _clienteRepository = clienteRepository;
            _servicioRepository = servicioRepository;
            _productoRepository = productoRepository;

            SeleccionarClienteCommand = new RelayCommand(p => SeleccionarCliente(p as Cliente));
            LimpiarClienteCommand = new RelayCommand(_ => LimpiarCliente());
            AgregarServicioCommand = new RelayCommand(p => AgregarServicio(p as Servicio));
            AgregarProductoCommand = new RelayCommand(p => AgregarProducto(p as Producto));
            EliminarLineaCommand = new RelayCommand(p => EliminarLinea(p as LineaTicket));
            IncrementarCantidadCommand = new RelayCommand(p => { if (p is LineaTicket l) { l.Cantidad++; RecalcularTotales(); } });
            DecrementarCantidadCommand = new RelayCommand(p => { if (p is LineaTicket l && l.Cantidad > 1) { l.Cantidad--; RecalcularTotales(); } });
            CobrarCommand = new RelayCommand(async _ => await Cobrar());
            LimpiarTicketCommand = new RelayCommand(_ => LimpiarTodo());
            MostrarServiciosCommand = new RelayCommand(_ => PanelServicios = true);
            MostrarProductosCommand = new RelayCommand(_ => PanelServicios = false);
            SeleccionarMetodoPagoCommand = new RelayCommand(p => { if (p is string m) MetodoPago = m; });

            _ = CargarCatalogos();
        }

        // ── Carga catálogos ───────────────────────────────────────────────────
        private async Task CargarCatalogos()
        {
            try
            {
                _todosClientes = await GetAllAsync(_clienteRepository);
                _todosServicios = await GetAllAsync(_servicioRepository);
                _todosProductos = await GetAllAsync(_productoRepository);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ClientesFiltrados = new ObservableCollection<Cliente>(_todosClientes.Where(c => c.Activo == true));
                    ServiciosFiltrados = new ObservableCollection<Servicio>(_todosServicios.Where(s => s.Activo == true));
                    ProductosFiltrados = new ObservableCollection<Producto>(_todosProductos.Where(p => p.Activo == true));
                });
            }
            catch (Exception ex)
            {
                SnackbarMessageQueue.Enqueue($"Error cargando catálogos: {ex.Message}");
            }
        }

        // ── Filtros ───────────────────────────────────────────────────────────
        private void FiltrarClientes()
        {
            var filtro = _busquedaCliente.Trim();
            var lista = string.IsNullOrEmpty(filtro)
                ? _todosClientes.Where(c => c.Activo == true)
                : _todosClientes.Where(c => c.Activo == true &&
                    (c.Nombre.Contains(filtro, StringComparison.OrdinalIgnoreCase) ||
                     (c.Telefono ?? string.Empty).Contains(filtro)));
            ClientesFiltrados = new ObservableCollection<Cliente>(lista);
        }

        private void FiltrarServicios()
        {
            var filtro = _busquedaServicio.Trim();
            var lista = string.IsNullOrEmpty(filtro)
                ? _todosServicios.Where(s => s.Activo == true)
                : _todosServicios.Where(s => s.Activo == true &&
                    s.Nombre.Contains(filtro, StringComparison.OrdinalIgnoreCase));
            ServiciosFiltrados = new ObservableCollection<Servicio>(lista);
        }

        private void FiltrarProductos()
        {
            var filtro = _busquedaProducto.Trim();
            var lista = string.IsNullOrEmpty(filtro)
                ? _todosProductos.Where(p => p.Activo == true && p.Cantidad > 0)
                : _todosProductos.Where(p => p.Activo == true && p.Cantidad > 0 &&
                    p.Nombre.Contains(filtro, StringComparison.OrdinalIgnoreCase));
            ProductosFiltrados = new ObservableCollection<Producto>(lista);
        }

        // Iconos por categoría de servicio
        private static readonly Dictionary<string, string> _iconosServicio = new()
        {
            ["Corte"] = "Scissors",
            ["Color"] = "Palette",
            ["Uñas"] = "HandPointingUp",
            ["Tratamientos"] = "Spa",
            ["Estética"] = "StarCircle",
            ["General"] = "TagMultiple",
        };

        // Iconos por categoría de producto
        private static readonly Dictionary<string, string> _iconosProducto = new()
        {
            ["Cabello"] = "HairDryer",
            ["Color"] = "PaletteOutline",
            ["Fijación"] = "SprayBottle",
            ["Uñas"] = "Nail",
            ["Piel"] = "FaceWoman",
            ["Depilación"] = "Wax",
            ["Accesorios"] = "Hanger",
            ["General"] = "PackageVariant",
        };

        private void ActualizarGruposServicios()
        {
            // Preservar estado expandido de grupos existentes
            var estadoActual = _gruposServicios.ToDictionary(g => g.Categoria, g => g.Expandido);

            var grupos = _serviciosFiltrados
                .GroupBy(s => string.IsNullOrEmpty(s.Categoria) ? "General" : s.Categoria)
                .OrderBy(g => g.Key)
                .Select(g => new GrupoServicio
                {
                    Categoria = g.Key,
                    Icono = _iconosServicio.TryGetValue(g.Key, out var ic) ? ic : "TagMultiple",
                    Items = new ObservableCollection<Servicio>(g.OrderBy(s => s.Nombre)),
                    Expandido = estadoActual.TryGetValue(g.Key, out var exp) ? exp : true,
                });

            GruposServicios = new ObservableCollection<GrupoServicio>(grupos);
        }

        private void ActualizarGruposProductos()
        {
            var estadoActual = _gruposProductos.ToDictionary(g => g.Categoria, g => g.Expandido);

            var grupos = _productosFiltrados
                .GroupBy(p => string.IsNullOrEmpty(p.Categoria) ? "General" : p.Categoria)
                .OrderBy(g => g.Key)
                .Select(g => new GrupoProducto
                {
                    Categoria = g.Key,
                    Icono = _iconosProducto.TryGetValue(g.Key, out var ic) ? ic : "PackageVariant",
                    Items = new ObservableCollection<Producto>(g.OrderBy(p => p.Nombre)),
                    Expandido = estadoActual.TryGetValue(g.Key, out var exp) ? exp : true,
                });

            GruposProductos = new ObservableCollection<GrupoProducto>(grupos);
        }

        // ── Operaciones ───────────────────────────────────────────────────────
        private void SeleccionarCliente(Cliente? c)
        {
            if (c == null) return;
            ClienteSeleccionado = c;
            BusquedaCliente = string.Empty;
            SnackbarMessageQueue.Enqueue($"Cliente: {c.Nombre}");
        }

        private void LimpiarCliente()
        {
            ClienteSeleccionado = null;
            ReservaVinculada = null;
        }

        private void AgregarServicio(Servicio? s)
        {
            if (s == null) return;
            var existente = LineasTicket.FirstOrDefault(l => l.Descripcion == s.Nombre && l.Tipo == "Servicio");
            if (existente != null)
                existente.Cantidad++;
            else
                LineasTicket.Add(new LineaTicket { Descripcion = s.Nombre, PrecioUnitario = s.Costo, Tipo = "Servicio" });
            RecalcularTotales();
        }

        private void AgregarProducto(Producto? p)
        {
            if (p == null) return;
            var existente = LineasTicket.FirstOrDefault(l => l.Descripcion == p.Nombre && l.Tipo == "Producto");
            if (existente != null)
                existente.Cantidad++;
            else
                LineasTicket.Add(new LineaTicket { Descripcion = p.Nombre, PrecioUnitario = p.Precio, Tipo = "Producto" });
            RecalcularTotales();
        }

        private void EliminarLinea(LineaTicket? linea)
        {
            if (linea == null) return;
            LineasTicket.Remove(linea);
            RecalcularTotales();
        }

        private void RecalcularTotales()
        {
            Subtotal = LineasTicket.Sum(l => l.Importe);
            DescuentoAplicado = Math.Min(Descuento, Subtotal);
            Total = Subtotal - DescuentoAplicado;
            if (Total < 0) Total = 0;
            TicketVacio = !LineasTicket.Any();
        }

        // ── Cobrar ────────────────────────────────────────────────────────────
        private async Task Cobrar()
        {
            if (ClienteSeleccionado == null)
            { MensajeAdvertencia.Mostrar("Validación", "Selecciona un cliente antes de cobrar."); return; }
            if (!LineasTicket.Any())
            { MensajeAdvertencia.Mostrar("Validación", "El ticket está vacío."); return; }

            try
            {
                Cobrando = true;

                var factura = new Factura
                {
                    ClienteId = ClienteSeleccionado.Id,
                    UsuarioId = SesionUsuario.UsuarioActual?.Id ?? 1,
                    ReservaId = ReservaVinculada?.Id,
                    Fecha = DateTime.Now,
                    Subtotal = Subtotal,
                    Impuesto = Math.Round(Subtotal * 0.21m, 2), 
                    Descuento = DescuentoAplicado,
                    Total = Total,
                    MetodoPago = MetodoPago,
                    Detalle = string.Join(" | ", LineasTicket.Select(l => $"{l.Descripcion} x{l.Cantidad}")),
                    Estado = "Pagada",
                    FechaCreacion = DateTime.Now
                };

                await AddAsync(_facturaRepository, factura);

                if (ReservaVinculada != null)
                {
                    var r = await _reservaRepository.GetByIdAsync(ReservaVinculada.Id);
                    if (r != null) { r.Estado = "Completada"; await UpdateAsync(_reservaRepository, r); }
                }

                MensajeInformacion.Mostrar("✓ Cobro realizado",
                    $"Factura emitida a {ClienteSeleccionado.Nombre}\n" +
                    $"Total: {Total:F2} € · {MetodoPago}");

                LimpiarTodo();
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("Error", $"No se pudo registrar la factura: {ex.Message}");
            }
            finally
            {
                Cobrando = false;
            }
        }

        private void LimpiarTodo()
        {
            LineasTicket.Clear();
            ClienteSeleccionado = null;
            ReservaVinculada = null;
            Descuento = 0;
            RecalcularTotales();
            BusquedaCliente = BusquedaServicio = BusquedaProducto = string.Empty;
        }

        public async Task CargarDesdeReservaAsync(Reserva reserva)
        {
            if (reserva == null) return;

            try
            {
                // Limpiar estado anterior
                LimpiarTodo();

                // Esperar a que las listas estén cargadas (se cargan en el constructor async)
                // Si todavía no hay datos, esperar un tick
                if (!_todosClientes.Any())
                    await Task.Delay(300);

                // 1. Seleccionar el cliente
                var cliente = _todosClientes.FirstOrDefault(c => c.Id == reserva.ClienteId);
                if (cliente != null)
                    SeleccionarCliente(cliente);

                // 2. Añadir el servicio al ticket
                var servicio = _todosServicios.FirstOrDefault(s => s.Id == reserva.ServicioId);
                if (servicio != null)
                    AgregarServicio(servicio);

                // 3. Vincular la reserva
                ReservaVinculada = reserva;

                SnackbarMessageQueue.Enqueue($"Reserva #{reserva.Id} cargada — {cliente?.Nombre ?? "cliente"}");
            }
            catch (Exception ex)
            {
                SnackbarMessageQueue.Enqueue($"Error al cargar reserva: {ex.Message}");
            }
        }

    }
}