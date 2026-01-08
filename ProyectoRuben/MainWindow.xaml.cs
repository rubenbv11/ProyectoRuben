using Microsoft.Extensions.Logging.Abstractions;
using ProyectoRuben.Backen.Modelo;
using System.Windows;
using System.Windows.Threading;

namespace ProyectoRuben
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();
            InicializarVentana();
            CargarDatosDashboard();
        }
        private void InicializarVentana()
        {
            // Configurar el timer para actualizar la fecha y hora
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();

            // Actualizar fecha/hora inmediatamente
            ActualizarFechaHora();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            ActualizarFechaHora();
        }

        private void ActualizarFechaHora()
        {
            txtFechaHora.Text = DateTime.Now.ToString("dddd, dd 'de' MMMM yyyy - HH:mm:ss");
        }

        // ============================================
        // MÉTODOS DE NAVEGACIÓN DEL MENÚ
        // ============================================

        private void btnDashboard_Click(object sender, RoutedEventArgs e)
        {
            txtTituloPagina.Text = "Dashboard";
            CargarDatosDashboard();
        }

        private void btnReservas_Click(object sender, RoutedEventArgs e)
        {
            txtTituloPagina.Text = "Gestión de Reservas";
            MessageBox.Show("Navegando a Reservas...", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnClientes_Click(object sender, RoutedEventArgs e)
        {
            txtTituloPagina.Text = "Gestión de Clientes";
            MessageBox.Show("Navegando a Clientes...", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnServicios_Click(object sender, RoutedEventArgs e)
        {
            txtTituloPagina.Text = "Gestión de Servicios";
            MessageBox.Show("Navegando a Servicios...", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnProductos_Click(object sender, RoutedEventArgs e)
        {
            txtTituloPagina.Text = "Gestión de Productos";
            MessageBox.Show("Navegando a Productos...", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnFacturacion_Click(object sender, RoutedEventArgs e)
        {
            txtTituloPagina.Text = "Facturación";
            MessageBox.Show("Navegando a Facturación...", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnReportes_Click(object sender, RoutedEventArgs e)
        {
            txtTituloPagina.Text = "Reportes y Estadísticas";
            MessageBox.Show("Navegando a Reportes...", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnInventario_Click(object sender, RoutedEventArgs e)
        {
            txtTituloPagina.Text = "Control de Inventario";
            MessageBox.Show("Navegando a Inventario...", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnEmpleados_Click(object sender, RoutedEventArgs e)
        {
            txtTituloPagina.Text = "Gestión de Empleados";
            MessageBox.Show("Navegando a Empleados...", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnConfiguracion_Click(object sender, RoutedEventArgs e)
        {
            txtTituloPagina.Text = "Configuración del Sistema";
            MessageBox.Show("Navegando a Configuración...", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ============================================
        // CONTROLES DE VENTANA
        // ============================================

        private void btnMinimizar_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void btnMaximizar_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
                btnMaximizar.Content = "□";
            }
            else
            {
                this.WindowState = WindowState.Maximized;
                btnMaximizar.Content = "❐";
            }
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "¿Está seguro que desea cerrar la aplicación?",
                "Confirmar salida",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        private void btnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "¿Desea cerrar la sesión actual?",
                "Cerrar sesión",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Detener el timer
                timer.Stop();

                // Abrir ventana de login
                Login loginWindow = new Login();
                loginWindow.Show();

                // Cerrar esta ventana
                this.Close();
            }
        }

        // ============================================
        // CARGA DE DATOS DEL DASHBOARD
        // ============================================

        private void CargarDatosDashboard()
        {
            // Cargar estadísticas
            txtReservasHoy.Text = "12";
            txtClientesAtendidos.Text = "245";
            txtIngresosHoy.Text = "€850,00";
            txtProductosBajoStock.Text = "8";

            // Cargar próximas citas
            CargarProximasCitas();
        }

        private void CargarProximasCitas()
        {
            // Datos de ejemplo
            var proximasCitas = new List<CitaViewModel>
            {
                new CitaViewModel
                {
                    Hora = "09:30",
                    Cliente = "María González López",
                    Servicio = "Corte + Color",
                    Empleado = "Ana López",
                    Estado = "Confirmada"
                },
                new CitaViewModel
                {
                    Hora = "10:30",
                    Cliente = "Carlos Mendoza Ruiz",
                    Servicio = "Corte Caballero",
                    Empleado = "Pedro Ramírez",
                    Estado = "Confirmada"
                },
                new CitaViewModel
                {
                    Hora = "11:00",
                    Cliente = "Laura Jiménez Pérez",
                    Servicio = "Manicura Permanente",
                    Empleado = "Sofía Castro",
                    Estado = "Pendiente"
                },
                new CitaViewModel
                {
                    Hora = "12:00",
                    Cliente = "Jorge Herrera Sánchez",
                    Servicio = "Tratamiento Keratina",
                    Empleado = "Ana López",
                    Estado = "Confirmada"
                },
                new CitaViewModel
                {
                    Hora = "14:00",
                    Cliente = "Elena Ruiz Moreno",
                    Servicio = "Mechas Californianas",
                    Empleado = "Ana López",
                    Estado = "Confirmada"
                },
                new CitaViewModel
                {
                    Hora = "16:00",
                    Cliente = "David Sánchez Díaz",
                    Servicio = "Corte + Barba",
                    Empleado = "Pedro Ramírez",
                    Estado = "Pendiente"
                }
            };

            dgProximasCitas.ItemsSource = proximasCitas;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            timer?.Stop();
        }
    }

    // ============================================
    // VIEWMODEL PARA LAS CITAS
    // ============================================

    public class CitaViewModel
    {
        public string Hora { get; set; }
        public string Cliente { get; set; }
        public string Servicio { get; set; }
        public string Empleado { get; set; }
        public string Estado { get; set; }
    }
}