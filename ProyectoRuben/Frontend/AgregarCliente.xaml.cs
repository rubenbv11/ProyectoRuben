using ProyectoRuben.MVVM;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ProyectoRuben.Frontend
{
    public partial class AgregarCliente : Window
    {
        private MVClientes _mvClientes;

        public AgregarCliente(MVClientes mvClientes)
        {
            InitializeComponent();
            _mvClientes = mvClientes;
        }

        private void AgregarCliente_Loaded(object sender, RoutedEventArgs e)
        {
            this.AddHandler(Validation.ErrorEvent, new RoutedEventHandler(_mvClientes.OnErrorEvent));
            DataContext = _mvClientes;
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void Guardar_Click(object sender, RoutedEventArgs e)
        {
            await _mvClientes.GuardarCliente();
            DialogResult = true;
            Close();
        }
    }
}
