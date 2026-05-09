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

        // ── FechaSeleccionada ─────────────────────────────────────────────────
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
        /// Texto formateado en español para mostrar en la cabecera de reservas.
        /// Ejemplo: "martes, 13 de enero de 2026"
        /// </summary>
        public string FechaSeleccionadaTexto =>
            _fechaSeleccionada == default
                ? string.Empty
                : _fechaSeleccionada.ToString(
                    "dddd, dd 'de' MMMM 'de' yyyy",
                    new CultureInfo("es-ES"));

        // ── Lista de reservas ─────────────────────────────────────────────────
        private ListCollectionView _listaReservas;
        public ListCollectionView listaReservas
        {
            get => _listaReservas;
            set => SetProperty(ref _listaReservas, value);
        }

        // ── Catálogos para AgregarReserva ─────────────────────────────────────
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

        // ── Comandos ──────────────────────────────────────────────────────────
        public ICommand CambiarEstadoCommand { get; }
        public ICommand EliminarCommand { get; }

        // ── Constructor ───────────────────────────────────────────────────────
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

            // Asignar directamente el campo (no el setter) para no
            // disparar Inicializa() antes de que los repositorios estén listos
            _fechaSeleccionada = DateTime.Today;

            // Primera carga
            _ = Inicializa();
        }

        // ── Carga principal ───────────────────────────────────────────────────
        public async Task Inicializa()
        {
            try
            {
                var clientes = await _clienteRepository.GetAllAsync();
                var servicios = await _servicioRepository.GetAllAsync();

                // Traer todo a memoria — el proveedor MySQL de EF Core
                // no traduce bien comparaciones de DATE en todos los casos
                var todas = await _reservaRepository
                    .Query(asNoTracking: true,
                           r => r.Cliente,
                           r => r.Servicio,
                           r => r.Empleado)
                    .ToListAsync();

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
                SnackbarMessageQueue.Enqueue($"Error: {ex.Message}");
            }
        }

        // ── Acciones ──────────────────────────────────────────────────────────
        private void CambiarEstadoReserva(object parametro)
        {
            if (parametro is Reserva reserva)
                MessageBox.Show($"Cambiando estado de reserva ID: {reserva.Id}",
                    "Estado", MessageBoxButton.OK, MessageBoxImage.Information);
        }

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

        public async Task<bool> GuardarReserva()
        {
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

                var nueva = new Reserva
                {
                    ClienteId = ClienteSeleccionado.Id,
                    ServicioId = ServicioSeleccionado.Id,
                    EmpleadoId = 1, // TODO: usuario logueado
                    Fecha = fechaFinal.Date,
                    Hora = fechaFinal.TimeOfDay,
                    Estado = "Pendiente",
                    FechaCreacion = DateTime.Now,
                    FechaModificacion = DateTime.Now
                };

                await _reservaRepository.AddAsync(nueva);
                SnackbarMessageQueue.Enqueue("Reserva guardada correctamente.");
                await Inicializa();
                return true;
            }
            catch (Exception ex)
            {
                SnackbarMessageQueue.Enqueue($"Error al guardar: {ex.Message}");
                return false;
            }
        }
    }
}