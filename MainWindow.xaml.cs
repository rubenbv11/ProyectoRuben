using Microsoft.Extensions.DependencyInjection;
using ProyectoRuben.Frontend;
using ProyectoRuben.MVVM;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;

namespace ProyectoRuben
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;
        private readonly MVDashboard _mvDashboard;
        private readonly IServiceProvider _serviceProvider;

        // ── Vistas ────────────────────────────────────────────────────────────
        private readonly UCReservas _uCReservas;
        private readonly UCClientes _uCClientes;
        private readonly UCServicios _uCServicios;
        private readonly UCProductos _uCProductos;
        private readonly UCCaja _uCCaja;
        private readonly UCInventario _ucInventario;
        private readonly UCReportes _ucReportes;
        private readonly UCInformes _ucInformes;    // ← nuevo

        private readonly List<UIElement> _dashboardChildren = new();

        public MainWindow(MVDashboard mvDashboard,
                          IServiceProvider serviceProvider,
                          UCReservas uCReservas,
                          UCClientes uCClientes,
                          UCServicios uCServicios,
                          UCProductos uCProductos,
                          UCCaja uCCaja,
                          UCInventario ucInventario,
                          UCReportes ucReportes,
                          UCInformes ucInformes)   // ← nuevo
        {
            InitializeComponent();
            _mvDashboard = mvDashboard;
            _serviceProvider = serviceProvider;
            _uCReservas = uCReservas;
            _uCClientes = uCClientes;
            _uCServicios = uCServicios;
            _uCProductos = uCProductos;
            _uCCaja = uCCaja;
            _ucInventario = ucInventario;
            _ucReportes = ucReportes;
            _ucInformes = ucInformes;

            DataContext = _mvDashboard;
            InicializarVentana();
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try { await _mvDashboard.Inicializa(); }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al inicializar el dashboard: {ex.Message}",
                    "Error de inicio", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InicializarVentana()
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (_, _) => ActualizarFechaHora();
            _timer.Start();
            ActualizarFechaHora();

            foreach (UIElement child in DashboardContent.Children)
                _dashboardChildren.Add(child);
        }

        private void ActualizarFechaHora() =>
            txtFechaHora.Text = DateTime.Now.ToString("dddd, dd 'de' MMMM yyyy - HH:mm:ss");

        // ══════════════════════════════════════════════════════════════════════
        // NAVEGACIÓN
        // ══════════════════════════════════════════════════════════════════════

        private async void btnDashboard_Click(object sender, RoutedEventArgs e)
        {
            txtTituloPagina.Text = "Dashboard";
            DashboardContent.Children.Clear();
            foreach (var child in _dashboardChildren)
                DashboardContent.Children.Add(child);
            await _mvDashboard.Inicializa();
        }

        private void btnReservas_Click(object sender, RoutedEventArgs e)
        {
            txtTituloPagina.Text = "Gestión de Reservas";
            var vm = _serviceProvider.GetRequiredService<MVReservas>();
            _uCReservas.DataContext = vm;
            DashboardContent.Children.Clear();
            DashboardContent.Children.Add(_uCReservas);
        }

        private void btnClientes_Click(object sender, RoutedEventArgs e)
        {
            txtTituloPagina.Text = "Gestión de Clientes";
            var vm = _serviceProvider.GetRequiredService<MVClientes>();
            _uCClientes.DataContext = vm;
            DashboardContent.Children.Clear();
            DashboardContent.Children.Add(_uCClientes);
        }

        private void btnServicios_Click(object sender, RoutedEventArgs e)
        {
            txtTituloPagina.Text = "Gestión de Servicios";
            var vm = _serviceProvider.GetRequiredService<MVServicios>();
            _uCServicios.DataContext = vm;
            DashboardContent.Children.Clear();
            DashboardContent.Children.Add(_uCServicios);
        }

        private void btnProductos_Click(object sender, RoutedEventArgs e)
        {
            txtTituloPagina.Text = "Gestión de Productos";
            var vm = _serviceProvider.GetRequiredService<MVProductos>();
            _uCProductos.DataContext = vm;
            DashboardContent.Children.Clear();
            DashboardContent.Children.Add(_uCProductos);
        }

        private void btnFacturacion_Click(object sender, RoutedEventArgs e)
        {
            txtTituloPagina.Text = "Caja - Punto de Venta";
            var vm = _serviceProvider.GetRequiredService<MVCaja>();
            _uCCaja.DataContext = vm;
            DashboardContent.Children.Clear();
            DashboardContent.Children.Add(_uCCaja);
        }

        private void btnReportes_Click(object sender, RoutedEventArgs e)
        {
            txtTituloPagina.Text = "Reportes";
            var vm = _serviceProvider.GetRequiredService<MVReportes>();
            _ucReportes.DataContext = vm;
            DashboardContent.Children.Clear();
            DashboardContent.Children.Add(_ucReportes);
        }

        private void btnInformes_Click(object sender, RoutedEventArgs e)
        {
            txtTituloPagina.Text = "Informes y Análisis";
            var vm = _serviceProvider.GetRequiredService<MVInformes>();
            _ucInformes.DataContext = vm;
            DashboardContent.Children.Clear();
            DashboardContent.Children.Add(_ucInformes);
        }

        private void btnInventario_Click(object sender, RoutedEventArgs e)
        {
            txtTituloPagina.Text = "Control de Stock";
            var vm = _serviceProvider.GetRequiredService<MVInventario>();
            _ucInventario.DataContext = vm;
            DashboardContent.Children.Clear();
            DashboardContent.Children.Add(_ucInventario);
        }

        // ══════════════════════════════════════════════════════════════════════
        // CONTROLES DE VENTANA
        // ══════════════════════════════════════════════════════════════════════

        private void btnMinimizar_Click(object sender, RoutedEventArgs e) =>
            WindowState = WindowState.Minimized;

        private void btnMaximizar_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            { WindowState = WindowState.Normal; btnMaximizar.Content = "□"; }
            else
            { WindowState = WindowState.Maximized; btnMaximizar.Content = "❐"; }
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("¿Cerrar la aplicación?", "Confirmar",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                Application.Current.Shutdown();
        }

        private void btnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("¿Cerrar sesión?", "Confirmar",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _timer.Stop();
                // Login es Transient → instancia nueva y limpia
                _serviceProvider.GetRequiredService<Login>().Show();
                Close();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _timer?.Stop();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) { }
    }

    public class CitaViewModel
    {
        public string Hora { get; set; }
        public string Cliente { get; set; }
        public string Servicio { get; set; }
        public string Empleado { get; set; }
        public string Estado { get; set; }
    }
}