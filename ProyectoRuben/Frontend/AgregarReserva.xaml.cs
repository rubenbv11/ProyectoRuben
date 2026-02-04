using ProyectoRuben.Backen.Modelo;
using ProyectoRuben.Backend.Servicios;
using ProyectoRuben.MVVM;
using System;
using System.Linq; // Importante para el ToList()
using System.Windows;
using System.Windows.Controls;

namespace ProyectoRuben.Frontend
{
    public partial class AgregarReserva : Window
    {
        private MVReservas _mVReservas;
         

        public AgregarReserva(MVReservas mVReservas)
        {
            InitializeComponent();
            _mVReservas = mVReservas;
        }

        private async void AgregarReserva_Loaded(object sender, RoutedEventArgs e)
        {
            await _mVReservas.Inicializa();
            this.AddHandler(Validation.ErrorEvent, new RoutedEventHandler(_mVReservas.OnErrorEvent));
            DataContext = _mVReservas;
        }
        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        private async void Guardar_Click(object sender, RoutedEventArgs e)
        {
            await _mVReservas.GuardarReserva();
            DialogResult = true;
            Close();
        }

    }
}