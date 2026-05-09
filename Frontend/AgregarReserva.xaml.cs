using ProyectoRuben.Backen.Modelo;
using ProyectoRuben.MVVM;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProyectoRuben.Frontend
{
    public partial class AgregarReserva : Window
    {
        private MVReservas _mVReservas;

        /// <summary>
        /// Si se asigna antes de ShowDialog(), el diálogo pre-carga sus datos (modo edición).
        /// </summary>
        public Reserva? ReservaAEditar { get; set; }

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

            // Fuente inicial: todos los clientes y servicios
            ListaClientesFiltrada.ItemsSource = _mVReservas.ListaClientes;
            ListaServiciosFiltrada.ItemsSource = _mVReservas.ListaServicios;

            // ── Pre-cargar si estamos en modo edición ──────────────────────
            if (ReservaAEditar != null)
            {
                // Título de la ventana
                this.Title = "Editar Reserva";

                // Pre-seleccionar cliente
                var cliente = _mVReservas.ListaClientes
                    .FirstOrDefault(c => c.Id == ReservaAEditar.ClienteId);
                if (cliente != null)
                {
                    _mVReservas.ClienteSeleccionado = cliente;
                    MostrarChipCliente(cliente.Nombre);
                }

                // Pre-seleccionar servicio
                var servicio = _mVReservas.ListaServicios
                    .FirstOrDefault(s => s.Id == ReservaAEditar.ServicioId);
                if (servicio != null)
                {
                    _mVReservas.ServicioSeleccionado = servicio;
                    MostrarChipServicio(servicio);
                }
            }
        }

        // ── Arrastrar ventana ─────────────────────────────────────────────────
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        // ══════════════════════════════════════════════════════════════════════
        // CLIENTE
        // ══════════════════════════════════════════════════════════════════════

        private void TxtBuscarCliente_TextChanged(object sender, TextChangedEventArgs e)
        {
            var filtro = TxtBuscarCliente.Text.Trim();

            ListaClientesFiltrada.ItemsSource = string.IsNullOrEmpty(filtro)
                ? _mVReservas.ListaClientes
                : _mVReservas.ListaClientes
                    .Where(c => c.Nombre.Contains(filtro, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            if (_mVReservas.ClienteSeleccionado != null &&
                !_mVReservas.ClienteSeleccionado.Nombre.Contains(filtro, StringComparison.OrdinalIgnoreCase))
            {
                _mVReservas.ClienteSeleccionado = null;
                OcultarChipCliente();
            }
        }

        private void ListaClientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListaClientesFiltrada.SelectedItem is Cliente c)
            {
                _mVReservas.ClienteSeleccionado = c;
                MostrarChipCliente(c.Nombre);
                TxtBuscarCliente.Text = string.Empty;
                ListaClientesFiltrada.ItemsSource = null;
                BorderListaClientes.Visibility = Visibility.Collapsed;
            }
        }

        private void MostrarChipCliente(string nombre)
        {
            ChipCliente.Visibility = Visibility.Visible;
            BorderListaClientes.Visibility = Visibility.Collapsed;
            TxtNombreClienteSeleccionado.Text = nombre;
            TxtInicialCliente.Text = nombre.Length > 0 ? nombre[0].ToString().ToUpper() : "?";
        }

        private void OcultarChipCliente()
        {
            ChipCliente.Visibility = Visibility.Collapsed;
            BorderListaClientes.Visibility = Visibility.Visible;
            ListaClientesFiltrada.ItemsSource = _mVReservas.ListaClientes;
            ListaClientesFiltrada.SelectedItem = null;
        }

        private void LimpiarCliente_Click(object sender, RoutedEventArgs e)
        {
            _mVReservas.ClienteSeleccionado = null;
            TxtBuscarCliente.Text = string.Empty;
            OcultarChipCliente();
            TxtBuscarCliente.Focus();
        }

        // ══════════════════════════════════════════════════════════════════════
        // SERVICIO
        // ══════════════════════════════════════════════════════════════════════

        private void TxtBuscarServicio_TextChanged(object sender, TextChangedEventArgs e)
        {
            var filtro = TxtBuscarServicio.Text.Trim();

            ListaServiciosFiltrada.ItemsSource = string.IsNullOrEmpty(filtro)
                ? _mVReservas.ListaServicios
                : _mVReservas.ListaServicios
                    .Where(s => s.Nombre.Contains(filtro, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            if (_mVReservas.ServicioSeleccionado != null &&
                !_mVReservas.ServicioSeleccionado.Nombre.Contains(filtro, StringComparison.OrdinalIgnoreCase))
            {
                _mVReservas.ServicioSeleccionado = null;
                OcultarChipServicio();
            }
        }

        private void ListaServicios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListaServiciosFiltrada.SelectedItem is Servicio s)
            {
                _mVReservas.ServicioSeleccionado = s;
                MostrarChipServicio(s);
                TxtBuscarServicio.Text = string.Empty;
                ListaServiciosFiltrada.ItemsSource = null;
                BorderListaServicios.Visibility = Visibility.Collapsed;
            }
        }

        private void MostrarChipServicio(Servicio s)
        {
            ChipServicio.Visibility = Visibility.Visible;
            BorderListaServicios.Visibility = Visibility.Collapsed;
            TxtNombreServicioSeleccionado.Text = s.Nombre;
            TxtDetalleServicioSeleccionado.Text = $"{s.Duracion} min · {s.Costo:F2} €";
        }

        private void OcultarChipServicio()
        {
            ChipServicio.Visibility = Visibility.Collapsed;
            BorderListaServicios.Visibility = Visibility.Visible;
            ListaServiciosFiltrada.ItemsSource = _mVReservas.ListaServicios;
            ListaServiciosFiltrada.SelectedItem = null;
        }

        private void LimpiarServicio_Click(object sender, RoutedEventArgs e)
        {
            _mVReservas.ServicioSeleccionado = null;
            TxtBuscarServicio.Text = string.Empty;
            OcultarChipServicio();
            TxtBuscarServicio.Focus();
        }

        // ══════════════════════════════════════════════════════════════════════
        // VALIDACIÓN Y GUARDADO
        // ══════════════════════════════════════════════════════════════════════

        private void SoloNumeros_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            _mVReservas.ReservaEnEdicion = null; // limpiar si se cancela edición
            DialogResult = false;
            Close();
        }

        private async void Guardar_Click(object sender, RoutedEventArgs e)
        {
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