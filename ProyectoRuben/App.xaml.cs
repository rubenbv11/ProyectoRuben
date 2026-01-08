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
        protected override void OnStartup(StartupEventArgs e)
        {
            var splash = new SplashScreen("/Recursos/Imagenes/logo.png");
            splash.Show(false);

            // Tiempo mínimo visible
            System.Threading.Thread.Sleep(2000); // 2 segundos

            splash.Close(TimeSpan.FromSeconds(0.3)); // Animación opcional

            base.OnStartup(e);
        }

    }

}
