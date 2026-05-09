using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProyectoRuben.Backen.Modelo;
using ProyectoRuben.Backend.Servicios;
using ProyectoRuben.Frontend;
using ProyectoRuben.MVVM;
using pruebaNavegacion.Backend.Servicios;
using System;
using System.Windows;
using System.Globalization;
using System.Threading;
using System.Windows.Markup;

namespace ProyectoRuben
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IServiceProvider _serviceProvider;

        public App()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // --- A. Contexto de Base de Datos y Logging ---
            services.AddDbContextFactory<GestioninventarioyserviciosContext>();
            services.AddLogging(configure => configure.AddConsole());

            // --- B+C. Repositorios: Transient para que cada ViewModel tenga su propio contexto ---
            services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddTransient<IUsuarioRepository, UsuarioRepository>();
            services.AddTransient<IClienteRepository, ClienteRepository>();
            services.AddTransient<IReservaRepository, ReservaRepository>();
            services.AddTransient<IFacturaRepository, FacturaRepository>();
            services.AddTransient<IProductoRepository, ProductoRepository>();
            services.AddTransient<IServicioRepository, ServicioRepository>();
            services.AddTransient<IRoleRepository, RoleRepository>();
            services.AddTransient<IPermisoRepository, PermisoRepository>();
            services.AddTransient<IRolesPermisosRepository, RolesPermisosRepository>();
            services.AddTransient<IOfertaRepository, OfertaRepository>();
            services.AddTransient<IConfiguracionRepository, ConfiguracionRepository>();
            services.AddTransient<IHorarioRepository, HorarioRepository>();
            services.AddTransient<IServicioProductoRepository, ServicioProductoRepository>();

            // --- D. ViewModels ---
            services.AddTransient<MVDashboard>();
            services.AddTransient<MVUsuario>();
            services.AddTransient<MVReservas>();
            services.AddTransient<MVClientes>();
            services.AddTransient<MVServicios>();
            services.AddTransient<MVProductos>();
            services.AddTransient<MVCaja>();
            services.AddTransient<MVInventario>();
            services.AddTransient<MVReportes>();

            // --- E. Ventanas (Vistas) ---
            services.AddTransient<MainWindow>();
            services.AddTransient<Login>();
            services.AddTransient<UCReservas>();
            services.AddTransient<UCClientes>();
            services.AddTransient<UCServicios>();
            services.AddTransient<UCProductos>();
            services.AddTransient<UCCaja>();
            services.AddTransient<UCInventario>();
            services.AddTransient<UCReportes>();
            services.AddTransient<AgregarReserva>();
            services.AddTransient<AgregarCliente>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var culture = new System.Globalization.CultureInfo("es-ES");

            // 1. Esto fuerza el español en TODOS los hilos de .NET 8 (es el truco clave)
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = culture;
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = culture;

            // 2. Fuerzas el hilo actual
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;

            // 3. Fuerzas la interfaz gráfica de WPF
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(System.Windows.Markup.XmlLanguage.GetLanguage(culture.IetfLanguageTag)));

            var loginWindow = _serviceProvider.GetRequiredService<Login>();
            loginWindow.Show();
            base.OnStartup(e);
        }
    }
}
