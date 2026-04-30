using Castle.Core.Logging;
using di.proyecto.clase._2025.Frontend.Mensajes;
using MahApps.Metro.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProyectoRuben.Backen.Modelo;
using pruebaNavegacion.Backend.Servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ProyectoRuben
{
    /// <summary>
    /// Lógica de interacción para Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        private IUsuarioRepository _usuarioRepository;
        private readonly IServiceProvider _serviceProvider;
        private readonly MainWindow _ventanaPrincipal;
        public Login(IUsuarioRepository usuarioRepository, IServiceProvider serviceProvider,MainWindow ventanaPrincipal)
        {
            InitializeComponent();
            _usuarioRepository = usuarioRepository;
            _serviceProvider = serviceProvider;
            _ventanaPrincipal = ventanaPrincipal;
        }

      

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtUsuario.Text) && !string.IsNullOrEmpty(txtPassword.Password))
            {
                bool isAuthenticated = await _usuarioRepository.LoginAsync(txtUsuario.Text, txtPassword.Password);
                if (!isAuthenticated)
                {
                    MensajeError.Mostrar("Error de autenticación", "Usuario o clave incorrectos.", 3);
                    return;
                }
                else
                {
                    MensajeInformacion.Mostrar("Acceso correcto", "Bienvenido.", 2);

                    _ventanaPrincipal.Show();
                    this.Close();
                }

            }
            else
            {
                MensajeAdvertencia.Mostrar("Datos incompletos", "Por favor, introduzca usuario y clave.", 3);
            }
        }

        private void txtOlvidar_Click(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
