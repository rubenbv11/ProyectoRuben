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

        /// <summary>
        /// MainWindow asigna esta acción al inicializar.
        /// Al pulsar "Cobrar" en una reserva, se invoca con el ID
        /// y MainWindow navega a Caja pre-cargando esa reserva.
        /// </summary>
        public Action<Reserva> OnCobrarReserva { get; set; }

        public UCReservas(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
        }

        private MVReservas ViewModel => DataContext as MVReservas;

        // ── Botón "Cobrar" en la fila ─────────────────────────────────────────
        private void BtnCobrar_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Reserva reserva)
                OnCobrarReserva?.Invoke(reserva);
        }

        // ══════════════════════════════════════════════════════════════════════
        // MENÚ CONTEXTUAL
        // ══════════════════════════════════════════════════════════════════════

        private void MenuEditar_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null) return;

            Reserva reserva = null;
            if (sender is MenuItem mi && mi.DataContext is Reserva r)
                reserva = r;
            if (reserva == null) return;

            var dialogo = new AgregarReserva(ViewModel);
            ViewModel.ReservaEnEdicion = reserva;
            ViewModel.FechaReserva = reserva.Fecha;
            ViewModel.HoraHH = reserva.Hora.Hours.ToString("D2");
            ViewModel.HoraMM = reserva.Hora.Minutes.ToString("D2");
            dialogo.ReservaAEditar = reserva;
            dialogo.ShowDialog();

            if (dialogo.DialogResult == true)
                _ = ViewModel.Inicializa();
        }

        private async void MenuEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null) return;

            Reserva reserva = null;
            if (sender is MenuItem mi && mi.DataContext is Reserva r)
                reserva = r;
            if (reserva == null) return;

            var dialogo = new DialogoEliminar(
                $"¿Seguro que deseas eliminar la reserva de {reserva.Cliente?.Nombre ?? "este cliente"}?")
            { Owner = Window.GetWindow(this) };

            if (dialogo.ShowDialog() == true)
                await ViewModel.EliminarReserva(reserva.Id);
        }

        private async void Agregar_Reserva(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null) return;
            ViewModel.ReservaEnEdicion = null;
            var dialogo = new AgregarReserva(ViewModel);
            dialogo.ShowDialog();
            if (dialogo.DialogResult == true)
                await ViewModel.Inicializa();
        }

        private void dgReservas_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel == null) return;
            var reserva = (sender as DataGrid)?.SelectedItem as Reserva;
            if (reserva == null) return;

            var dialogo = new AgregarReserva(ViewModel);
            ViewModel.ReservaEnEdicion = reserva;
            ViewModel.FechaReserva = reserva.Fecha;
            ViewModel.HoraHH = reserva.Hora.Hours.ToString("D2");
            ViewModel.HoraMM = reserva.Hora.Minutes.ToString("D2");
            dialogo.ReservaAEditar = reserva;
            dialogo.ShowDialog();

            if (dialogo.DialogResult == true)
                _ = ViewModel.Inicializa();
        }
    }
}