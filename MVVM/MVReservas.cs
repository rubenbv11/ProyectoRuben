using ProyectoRuben.Backen.Modelo;
using ProyectoRuben.Backend.Servicios;
using pruebaNavegacion.MVVM;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Linq;

namespace ProyectoRuben.MVVM
{
    public class MVReservas : MVBase
    {
        private readonly IReservaRepository _reservaRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IServicioRepository _servicioRepository;

        // ── Lista de reservas del día ──────────────────────────────────────────
        private DateTime _fechaSeleccionada;
        public DateTime FechaSeleccionada
        {
            get => _fechaSeleccionada;
            set
            {
                if (SetProperty(ref _fechaSeleccionada, value))
                    _ = Inicializa();
            }
        }

        private ListCollectionView _listaReservas;
        public ListCollectionView listaReservas
        {
            get => _listaReservas;
            set => SetProperty(ref _listaReservas, value);
        }

        // ── Datos para el formulario AgregarReserva ───────────────────────────
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

        // ── Comandos ──────────────────────────────────────────────────────────
        public ICommand EditarCommand { get; }
        public ICommand CambiarEstadoCommand { get; }
        public ICommand EliminarCommand { get; }

        public MVReservas(IReservaRepository reservaRepository,
                          IClienteRepository clienteRepository,
                          IServicioRepository servicioRepository)
        {
            _reservaRepository  = reservaRepository;
            _clienteRepository  = clienteRepository;
            _servicioRepository = servicioRepository;

            EditarCommand       = new RelayCommand(EditarReserva);
            CambiarEstadoCommand = new RelayCommand(CambiarEstadoReserva);
            EliminarCommand     = new RelayCommand(async (param) =>
            {
                if (param is int id)       await EliminarReserva(id);
                else if (param is Reserva r) await EliminarReserva(r.Id);
            });

            FechaSeleccionada = DateTime.Today; // dispara Inicializa()
        }

        /// <summary>
        /// Carga las reservas del día seleccionado y los catálogos de clientes/servicios
        /// necesarios para el formulario de nueva reserva.
        /// </summary>
        public async Task Inicializa()
        {
            try
            {
                var reservas  = await _reservaRepository.GetReservasByFechaAsync(FechaSeleccionada);
                var clientes  = await _clienteRepository.FindAsync(c => c.Activo == true);
                var servicios = await _servicioRepository.FindAsync(s => s.Activo == true);

                var listaRes = reservas.ToList();

                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    listaReservas   = new ListCollectionView(listaRes);
                    ListaClientes   = new ObservableCollection<Cliente>(clientes);
                    ListaServicios  = new ObservableCollection<Servicio>(servicios);
                });
            }
            catch (Exception ex)
            {
                SnackbarMessageQueue.Enqueue($"Error cargando datos: {ex.Message}");
            }
        }

        private void EditarReserva(object parametro)
        {
            if (parametro is Reserva reserva)
            {
                MessageBox.Show($"Editando reserva de: {reserva.Cliente?.Nombre}", "Editar", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CambiarEstadoReserva(object parametro)
        {
            if (parametro is Reserva reserva)
            {
                MessageBox.Show($"Cambiando estado de reserva ID: {reserva.Id}", "Estado", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public async Task EliminarReserva(int id)
        {
            try
            {
                await _reservaRepository.RemoveByIdAsync(id);
                await Inicializa();
                SnackbarMessageQueue.Enqueue("Reserva eliminada correctamente.");
            }
            catch (Exception ex)
            {
                SnackbarMessageQueue.Enqueue($"Error al eliminar: {ex.Message}");
            }
        }

        /// <summary>
        /// Construye y guarda una nueva reserva a partir de los campos del formulario.
        /// </summary>
        public async Task<bool> GuardarReserva()
        {
            // Validaciones básicas
            if (ClienteSeleccionado == null)
            {
                SnackbarMessageQueue.Enqueue("Selecciona un cliente.");
                return false;
            }
            if (ServicioSeleccionado == null)
            {
                SnackbarMessageQueue.Enqueue("Selecciona un servicio.");
                return false;
            }
            if (!FechaReserva.HasValue)
            {
                SnackbarMessageQueue.Enqueue("Selecciona una fecha.");
                return false;
            }
            if (!HoraReserva.HasValue)
            {
                SnackbarMessageQueue.Enqueue("Selecciona una hora.");
                return false;
            }

            try
            {
                var reserva = new Reserva
                {
                    ClienteId     = ClienteSeleccionado.Id,
                    ServicioId    = ServicioSeleccionado.Id,
                    EmpleadoId    = 1,  // TODO: obtener del usuario logueado
                    Fecha         = FechaReserva.Value.Date,
                    Hora          = HoraReserva.Value.TimeOfDay,
                    Estado        = "Pendiente",
                    FechaCreacion = DateTime.Now
                };

                await _reservaRepository.AddAsync(reserva);
                SnackbarMessageQueue.Enqueue($"Reserva guardada para {ClienteSeleccionado.Nombre}.");

                // Limpiar selección del formulario para la próxima reserva
                ClienteSeleccionado  = null;
                ServicioSeleccionado = null;
                FechaReserva         = DateTime.Today;
                HoraReserva          = null;

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