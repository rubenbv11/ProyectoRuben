using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProyectoRuben.Backen.Modelo; 
using ProyectoRuben.Backend.Servicios;
using ProyectoRuben.Frontend;
using ProyectoRuben.MVVM; 
using pruebaNavegacion.Backend.Servicios; 
using System;
using System.Windows;

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
            services.AddDbContext<GestioninventarioyserviciosContext>();
            services.AddLogging(configure => configure.AddConsole());

            // --- B. Repositorios Genéricos ---
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // --- C. Repositorios Específicos (Interfaz -> Implementación) ---
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<IClienteRepository, ClienteRepository>();
            services.AddScoped<IReservaRepository, ReservaRepository>();
            services.AddScoped<IFacturaRepository, FacturaRepository>();
            services.AddScoped<IProductoRepository, ProductoRepository>();
            services.AddScoped<IServicioRepository, ServicioRepository>();
           services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IPermisoRepository, PermisoRepository>();
            services.AddScoped<IRolesPermisosRepository, RolesPermisosRepository>();
            services.AddScoped<IOfertaRepository, OfertaRepository>();
            services.AddScoped<IConfiguracionRepository, ConfiguracionRepository>();
            services.AddScoped<IHorarioRepository, HorarioRepository>();
            services.AddScoped<IServicioProductoRepository, ServicioProductoRepository>();

            // --- D. ViewModels ---
            services.AddTransient<MVDashboard>();
            services.AddTransient<MVUsuario>();
            services.AddTransient<MVReservas>();

            // --- E. Ventanas (Vistas) ---
            services.AddTransient<MainWindow>(); 
            services.AddTransient<Login>();   
            services.AddTransient<UCReservas>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var loginWindow = _serviceProvider.GetRequiredService<Login>();
            loginWindow.Show();
        }
    }
}