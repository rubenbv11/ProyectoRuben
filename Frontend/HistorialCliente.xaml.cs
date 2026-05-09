using ProyectoRuben.Backen.Modelo;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ProyectoRuben.Frontend
{
    // ── Modelo para cada fila del historial ───────────────────────────────────
    public class CitaHistorial
    {
        public string Dia      { get; set; } = string.Empty;
        public string Mes      { get; set; } = string.Empty;
        public string Hora     { get; set; } = string.Empty;
        public string Servicio { get; set; } = string.Empty;
        public string Empleado { get; set; } = string.Empty;
        public string Estado   { get; set; } = string.Empty;

        public Brush ColorEstado => Estado switch
        {
            "Completada"  => new SolidColorBrush(Color.FromRgb(232, 245, 233)),
            "Pendiente"   => new SolidColorBrush(Color.FromRgb(255, 243, 224)),
            "Cancelada"   => new SolidColorBrush(Color.FromRgb(255, 235, 238)),
            "Confirmada"  => new SolidColorBrush(Color.FromRgb(227, 242, 253)),
            _             => new SolidColorBrush(Color.FromRgb(223, 230, 233))
        };

        public Brush TextoEstado => Estado switch
        {
            "Completada"  => new SolidColorBrush(Color.FromRgb(46,  125, 50)),
            "Pendiente"   => new SolidColorBrush(Color.FromRgb(239, 108, 0)),
            "Cancelada"   => new SolidColorBrush(Color.FromRgb(198, 40,  40)),
            "Confirmada"  => new SolidColorBrush(Color.FromRgb(21,  101, 192)),
            _             => new SolidColorBrush(Color.FromRgb(99,  110, 114))
        };
    }

    // ── Ventana ───────────────────────────────────────────────────────────────
    public partial class HistorialCliente : Window
    {
        public HistorialCliente(string nombreCliente, List<Reserva> reservas)
        {
            InitializeComponent();
            CargarDatos(nombreCliente, reservas);
        }

        private void CargarDatos(string nombre, List<Reserva> reservas)
        {
            // Cabecera
            TxtNombreCliente.Text = nombre;
            TxtSubtitulo.Text     = $"{reservas.Count} citas registradas";

            // Stats
            TxtTotalCitas.Text   = reservas.Count.ToString();
            TxtCompletadas.Text  = reservas.Count(r => r.Estado == "Completada").ToString();
            TxtPendientes.Text   = reservas.Count(r => r.Estado is "Pendiente" or "Confirmada").ToString();
            TxtCanceladas.Text   = reservas.Count(r => r.Estado == "Cancelada").ToString();

            if (!reservas.Any())
            {
                PanelVacio.Visibility = Visibility.Visible;
                return;
            }

            // Convertir a modelo de vista
            var cultura = new CultureInfo("es-ES");
            var citas = reservas
                .OrderByDescending(r => r.Fecha)
                .Select(r => new CitaHistorial
                {
                    Dia      = r.Fecha.Day.ToString(),
                    Mes      = r.Fecha.ToString("MMM", cultura).ToUpper(),
                    Hora     = r.Hora.ToString(@"hh\:mm"),
                    Servicio = r.Servicio?.Nombre ?? "Servicio",
                    Empleado = r.Empleado?.Nombre ?? "Sin asignar",
                    Estado   = r.Estado
                })
                .ToList();

            ListaCitas.ItemsSource = citas;
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void Cerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
