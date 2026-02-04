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

        private Reserva _reserva;

        private DateTime _fechaSeleccionada;
        public DateTime FechaSeleccionada
        {
            get => _fechaSeleccionada;
            set
            {
                if (SetProperty(ref _fechaSeleccionada, value))
                {
                    _ = CargarReservas();
                }
            }
        }

        private ListCollectionView _listaReservas;
        public ListCollectionView listaReservas
        {
            get => _listaReservas;
            set => SetProperty(ref _listaReservas, value);
        }

        public ICommand EditarCommand { get; }
        public ICommand CambiarEstadoCommand { get; }
        public ICommand EliminarCommand { get; }

        public MVReservas(IReservaRepository reservaRepository)
        {
            _reservaRepository = reservaRepository;
            FechaSeleccionada = DateTime.Today;
            _reserva = new Reserva();

            EditarCommand = new RelayCommand(EditarReserva);
            CambiarEstadoCommand = new RelayCommand(CambiarEstadoReserva);

            // Adaptado para aceptar la llamada asíncrona y el cast a int
            EliminarCommand = new RelayCommand(async (param) => await EliminarReserva((int)param));
        }

        public async Task CargarReservas()
        {
            try
            {
                var reservas = await _reservaRepository.GetReservasByFechaAsync(FechaSeleccionada);
                listaReservas = new ListCollectionView(reservas.ToList());
            }
            catch (Exception ex)
            {
                SnackbarMessageQueue.Enqueue($"Error cargando reservas: {ex.Message}");
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
                await CargarReservas();
                SnackbarMessageQueue.Enqueue("Reserva eliminada correctamente.");
            }
            catch (Exception ex)
            {
                SnackbarMessageQueue.Enqueue($"Error al eliminar: {ex.Message}");
            }
        }
    }
}