using ProyectoRuben.Backen.Modelo;
using ProyectoRuben.Backend.Servicios;
using pruebaNavegacion.MVVM;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;

namespace ProyectoRuben.MVVM
{
    public class MVReservas : MVBase
    {
        private readonly IReservaRepository _reservaRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IServicioRepository _servicioRepository;

        // ══════════════════════════════════════════════════════════════════════
        // FECHA SELECCIONADA EN EL CALENDARIO
        // ══════════════════════════════════════════════════════════════════════

        private DateTime _fechaSeleccionada;
        public DateTime FechaSeleccionada
        {
            get => _fechaSeleccionada;
            set
            {
                if (SetProperty(ref _fechaSeleccionada, value))
                {
                    OnPropertyChanged(nameof(FechaSeleccionadaTexto));
                    _ = Inicializa();
                }
            }
        }

        /// <summary>
        /// Texto formateado en español para la cabecera.
        /// Ejemplo: "sábado, 09 de mayo de 2026"
        /// </summary>
        public string FechaSeleccionadaTexto =>
            _fechaSeleccionada == default
                ? string.Empty
                : _fechaSeleccionada.ToString(
                    "dddd, dd 'de' MMMM 'de' yyyy",
                    new CultureInfo("es-ES"));

        // ══════════════════════════════════════════════════════════════════════
        // LISTA DE RESERVAS DEL DÍA
        // ══════════════════════════════════════════════════════════════════════

        private ListCollectionView _listaReservas;
        public ListCollectionView listaReservas
        {
            get => _listaReservas;
            set => SetProperty(ref _listaReservas, value);
        }

        // ══════════════════════════════════════════════════════════════════════
        // CATÁLOGOS PARA EL FORMULARIO
        // ══════════════════════════════════════════════════════════════════════

        private ObservableCollection<Cliente> _listaClientes = new();
        public ObservableCollection<Cliente> ListaClientes
        {
            get => _listaClientes;
            set => SetProperty(ref _listaClientes, value);
        }

        private ObservableCollection<Servicio> _listaServicios = new();
        public ObservableCollection<Servicio> ListaServicios
        {
            get => _listaServicios;
            set => SetProperty(ref _listaServicios, value);
        }

        // ══════════════════════════════════════════════════════════════════════
        // CAMPOS DEL FORMULARIO NUEVA / EDITAR RESERVA
        // ══════════════════════════════════════════════════════════════════════

        private Cliente _clienteSeleccionado;
        public Cliente ClienteSeleccionado
        {
            get => _clienteSeleccionado;
            set => SetProperty(ref _clienteSeleccionado, value);
        }

        private Servicio _servicioSeleccionado;
        public Servicio ServicioSeleccionado
        {
            get => _servicioSeleccionado;
            set => SetProperty(ref _servicioSeleccionado, value);
        }

        private DateTime? _fechaReserva = DateTime.Today;
        public DateTime? FechaReserva
        {
            get => _fechaReserva;
            set => SetProperty(ref _fechaReserva, value);
        }

        private DateTime? _horaReserva;
        public DateTime? HoraReserva
        {
            get => _horaReserva;
            set => SetProperty(ref _horaReserva, value);
        }

        // Campos auxiliares para los TextBox HH y MM
        private string _horaHH = "09";
        public string HoraHH
        {
            get => _horaHH;
            set => SetProperty(ref _horaHH, value);
        }

        private string _horaMM = "00";
        public string HoraMM
        {
            get => _horaMM;
            set => SetProperty(ref _horaMM, value);
        }

        // ══════════════════════════════════════════════════════════════════════
        // EDICIÓN DE RESERVA EXISTENTE
        // ══════════════════════════════════════════════════════════════════════

        private Reserva _reservaEnEdicion;
        /// <summary>
        /// Reserva que se está editando. Null si se está creando una nueva.
        /// </summary>
        public Reserva ReservaEnEdicion
        {
            get => _reservaEnEdicion;
            set => SetProperty(ref _reservaEnEdicion, value);
        }

        // ══════════════════════════════════════════════════════════════════════
        // COMANDOS
        // ══════════════════════════════════════════════════════════════════════

        public ICommand CambiarEstadoCommand { get; }
        public ICommand EliminarCommand { get; }

        // ══════════════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ══════════════════════════════════════════════════════════════════════

        public MVReservas(IReservaRepository reservaRepository,
                          IClienteRepository clienteRepository,
                          IServicioRepository servicioRepository)
        {
            _reservaRepository = reservaRepository;
            _clienteRepository = clienteRepository;
            _servicioRepository = servicioRepository;

            CambiarEstadoCommand = new RelayCommand(CambiarEstadoReserva);
            EliminarCommand = new RelayCommand(async (param) =>
            {
                if (param is int id) await EliminarReserva(id);
                else if (param is Reserva r) await EliminarReserva(r.Id);
            });

            // Asignar el campo directamente (no el setter) para no
            // disparar Inicializa() antes de que los repositorios estén listos
            _fechaSeleccionada = DateTime.Today;

            // Primera carga
            _ = Inicializa();
        }

        // ══════════════════════════════════════════════════════════════════════
        // CARGA PRINCIPAL DE DATOS
        // ══════════════════════════════════════════════════════════════════════

