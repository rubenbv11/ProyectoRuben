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
            // ── BD y Logging ──────────────────────────────────────────────────
            services.AddDbContextFactory<GestioninventarioyserviciosContext>();
            services.AddLogging(configure => configure.AddConsole());

            // ── Repositorios → Singleton ──────────────────────────────────────
            services.AddSingleton(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddSingleton<IUsuarioRepository, UsuarioRepository>();
            services.AddSingleton<IClienteRepository, ClienteRepository>();
            services.AddSingleton<IReservaRepository, ReservaRepository>();
            services.AddSingleton<IFacturaRepository, FacturaRepository>();
            services.AddSingleton<IProductoRepository, ProductoRepository>();
            services.AddSingleton<IServicioRepository, ServicioRepository>();
            services.AddSingleton<IRoleRepository, RoleRepository>();
            services.AddSingleton<IPermisoRepository, PermisoRepository>();
            services.AddSingleton<IRolesPermisosRepository, RolesPermisosRepository>();
            services.AddSingleton<IOfertaRepository, OfertaRepository>();
            services.AddSingleton<IConfiguracionRepository, ConfiguracionRepository>();
            services.AddSingleton<IHorarioRepository, HorarioRepository>();
            services.AddSingleton<IServicioProductoRepository, ServicioProductoRepository>();

            // ── ViewModels → Singleton ────────────────────────────────────────
            services.AddSingleton<MVDashboard>();
            services.AddSingleton<MVUsuario>();
            services.AddSingleton<MVReservas>();
            services.AddSingleton<MVClientes>();
            services.AddSingleton<MVServicios>();
            services.AddSingleton<MVProductos>();
            services.AddSingleton<MVCaja>();
            services.AddSingleton<MVInformes>();
            services.AddSingleton<MVAdministracion>();

            // ── Vistas → Singleton ────────────────────────────────────────────
            services.AddSingleton<MainWindow>();
            services.AddSingleton<UCDashboard>();
            services.AddSingleton<UCReservas>();
            services.AddSingleton<UCClientes>();
            services.AddSingleton<UCServicios>();
            services.AddSingleton<UCProductos>();
            services.AddSingleton<UCCaja>();
            services.AddSingleton<UCInformes>();
            services.AddSingleton<UCAdministracion>();

            // ── Ventanas modales → Transient ──────────────────────────────────
            services.AddTransient<Login>();
            services.AddTransient<AgregarReserva>();
            services.AddTransient<AgregarCliente>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var culture = new CultureInfo("es-ES");

            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    XmlLanguage.GetLanguage(culture.IetfLanguageTag)));

            _serviceProvider.GetRequiredService<Login>().Show();
            base.OnStartup(e);
        }
    }
}