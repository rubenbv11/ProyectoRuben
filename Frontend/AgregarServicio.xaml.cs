using ProyectoRuben.MVVM;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProyectoRuben.Frontend
{
    public partial class AgregarServicio : Window
    {
        private readonly MVServicios _mvServicios;

        /// <summary>
        /// Si se asigna antes de ShowDialog(), el diálogo funciona en modo edición.
        /// </summary>
        public ProyectoRuben.Backen.Modelo.Servicio? ServicioAEditar { get; set; }

        public AgregarServicio(MVServicios mvServicios)
        {
            InitializeComponent();
            _mvServicios = mvServicios;
        }

        private void AgregarServicio_Loaded(object sender, RoutedEventArgs e)
        {
            this.AddHandler(Validation.ErrorEvent,
                new RoutedEventHandler(_mvServicios.OnErrorEvent));
            DataContext = _mvServicios;

            // ── Modo edición ──────────────────────────────────────────────────
            if (ServicioAEditar != null)
            {
                TxtTituloVentana.Text = "Editar Servicio";
                BtnGuardar.Content    = "Actualizar";

                _mvServicios.ServicioNuevo.Id          = ServicioAEditar.Id;
                _mvServicios.ServicioNuevo.Nombre      = ServicioAEditar.Nombre      ?? string.Empty;
                _mvServicios.ServicioNuevo.Descripcion = ServicioAEditar.Descripcion ?? string.Empty;
                _mvServicios.ServicioNuevo.Duracion    = ServicioAEditar.Duracion;
                _mvServicios.ServicioNuevo.Costo       = ServicioAEditar.Costo;
                _mvServicios.ServicioNuevo.Activo      = true;

                // Forzar refresco de TextBox
                TxtNombre.Text      = _mvServicios.ServicioNuevo.Nombre;
                TxtDescripcion.Text = _mvServicios.ServicioNuevo.Descripcion;
                TxtDuracion.Text    = _mvServicios.ServicioNuevo.Duracion.ToString();
                TxtPrecio.Text      = _mvServicios.ServicioNuevo.Costo.ToString("F2");
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        // Solo números enteros para duración
        private void SoloNumeros_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        // Números decimales para precio (permite coma y punto)
        private void SoloDecimales_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var tb    = sender as TextBox;
            var texto = (tb?.Text ?? string.Empty) + e.Text;
            e.Handled = !decimal.TryParse(
                texto.Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out _);
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void Guardar_Click(object sender, RoutedEventArgs e)
        {
            bool exito;

            if (ServicioAEditar != null)
                exito = await _mvServicios.ActualizarServicio();
            else
                exito = await _mvServicios.GuardarServicio();

            if (exito)
            {
                DialogResult = true;
                Close();
            }
        }
    }
}
