using Microsoft.Extensions.DependencyInjection;
using ProyectoRuben.Backend.Servicios;
using ProyectoRuben.Frontend;
using pruebaNavegacion.Backend.Servicios;
using System;
using System.Windows;
using System.Windows.Input;

namespace ProyectoRuben
{
    public partial class Login : Window
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IServiceProvider _serviceProvider;

        public Login(IUsuarioRepository usuarioRepository, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _usuarioRepository = usuarioRepository;
            _serviceProvider = serviceProvider;
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtUsuario.Text) && !string.IsNullOrEmpty(txtPassword.Password))
            {
                bool isAuthenticated = await _usuarioRepository.LoginAsync(
                    txtUsuario.Text, txtPassword.Password);

                if (!isAuthenticated)
                {
                    MensajeError.Mostrar("Error de autenticación", "Usuario o clave incorrectos.", 3);
                    return;
                }

                var nombre = SesionUsuario.UsuarioActual?.Nombre ?? "Usuario";
                MensajeInformacion.Mostrar("Acceso correcto", $"Bienvenido, {nombre}.", 2);

                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                mainWindow.Show();
                Close();
            }
            else
            {
                MensajeAdvertencia.Mostrar("Datos incompletos", "Por favor, introduzca usuario y clave.", 3);
            }
        }

        private void txtOlvidar_Click(object sender, MouseButtonEventArgs e) { }
    }
}