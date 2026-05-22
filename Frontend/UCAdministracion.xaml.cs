using ProyectoRuben.MVVM;
using System.Windows.Controls;

namespace ProyectoRuben.Frontend
{
    public partial class UCAdministracion : UserControl
    {
        public UCAdministracion()
        {
            InitializeComponent();
        }

        private void RbEmpleado_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is MVAdministracion vm)
                vm.NuevoRol = "Empleado";
        }

        private void RbAdmin_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is MVAdministracion vm)
                vm.NuevoRol = "Administrador";
        }
    }
}