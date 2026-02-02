using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using ProyectoRuben.Backen.Modelo;
using ProyectoRuben.MVVM;
using System.Threading.Tasks;
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
        private readonly MVDashboard _mvDashboard;
        private readonly IServiceProvider _serviceProvider;

        public MainWindow(MVDashboard mVDashboard, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _mvDashboard = mVDashboard;
            _serviceProvider = serviceProvider;
            this.DataContext = _mvDashboard;
            InicializarVentana();

            this.Loaded += MainWindow_Loaded;
        }
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await _mvDashboard.Inicializa();
        }

        private void InicializarVentana()
        {
            // Configurar el timer para actualizar la fecha y hora
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
            txtFechaHora.Text = DateTime.Now.ToString("dddd, dd 'de' MMMM yyyy - HH:mm:ss");

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

        private async void btnDashboard_Click(object sender, RoutedEventArgs e)
        {
            txtTituloPagina.Text = "Dashboard";
            await _mvDashboard.Inicializa();
        }

        private void btnReservas_Click(object sender, RoutedEventArgs e)
        {
            txtTituloPagina.Text = "Gestión de Reservas";

            var vmReservas = _serviceProvider.GetRequiredService<MVReservas>();

            var vistaReservas = new ProyectoRuben.Frontend.UCReservas();
            vistaReservas.DataContext = vmReservas;
            DashboardContent.Children.Clear();
            DashboardContent.Children.Add(vistaReservas);
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
                var loginWindow = _serviceProvider.GetRequiredService<Login>();
                loginWindow.Show();

                // Cerrar esta ventana
                this.Close();
            }
        }

        
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            timer?.Stop();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

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