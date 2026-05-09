using Microsoft.Extensions.DependencyInjection;
using ProyectoRuben.Backen.Modelo;
using ProyectoRuben.Backend.Servicios;
using ProyectoRuben.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProyectoRuben.Frontend
{
    /// <summary>
    /// Interaction logic for UCReservas.xaml
    /// </summary>
    public partial class UCReservas : UserControl
    {
        private readonly IServiceProvider _serviceProvider;

        // MainWindow es quien gestiona el ViewModel que se inyecta vía DataContext.
        public UCReservas(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
        }

        private MVReservas ViewModel => DataContext as MVReservas;

        private async void MenuEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item && item.DataContext is Reserva reserva)
            {
                var dialogo = new DialogoEliminar
                {
                    Owner = Window.GetWindow(this)
                };

                if (dialogo.ShowDialog() == true && ViewModel != null)
                    await ViewModel.EliminarReserva(reserva.Id);
            }
        }

        private async void Agregar_Reserva(object sender, RoutedEventArgs e)
        {
            // Pasamos el ViewModel activo (el del DataContext) al diálogo
            if (ViewModel == null) return;

            var dialogo = new AgregarReserva(ViewModel);
            dialogo.ShowDialog();

            if (dialogo.DialogResult == true)
                await ViewModel.Inicializa(); // Refrescar desde el VM correcto
        }
    }
}
