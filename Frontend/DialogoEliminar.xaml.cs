using System.Windows;
using System.Windows.Controls;

namespace ProyectoRuben.Frontend.Dialogos
{
    public partial class DialogoEliminar : Window
    {
        public bool Confirmado { get; private set; } = false;

        public DialogoEliminar()
        {
            InitializeComponent();
        }

        private void txtConfirmacion_TextChanged(object sender, TextChangedEventArgs e)
        {
            string texto = txtConfirmacion.Text.Trim().ToLower();
            btnEliminar.IsEnabled = texto == "eliminar";
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            Confirmado = true;
            Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}