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
        private AgregarReserva _agregarReserva;
        private readonly IServiceProvider _serviceProvider;
        private MVReservas _mvReservas;

        public UCReservas(AgregarReserva agregarReserva,IServiceProvider serviceProvider,MVReservas mVReservas)
        {
            InitializeComponent();
            _agregarReserva = agregarReserva;
            _serviceProvider = serviceProvider;
            _mvReservas = mVReservas;
            DataContext = _mvReservas;
        }
        private async void MenuEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item && item.DataContext is Reserva reserva)
            {
                var dialogo = new ProyectoRuben.Frontend.Dialogos.DialogoEliminar();
                dialogo.Owner = Window.GetWindow(this);

                if (dialogo.ShowDialog() == true)
                {
                    if (DataContext is MVReservas vm)
                    {
                        await vm.EliminarReserva(reserva.Id);
                    }
                }
            }
        }

        private async void Agregar_Reserva(object sender, RoutedEventArgs e)
        {
            _agregarReserva = _serviceProvider.GetRequiredService<AgregarReserva>();
            _agregarReserva.ShowDialog();
            if(_agregarReserva.DialogResult == true)
            {
                _mvReservas.listaReservas.Refresh();
            }
        }
    }
}
