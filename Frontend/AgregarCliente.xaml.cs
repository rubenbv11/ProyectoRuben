using ProyectoRuben.MVVM;
using System;
using System.Windows;
using System.Windows.Controls;   // ← necesario para Validation
using System.Windows.Input;

namespace ProyectoRuben.Frontend
{
    public partial class AgregarCliente : Window
    {
        private readonly MVClientes _mvClientes;

        /// <summary>
        /// Si se asigna antes de ShowDialog(), el diálogo funciona en modo edición.
        /// </summary>
        public ClienteViewModel? ClienteAEditar { get; set; }

        public AgregarCliente(MVClientes mvClientes)
        {
            InitializeComponent();
            _mvClientes = mvClientes;
        }

        private void AgregarCliente_Loaded(object sender, RoutedEventArgs e)
        {
            this.AddHandler(Validation.ErrorEvent,
                new RoutedEventHandler(_mvClientes.OnErrorEvent));
            DataContext = _mvClientes;

            // ── Modo edición: pre-cargar datos ────────────────────────────────
            if (ClienteAEditar != null)
            {
                TxtTituloVentana.Text = "Editar Cliente";
                BtnGuardar.Content = "Actualizar";

                // Volcar datos del cliente en ClienteNuevo para que los bindings funcionen
                _mvClientes.ClienteNuevo.Id = ClienteAEditar.Id;
                _mvClientes.ClienteNuevo.Nombre = ClienteAEditar.Nombre ?? string.Empty;
                _mvClientes.ClienteNuevo.Telefono = ClienteAEditar.Telefono ?? string.Empty;
                _mvClientes.ClienteNuevo.Email = ClienteAEditar.Email ?? string.Empty;
                _mvClientes.ClienteNuevo.Contacto = ClienteAEditar.Nombre ?? string.Empty;
                _mvClientes.ClienteNuevo.Activo = true;

                // Forzar refresco visual de los TextBox
                // (el binding puede no actualizarse si ClienteNuevo ya tenía valores vacíos)
                TxtNombre.Text = _mvClientes.ClienteNuevo.Nombre;
                TxtTelefono.Text = _mvClientes.ClienteNuevo.Telefono;
                TxtEmail.Text = _mvClientes.ClienteNuevo.Email;
            }
        }

        // ── Arrastrar ventana sin borde ───────────────────────────────────────
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void Guardar_Click(object sender, RoutedEventArgs e)
        {
            bool exito;

            if (ClienteAEditar != null)
                exito = await _mvClientes.ActualizarCliente();
            else
                exito = await _mvClientes.GuardarCliente();

            if (exito)
            {
                DialogResult = true;
                Close();
            }
        }
    }
}