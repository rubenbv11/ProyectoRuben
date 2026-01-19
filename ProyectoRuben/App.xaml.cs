using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProyectoRuben.Backen.Modelo;
using ProyectoRuben.Backend.Servicios;
using pruebaNavegacion.Backend.Servicios;
using System.Configuration;
using System.Data;
using System.Windows;

namespace ProyectoRuben
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private IServiceProvider _serviceProvider;

        private GestioninventarioyserviciosContext _contexto;

        public App()
        {
           var serviceCollection = new ServiceCollection();
       
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
            _contexto = new GestioninventarioyserviciosContext();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // Contexto de la base de datos
            services.AddDbContext<GestioninventarioyserviciosContext>();
            // Servicios de logging
            services.AddLogging(configure => configure.AddConsole());
            // Repositorios Genericos
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            // Ventada Principal
            services.AddSingleton<MainWindow>();
            // Repositorios Específicos
            services.AddScoped<IGenericRepository<Usuario>, UsuarioRepository>();
            services.AddScoped<IGenericRepository<Producto>, ProductoRepository>();
            services.AddScoped<IGenericRepository<Servicio>, ServicioRepository>();
            services.AddScoped<IGenericRepository<Cliente>, ClienteRepository>();
            services.AddScoped<IGenericRepository<Reserva>, ReservaRepository>();
            services.AddScoped<IGenericRepository<Factura>, FacturaRepository>();
            services.AddScoped<IGenericRepository<Role>, RoleRepository>();
            services.AddScoped<IGenericRepository<Permiso>, PermisoRepository>();
            services.AddScoped<IGenericRepository<RolesPermisos>, RolesPermisosRepository>();
            services.AddScoped<IGenericRepository<Oferta>, OfertaRepository>();
            services.AddScoped<IGenericRepository<Configuracion>, ConfiguracionRepository>();
            services.AddScoped<IGenericRepository<Horario>, HorarioRepository>();
            services.AddScoped<IGenericRepository<ServicioProducto>, ServicioProductoRepository>();

            // Servicios especificos
            services.AddScoped<UsuarioRepository>();
            services.AddScoped<ProductoRepository>();
            services.AddScoped<ServicioRepository>();
            services.AddScoped<ClienteRepository>();
            services.AddScoped<ReservaRepository>();
            services.AddScoped<FacturaRepository>();
            services.AddScoped<RoleRepository>();
            services.AddScoped<PermisoRepository>();
            services.AddScoped<RolesPermisosRepository>();
            services.AddScoped<OfertaRepository>();
            services.AddScoped<ConfiguracionRepository>();
            services.AddScoped<HorarioRepository>();
            services.AddScoped<ServicioProductoRepository>();

            // Ventanas
            services.AddTransient<Login>();
            services.AddTransient<MainWindow>();

        }





        protected override void OnStartup(StartupEventArgs e)
        {
            var splash = new SplashScreen("/Recursos/Imagenes/logo.png");
            splash.Show(false);

            // Tiempo mínimo visible
            System.Threading.Thread.Sleep(2000); // 2 segundos

            splash.Close(TimeSpan.FromSeconds(0.3)); // Animación opcional


            var loginWindow = _serviceProvider.GetService<Login>();
            loginWindow.Show();

            base.OnStartup(e);
        }

    }
}

