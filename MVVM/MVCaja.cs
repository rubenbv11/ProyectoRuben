using di.proyecto.clase._2025.Frontend.Mensajes;
using MaterialDesignThemes.Wpf;
using ProyectoRuben.Backen.Modelo;
using ProyectoRuben.Backend.Servicios;
using pruebaNavegacion.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProyectoRuben.MVVM
{
    /// <summary>
    /// Clase auxiliar que representa una línea de ticket en la caja.
    /// </summary>
    public class LineaTicket : ValidatableViewModel
    {
        private string _descripcion;
        private int _cantidad;
        private decimal _precioUnitario;

        public string Descripcion
        {
            get => _descripcion;
            set => SetProperty(ref _descripcion, value);
        }

        public int Cantidad
        {
            get => _cantidad;
            set
            {
                if (SetProperty(ref _cantidad, value))
                {
                    OnPropertyChanged(nameof(Importe));
                }
            }
        }

        public decimal PrecioUnitario
        {
            get => _precioUnitario;
            set
            {
                if (SetProperty(ref _precioUnitario, value))
                {
                    OnPropertyChanged(nameof(Importe));
                }
            }
        }

        public decimal Importe => Cantidad * PrecioUnitario;
    }

    /// <summary>
    /// ViewModel para la gestión de la Caja/TPV.
    /// Maneja clientes, reservas, tickets y cobros.
    /// </summary>
    public class MVCaja : MVBase
    {
        private readonly IFacturaRepository _facturaRepository;
        private readonly IReservaRepository _reservaRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IServicioRepository _servicioRepository;
        private readonly IProductoRepository _productoRepository;

        private Cliente _clienteSeleccionado;
        private Reserva _reservaVinculada;
        private decimal _subtotal;
        private decimal _descuento;
        private decimal _total;
        private string _metodoPagoSeleccionado = "Efectivo";
        private ObservableCollection<LineaTicket> _lineasTicket;
        private List<Reserva> _reservasDelDia;
        private int _reservaIdBusqueda;

        public Cliente ClienteSeleccionado
        {
            get => _clienteSeleccionado;
            set
            {
                if (SetProperty(ref _clienteSeleccionado, value))
                {
                    // Cuando cambia el cliente, limpiar el ticket
                    if (value != null)
                    {
                        LineasTicket.Clear();
                    }
                }
            }
        }

        public Reserva ReservaVinculada
        {
            get => _reservaVinculada;
            set => SetProperty(ref _reservaVinculada, value);
        }

        public decimal Subtotal
        {
            get => _subtotal;
            set
            {
                if (SetProperty(ref _subtotal, value))
                {
                    RecalcularTotal();
                }
            }
        }

        public decimal Descuento
        {
            get => _descuento;
            set
            {
                if (SetProperty(ref _descuento, value))
                {
                    RecalcularTotal();
                }
            }
        }

        public decimal Total
        {
            get => _total;
            set => SetProperty(ref _total, value);
        }

        public string MetodoPagoSeleccionado
        {
            get => _metodoPagoSeleccionado;
            set => SetProperty(ref _metodoPagoSeleccionado, value);
        }

        public ObservableCollection<LineaTicket> LineasTicket
        {
            get => _lineasTicket;
            set => SetProperty(ref _lineasTicket, value);
        }

        public int ReservaIdBusqueda
        {
            get => _reservaIdBusqueda;
            set => SetProperty(ref _reservaIdBusqueda, value);
        }

        public ICommand CargarReservaCommand { get; }
        public ICommand AgregarExtraCommand { get; }
        public ICommand CobrarCommand { get; }

        public MVCaja(IFacturaRepository facturaRepository, IReservaRepository reservaRepository,
                      IClienteRepository clienteRepository, IServicioRepository servicioRepository,
                      IProductoRepository productoRepository)
        {
            _facturaRepository = facturaRepository ?? throw new ArgumentNullException(nameof(facturaRepository));
            _reservaRepository = reservaRepository ?? throw new ArgumentNullException(nameof(reservaRepository));
            _clienteRepository = clienteRepository ?? throw new ArgumentNullException(nameof(clienteRepository));
            _servicioRepository = servicioRepository ?? throw new ArgumentNullException(nameof(servicioRepository));
            _productoRepository = productoRepository ?? throw new ArgumentNullException(nameof(productoRepository));

            LineasTicket = new ObservableCollection<LineaTicket>();
            _reservasDelDia = new List<Reserva>();

            CargarReservaCommand = new RelayCommand(_ => CargarReserva());
            AgregarExtraCommand = new RelayCommand(_ => AgregarExtra());
            CobrarCommand = new RelayCommand(async _ => await Cobrar());

            _ = CargarReservasDelDia();
        }

        /// <summary>
        /// Carga todas las reservas del día (estado no completado).
        /// </summary>
        private async Task CargarReservasDelDia()
        {
            try
            {
                var todasLasReservas = await GetAllAsync(_reservaRepository);
                var hoy = DateTime.Now.Date;
                _reservasDelDia = todasLasReservas
                    .Where(r => r.Fecha.Date == hoy && r.Estado != "Completada" && r.Estado != "Cancelada")
                    .ToList();
            }
            catch (Exception ex)
            {
                SnackbarMessageQueue.Enqueue($"Error cargando reservas del día: {ex.Message}");
            }
        }

        /// <summary>
        /// Carga una reserva por ID, asigna el cliente, añade el servicio y calcula subtotal.
        /// </summary>
        private async void CargarReserva()
        {
            try
            {
                if (ReservaIdBusqueda <= 0)
                {
                    MensajeAdvertencia.Mostrar("Validación", "Por favor, ingresa un ID de reserva válido.");
                    return;
                }

                var reserva = await GetByIdAsync(_reservaRepository, ReservaIdBusqueda);
                if (reserva == null)
                {
                    MensajeError.Mostrar("Error", "Reserva no encontrada.");
                    return;
                }

                // Cargar cliente vinculado
                ClienteSeleccionado = await GetByIdAsync(_clienteRepository, reserva.ClienteId);
                ReservaVinculada = reserva;

                // Cargar servicio y agregarlo al ticket
                var servicio = await GetByIdAsync(_servicioRepository, reserva.ServicioId);
                if (servicio != null)
                {
                    LineasTicket.Clear();
                    LineasTicket.Add(new LineaTicket
                    {
                        Descripcion = servicio.Nombre,
                        Cantidad = 1,
                        PrecioUnitario = servicio.Costo
                    });

                    Subtotal = LineasTicket.Sum(l => l.Importe);
                }

                SnackbarMessageQueue.Enqueue($"Reserva cargada: {servicio?.Nombre}");
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("Error", $"Error cargando reserva: {ex.Message}");
            }
        }

        /// <summary>
        /// Abre un diálogo para agregar un producto o servicio extra manualmente.
        /// </summary>
        private void AgregarExtra()
        {
            try
            {
                if (ClienteSeleccionado == null)
                {
                    MensajeAdvertencia.Mostrar("Validación", "Por favor, carga o selecciona un cliente primero.");
                    return;
                }

                // Aquí se podría abrir un diálogo para seleccionar producto/servicio
                // Por ahora, usamos un placeholder
                SnackbarMessageQueue.Enqueue("Función de agregar extra disponible próximamente.");
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("Error", $"Error al agregar extra: {ex.Message}");
            }
        }

        /// <summary>
        /// Recalcula el Total automáticamente cuando Subtotal o Descuento cambian.
        /// </summary>
        private void RecalcularTotal()
        {
            Total = Subtotal - Descuento;
            if (Total < 0) Total = 0;
        }

        /// <summary>
        /// Genera y guarda una Factura, actualiza el estado de la Reserva a "Completada"
        /// y limpia la caja para el siguiente cliente.
        /// </summary>
        private async Task Cobrar()
        {
            try
            {
                if (ClienteSeleccionado == null || LineasTicket.Count == 0)
                {
                    MensajeAdvertencia.Mostrar("Validación", "Por favor, carga un cliente y al menos una línea de servicio.");
                    return;
                }

                // Crear entidad Factura
                var factura = new Factura
                {
                    ClienteId = ClienteSeleccionado.Id,
                    UsuarioId = 1, // TODO: Obtener del usuario logueado
                    ReservaId = ReservaVinculada?.Id,
                    Fecha = DateTime.Now,
                    Subtotal = Subtotal,
                    Descuento = Descuento,
                    Total = Total,
                    MetodoPago = MetodoPagoSeleccionado,
                    Detalle = string.Join("\n", LineasTicket.Select(l => $"{l.Descripcion} x{l.Cantidad} = ${l.Importe:F2}")),
                    Estado = "Pagada",
                    FechaCreacion = DateTime.Now
                };

                // Guardar factura
                bool facturaGuardada = await AddAsync(_facturaRepository, factura);
                if (!facturaGuardada)
                {
                    MensajeError.Mostrar("Error", "Error al guardar la factura.");
                    return;
                }

                // Si hay reserva vinculada, cambiar su estado a Completada
                if (ReservaVinculada != null)
                {
                    ReservaVinculada.Estado = "Completada";
                    ReservaVinculada.FechaModificacion = DateTime.Now;
                    await UpdateAsync(_reservaRepository, ReservaVinculada);
                }

                // Notificar éxito
                MensajeInformacion.Mostrar(
                    "Cobro Realizado",
                    $"Factura registrada exitosamente.\nTotal: ${Total:F2}\nMétodo: {MetodoPagoSeleccionado}");

                // Limpiar caja
                LimpiarCaja();
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("Error", $"Error al cobrar: {ex.Message}");
            }
        }

        /// <summary>
        /// Limpia la caja para el siguiente cliente.
        /// </summary>
        private void LimpiarCaja()
        {
            ClienteSeleccionado = null;
            ReservaVinculada = null;
            LineasTicket.Clear();
            Subtotal = 0;
            Descuento = 0;
            Total = 0;
            MetodoPagoSeleccionado = "Efectivo";
            ReservaIdBusqueda = 0;
        }
    }
}
