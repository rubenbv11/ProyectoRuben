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

        private DateTime _fechaSeleccionada;
        public DateTime FechaSeleccionada
        {
            get => _fechaSeleccionada;
            set
            {
                if (SetProperty(ref _fechaSeleccionada, value))
                {
                    _ = Inicializa();
                }
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

        {

            CambiarEstadoCommand = new RelayCommand(CambiarEstadoReserva);
            EliminarCommand     = new RelayCommand(async (param) =>
            {
                if (param is int id)       await EliminarReserva(id);
                else if (param is Reserva r) await EliminarReserva(r.Id);
            });

        }

        /// <summary>
        /// Carga las reservas del día seleccionado y los catálogos de clientes/servicios
        /// necesarios para el formulario de nueva reserva.
        /// </summary>
        public async Task Inicializa()
        {
            try
            {
            }
            catch (Exception ex)
            {
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

        {
                SnackbarMessageQueue.Enqueue("Selecciona una hora.");
                return false;
            }

            try
            {
            }
            catch (Exception ex)
            {
                SnackbarMessageQueue.Enqueue($"Error al guardar: {ex.Message}");
                return false;
            }
        }

    }
}