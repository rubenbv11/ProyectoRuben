using ProyectoRuben.Backen.Modelo;
using ProyectoRuben.MVVM;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProyectoRuben.Frontend
{
    public partial class UCReservas : UserControl
    {
        private readonly IServiceProvider _serviceProvider;

        public UCReservas(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
        }

        private MVReservas ViewModel => DataContext as MVReservas;

        // ══════════════════════════════════════════════════════════════════════
        // MENÚ CONTEXTUAL — click derecho sobre una fila del DataGrid
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Obtiene la Reserva sobre la que se hizo click derecho.
        /// El DataContext de la fila es la Reserva; lo buscamos subiendo por el árbol visual.
        /// </summary>
        private Reserva ObtenerReservaDelContextMenu(object menuItemSender)
        {
            if (menuItemSender is MenuItem mi &&
                mi.CommandParameter is Reserva r)
                return r;

            // Fallback: leer la fila seleccionada del DataGrid
            var dg = FindDataGrid();
            return dg?.SelectedItem as Reserva;
        }

        private DataGrid FindDataGrid()
        {
            // El DataGrid está dentro de este UserControl; lo buscamos por nombre si lo tiene,
            // o simplemente devolvemos null y el caller usa el item del ContextMenu.
            return null;
        }

        // ── Editar ────────────────────────────────────────────────────────────
        private void MenuEditar_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null) return;

            // Obtener la reserva del DataContext de la fila (viene via CommandParameter o SelectedItem)
            Reserva reserva = null;

            if (sender is MenuItem mi && mi.DataContext is Reserva r)
                reserva = r;

            if (reserva == null) return;

            // Abrir el mismo diálogo de agregar pero con los datos pre-cargados
            var dialogo = new AgregarReserva(ViewModel);

            // Pre-cargar datos de la reserva en el ViewModel
            ViewModel.ReservaEnEdicion = reserva;
            ViewModel.FechaReserva = reserva.Fecha;
            ViewModel.HoraReserva = DateTime.Today + reserva.Hora;
            ViewModel.HoraHH = reserva.Hora.Hours.ToString("D2");
            ViewModel.HoraMM = reserva.Hora.Minutes.ToString("D2");

            // Cliente y servicio se seleccionarán en el diálogo por el usuario
            // (pre-selección requiere que las listas estén cargadas, se hace en Loaded)
            dialogo.ReservaAEditar = reserva;
            dialogo.ShowDialog();

            if (dialogo.DialogResult == true)
                _ = ViewModel.Inicializa();
        }

        // ── Eliminar ──────────────────────────────────────────────────────────
        private async void MenuEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null) return;

            Reserva reserva = null;
            if (sender is MenuItem mi && mi.DataContext is Reserva r)
                reserva = r;

            if (reserva == null) return;

            // Diálogo de confirmación
            var dialogo = new DialogoEliminar(
                $"¿Seguro que deseas eliminar la reserva de {reserva.Cliente?.Nombre ?? "este cliente"}?")
            {
                Owner = Window.GetWindow(this)
            };

            if (dialogo.ShowDialog() == true)
                await ViewModel.EliminarReserva(reserva.Id);
        }

        // ══════════════════════════════════════════════════════════════════════
        // BOTÓN NUEVA RESERVA
        // ══════════════════════════════════════════════════════════════════════
        private async void Agregar_Reserva(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null) return;

            // Limpiar cualquier reserva en edición
            ViewModel.ReservaEnEdicion = null;

            var dialogo = new AgregarReserva(ViewModel);
            dialogo.ShowDialog();

            if (dialogo.DialogResult == true)
                await ViewModel.Inicializa();
        }
    }
}