using System.Windows;

namespace ProyectoRuben.Frontend
{
    public partial class VentanaMensaje : Window
    {
        private MensajeVentana _mensajeVentana;

        // Constructor vacío requerido por XAML
        public VentanaMensaje()
        {
            InitializeComponent();
        }

        public VentanaMensaje(MensajeVentana mensajeVentana)
        {
            InitializeComponent();
            _mensajeVentana = mensajeVentana;
            Loaded += VentanaDialogoMensaje_Loaded;
        }

        private void VentanaDialogoMensaje_Loaded(object sender, RoutedEventArgs e)
        {
            // Ocultamos el icono por defecto si tienes tu propia lógica de imágenes
            // o puedes enlazar la imagen de _mensajeVentana.Imagen a un control Image en tu XAML

            if (txtMensaje != null) txtMensaje.Text = _mensajeVentana.Cuerpo;
            if (txtTitulo != null) txtTitulo.Text = _mensajeVentana.Titulo;

            // Si el botón Aceptar existe en tu nuevo XAML y quieres cambiarle el color dinámicamente:
            // btnAceptar.Background = _mensajeVentana.ColorDistintivo;
        }

        private void BtnAceptar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}