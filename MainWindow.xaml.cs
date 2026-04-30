using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using ProyectoRuben.Backen.Modelo;
using ProyectoRuben.Frontend;
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
        private readonly UCReservas _uCReservas;
        private readonly UCClientes _uCClientes;
        private readonly UCServicios _uCServicios;
        private readonly UCProductos _uCProductos;
        private readonly UCCaja _uCCaja;
        private readonly UCInventario _ucInventario;
        private readonly UCReportes _ucReportes;

        public MainWindow(MVDashboard mVDashboard, IServiceProvider serviceProvider, UCReservas uCReservas, UCClientes uCClientes, UCServicios uCServicios, UCProductos uCProductos, UCCaja uCCaja, UCInventario uCInventario, UCReportes uCReportes)
        {
            InitializeComponent();
            _mvDashboard = mVDashboard;
            _serviceProvider = serviceProvider;
            this.DataContext = _mvDashboard;
            _uCReservas = uCReservas;
            _uCClientes = uCClientes;
            _uCServicios = uCServicios;
            _uCProductos = uCProductos;
            _uCCaja = uCCaja;
            _ucInventario = uCInventario;
            _ucReportes = uCReportes;
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


            _uCReservas.DataContext = vmReservas;
            DashboardContent.Children.Clear();
            DashboardContent.Children.Add(_uCReservas);
        }

        private void btnClientes_Click(object sender, RoutedEventArgs e)
        {
            txtTituloPagina.Text = "Gestión de Clientes";
            var vmClientes = _serviceProvider.GetRequiredService<MVClientes>();
            _uCClientes.DataContext = vmClientes;
            DashboardContent.Children.Clear();
            DashboardContent.Children.Add(_uCClientes);
        }

        private void btnServicios_Click(object sender, RoutedEventArgs e)
        {
            txtTituloPagina.Text = "Gestión de Servicios";
            var vmServicios = _serviceProvider.GetRequiredService<MVServicios>();
            _uCServicios.DataContext = vmServicios;
            DashboardContent.Children.Clear();
            DashboardContent.Children.Add(_uCServicios);
        }

        private void btnProductos_Click(object sender, RoutedEventArgs e)
        {
            txtTituloPagina.Text = "Gestión de Productos";
            var vmProductos = _serviceProvider.GetRequiredService<MVProductos>();
            _uCProductos.DataContext = vmProductos;
            DashboardContent.Children.Clear();
            DashboardContent.Children.Add(_uCProductos);
        }

        private void btnFacturacion_Click(object sender, RoutedEventArgs e)
        {
            txtTituloPagina.Text = "Caja - Punto de Venta";
            var vmCaja = _serviceProvider.GetRequiredService<MVCaja>();
            _uCCaja.DataContext = vmCaja;
            DashboardContent.Children.Clear();
            DashboardContent.Children.Add(_uCCaja);
        }

        private void btnReportes_Click(object sender, RoutedEventArgs e)
        {
            txtTituloPagina.Text = "Gestión de Reportes";
            var vmReportes = _serviceProvider.GetRequiredService<MVReportes>();
            _ucReportes.DataContext = vmReportes;
            DashboardContent.Children.Clear();
            DashboardContent.Children.Add(_ucReportes);
        }

        private void btnInventario_Click(object sender, RoutedEventArgs e)
        {
            txtTituloPagina.Text = "Gestión de Stock";
            var vmInventario = _serviceProvider.GetRequiredService<MVInventario>();
            _ucReportes.DataContext = vmInventario;
            DashboardContent.Children.Clear();
            DashboardContent.Children.Add(_ucInventario);
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
