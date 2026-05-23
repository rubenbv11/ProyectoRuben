using Microsoft.Extensions.DependencyInjection;
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
                    MessageBox.Show("Usuario o clave incorrectos.", "Error de autenticación",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                mainWindow.Show();
                Close();
            }
            else
            {
                MessageBox.Show("Por favor, introduzca usuario y clave.", "Datos incompletos",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void txtOlvidar_Click(object sender, MouseButtonEventArgs e) { }
    }
}