        public async Task Inicializa()
        {
            try
            {
                var clientes = await _clienteRepository.GetAllAsync();
                var servicios = await _servicioRepository.GetAllAsync();

                // Traer todas las reservas a memoria con sus navegaciones incluidas.
                // El proveedor MySQL de EF Core no traduce bien .Date en LINQ,
                // así que filtramos en C# después de materializar.
                var todas = await _reservaRepository
                    .Query(asNoTracking: true,
                           r => r.Cliente,
                           r => r.Servicio,
                           r => r.Empleado)
                    .ToListAsync();

                // Filtrar por el día seleccionado en C# puro
                var reservasDelDia = todas
                    .Where(r => r.Fecha.Date == _fechaSeleccionada.Date)
                    .OrderBy(r => r.Hora)
                    .ToList();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ListaClientes = new ObservableCollection<Cliente>(
                                         clientes.Where(c => c.Activo == true));
                    ListaServicios = new ObservableCollection<Servicio>(
                                         servicios.Where(s => s.Activo == true));
                    listaReservas = new ListCollectionView(
                                         new ObservableCollection<Reserva>(reservasDelDia));
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EXCEPCIÓN en Inicializa: {ex}");
                SnackbarMessageQueue.Enqueue($"Error cargando reservas: {ex.Message}");
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // GUARDAR (CREAR O ACTUALIZAR)
        // ══════════════════════════════════════════════════════════════════════

        public async Task<bool> GuardarReserva()
        {
            // ── Validaciones ──────────────────────────────────────────────────
            if (ClienteSeleccionado == null)
            { SnackbarMessageQueue.Enqueue("Selecciona un cliente."); return false; }
            if (ServicioSeleccionado == null)
            { SnackbarMessageQueue.Enqueue("Selecciona un servicio."); return false; }
            if (FechaReserva == null)
            { SnackbarMessageQueue.Enqueue("Selecciona una fecha."); return false; }
            if (HoraReserva == null)
            { SnackbarMessageQueue.Enqueue("Selecciona una hora."); return false; }

            try
            {
                var fechaFinal = FechaReserva.Value.Date + HoraReserva.Value.TimeOfDay;

                if (ReservaEnEdicion != null)
                {
                    // ── MODO EDICIÓN ──────────────────────────────────────────
                    // GetByIdAsync devuelve una instancia trackeada por EF Core
                    var reservaTracked = await _reservaRepository.GetByIdAsync(ReservaEnEdicion.Id);
                    if (reservaTracked == null)
                    {
                        SnackbarMessageQueue.Enqueue("No se encontró la reserva.");
                        return false;
                    }

                    reservaTracked.ClienteId = ClienteSeleccionado.Id;
                    reservaTracked.ServicioId = ServicioSeleccionado.Id;
                    reservaTracked.Fecha = fechaFinal.Date;
                    reservaTracked.Hora = fechaFinal.TimeOfDay;
                    reservaTracked.FechaModificacion = DateTime.Now;

                    await _reservaRepository.UpdateAsync(reservaTracked);
                    SnackbarMessageQueue.Enqueue("Reserva actualizada correctamente.");
                }
                else
                {
                    // ── MODO CREACIÓN ─────────────────────────────────────────
                    var nueva = new Reserva
                    {
                        ClienteId = ClienteSeleccionado.Id,
                        ServicioId = ServicioSeleccionado.Id,
                        EmpleadoId = 1, // TODO: sustituir por el usuario logueado
                        Fecha = fechaFinal.Date,
                        Hora = fechaFinal.TimeOfDay,
                        Estado = "Pendiente",
                        FechaCreacion = DateTime.Now,
                        FechaModificacion = DateTime.Now
                    };
                    await _reservaRepository.AddAsync(nueva);
                    SnackbarMessageQueue.Enqueue("Reserva guardada correctamente.");
                }

                // Limpiar estado del formulario
                LimpiarFormulario();

                // Refrescar la lista
                await Inicializa();
                return true;
            }
            catch (Exception ex)
            {
                SnackbarMessageQueue.Enqueue($"Error al guardar: {ex.Message}");
                return false;
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ELIMINAR
        // ══════════════════════════════════════════════════════════════════════

        public async Task EliminarReserva(int id)
        {
            try
            {
                await _reservaRepository.RemoveByIdAsync(id);
                SnackbarMessageQueue.Enqueue("Reserva eliminada correctamente.");
                await Inicializa();
            }
            catch (Exception ex)
            {
                SnackbarMessageQueue.Enqueue($"Error al eliminar: {ex.Message}");
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ACCIONES INTERNAS
        // ══════════════════════════════════════════════════════════════════════

        private void CambiarEstadoReserva(object parametro)
        {
            if (parametro is Reserva reserva)
                MessageBox.Show(
                    $"Cambiando estado de reserva ID: {reserva.Id}",
                    "Estado", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Limpia el formulario de nueva/editar reserva tras guardar.
        /// </summary>
        private void LimpiarFormulario()
        {
            ReservaEnEdicion = null;
            ClienteSeleccionado = null;
            ServicioSeleccionado = null;
            FechaReserva = DateTime.Today;
            HoraReserva = null;
            HoraHH = "09";
            HoraMM = "00";
        }
    }
}