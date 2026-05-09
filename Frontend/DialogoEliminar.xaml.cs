using System.Windows;

namespace ProyectoRuben.Frontend
{
    public partial class DialogoEliminar : Window
    {
        public DialogoEliminar(string mensaje = "Esta acción no se puede deshacer.")
        {
            InitializeComponent();
            TxtMensaje.Text = mensaje;
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}