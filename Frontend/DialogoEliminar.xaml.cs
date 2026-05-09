using System.Windows;

namespace ProyectoRuben.Frontend
{
    public partial class DialogoEliminar : Window
    {
        public DialogoEliminar(string mensaje = "¿Estás seguro que deseas eliminar la reserva?")
        {
            InitializeComponent();
            txtMensaje.Text = mensaje;
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}