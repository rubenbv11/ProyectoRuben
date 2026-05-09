using ProyectoRuben.Backen.Modelo;
using ProyectoRuben.MVVM;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProyectoRuben.Frontend
{
    public partial class AgregarProducto : Window
    {
        private readonly MVProductos _mvProductos;

        /// <summary>
        /// Si se asigna antes de ShowDialog(), el diálogo funciona en modo edición.
        /// </summary>
        public Producto? ProductoAEditar { get; set; }

        public AgregarProducto(MVProductos mvProductos)
        {
            InitializeComponent();
            _mvProductos = mvProductos;
        }

        private void AgregarProducto_Loaded(object sender, RoutedEventArgs e)
        {
            this.AddHandler(Validation.ErrorEvent,
                new RoutedEventHandler(_mvProductos.OnErrorEvent));
            DataContext = _mvProductos;

            // ── Modo edición ──────────────────────────────────────────────────
            if (ProductoAEditar != null)
            {
                TxtTituloVentana.Text = "Editar Producto";
                BtnGuardar.Content    = "Actualizar";

                _mvProductos.ProductoNuevo.Id          = ProductoAEditar.Id;
                _mvProductos.ProductoNuevo.Nombre      = ProductoAEditar.Nombre      ?? string.Empty;
                _mvProductos.ProductoNuevo.Proveedor   = ProductoAEditar.Proveedor   ?? string.Empty;
                _mvProductos.ProductoNuevo.Precio      = ProductoAEditar.Precio;
                _mvProductos.ProductoNuevo.Cantidad    = ProductoAEditar.Cantidad;
                _mvProductos.ProductoNuevo.StockMinimo = ProductoAEditar.StockMinimo ?? 5;
                _mvProductos.ProductoNuevo.StockMaximo = ProductoAEditar.StockMaximo ?? 100;
                _mvProductos.ProductoNuevo.Activo      = true;

                // Forzar refresco visual
                TxtNombre.Text    = _mvProductos.ProductoNuevo.Nombre;
                TxtProveedor.Text = _mvProductos.ProductoNuevo.Proveedor;
                TxtPrecio.Text    = _mvProductos.ProductoNuevo.Precio.ToString("F2");
                TxtCantidad.Text  = _mvProductos.ProductoNuevo.Cantidad.ToString();
                TxtStockMin.Text  = _mvProductos.ProductoNuevo.StockMinimo.ToString();
                TxtStockMax.Text  = _mvProductos.ProductoNuevo.StockMaximo.ToString();
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void SoloNumeros_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

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
            bool exito = ProductoAEditar != null
                ? await _mvProductos.ActualizarProducto()
                : await _mvProductos.GuardarProducto();

            if (exito)
            {
                DialogResult = true;
                Close();
            }
        }
    }
}
