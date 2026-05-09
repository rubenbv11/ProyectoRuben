using ProyectoRuben.MVVM;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProyectoRuben.Frontend
{
    public partial class AgregarReserva : Window
    {
        private MVReservas _mVReservas;

        public AgregarReserva(MVReservas mVReservas)
        {
            InitializeComponent();
            _mVReservas = mVReservas;
        }

        private void AgregarReserva_Loaded(object sender, RoutedEventArgs e)
        {
            this.AddHandler(Validation.ErrorEvent,
                new RoutedEventHandler(_mVReservas.OnErrorEvent));
            DataContext = _mVReservas;

            // Mostrar todos los clientes al abrir
            ListaClientesFiltrada.ItemsSource = _mVReservas.ListaClientes;
        }

        // ── Arrastrar ventana sin borde ───────────────────────────────────────
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        // ── Búsqueda de cliente en tiempo real ────────────────────────────────
        private void TxtBuscarCliente_TextChanged(object sender, TextChangedEventArgs e)
        {
            var filtro = TxtBuscarCliente.Text.Trim();

            if (string.IsNullOrEmpty(filtro))
            {
                ListaClientesFiltrada.ItemsSource = _mVReservas.ListaClientes;
            }
            else
            {
                var filtrados = _mVReservas.ListaClientes
                    .Where(c => c.Nombre.Contains(filtro,
                                StringComparison.OrdinalIgnoreCase))
                    .ToList();
                ListaClientesFiltrada.ItemsSource = filtrados;
            }

            // Si hay cliente seleccionado y el texto cambió, limpiar selección
            if (_mVReservas.ClienteSeleccionado != null &&
                !_mVReservas.ClienteSeleccionado.Nombre.Contains(filtro,
                    StringComparison.OrdinalIgnoreCase))
            {
                _mVReservas.ClienteSeleccionado = null;
                OcultarChip();
            }
        }

        // ── Al seleccionar cliente de la lista ────────────────────────────────
        private void ListaClientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListaClientesFiltrada.SelectedItem is ProyectoRuben.Backen.Modelo.Cliente c)
            {
                _mVReservas.ClienteSeleccionado = c;
                MostrarChip(c.Nombre);
                TxtBuscarCliente.Text = string.Empty;
                ListaClientesFiltrada.ItemsSource = null; // ocultar lista
            }
        }

        // ── Chip: mostrar cliente seleccionado ────────────────────────────────
        private void MostrarChip(string nombre)
        {
            ChipCliente.Visibility = Visibility.Visible;
            ListaClientesFiltrada.Visibility = Visibility.Collapsed;
            TxtNombreClienteSeleccionado.Text = nombre;
            TxtInicialCliente.Text = nombre.Length > 0
                ? nombre[0].ToString().ToUpper()
                : "?";
        }

        private void OcultarChip()
        {
            ChipCliente.Visibility = Visibility.Collapsed;
            ListaClientesFiltrada.Visibility = Visibility.Visible;
            ListaClientesFiltrada.ItemsSource = _mVReservas.ListaClientes;
            ListaClientesFiltrada.SelectedItem = null;
        }

        // ── Botón × del chip: limpiar cliente ────────────────────────────────
        private void LimpiarCliente_Click(object sender, RoutedEventArgs e)
        {
            _mVReservas.ClienteSeleccionado = null;
            TxtBuscarCliente.Text = string.Empty;
            OcultarChip();
            TxtBuscarCliente.Focus();
        }

        // ── Validar que solo entren números en HH y MM ────────────────────────
        private void SoloNumeros_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        // ── Botones principales ───────────────────────────────────────────────
        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void Guardar_Click(object sender, RoutedEventArgs e)
        {
            // Validar HH y MM antes de pasar al ViewModel
            if (!int.TryParse(TxtHora.Text, out int hh) || hh < 0 || hh > 23)
            {
                MessageBox.Show("Introduce una hora válida (0–23).",
                    "Hora incorrecta", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtHora.Focus();
                return;
            }
            if (!int.TryParse(TxtMinutos.Text, out int mm) || mm < 0 || mm > 59)
            {
                MessageBox.Show("Introduce los minutos correctamente (0–59).",
                    "Minutos incorrectos", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtMinutos.Focus();
                return;
            }

            // Construir el DateTime de hora y asignarlo al ViewModel
            _mVReservas.HoraReserva = DateTime.Today.AddHours(hh).AddMinutes(mm);

            bool exito = await _mVReservas.GuardarReserva();
            if (exito)
            {
                DialogResult = true;
                Close();
            }
        }
    }
}