using System;
using System.Windows;
using System.Windows.Controls;

namespace ProyectoRuben.Frontend
{
    public partial class UCDashboard : UserControl
    {
        /// <summary>
        /// MainWindow asigna esta acción igual que en UCReservas.
        /// Al pulsar Cobrar en una cita del dashboard, navega a Caja
        /// y pre-carga la reserva automáticamente.
        /// </summary>
        public Action<int> OnCobrarCita { get; set; }

        public UCDashboard()
        {
            InitializeComponent();
        }

        private void BtnCobrarCita_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int reservaId)
                OnCobrarCita?.Invoke(reservaId);
        }
    }
}