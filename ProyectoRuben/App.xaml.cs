using Microsoft.Extensions.DependencyInjection;
using ProyectoRuben.Backen.Modelo;
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

