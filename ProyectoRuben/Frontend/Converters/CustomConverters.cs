using System;
using System.Globalization;
using System.Windows.Data;

namespace ProyectoRuben.Frontend.Converters
{
    /// <summary>
    /// Convierte una cadena a su primer carácter (inicial).
    /// Usado para mostrar iniciales en avatares.
    /// </summary>
    public class StringToFirstCharConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && !string.IsNullOrEmpty(str))
            {
                return str.Substring(0, 1).ToUpper();
            }
            return "?";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Invierte un valor booleano de visibilidad.
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                // Si parameter es "Invert", invertir la lógica
                bool invert = parameter != null && parameter.ToString() == "Invert";
                bool result = invert ? !b : b;
                return result ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
            return System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Detecta si el stock está bajo comparando cantidad vs stock mínimo.
    /// Usado para mostrar alertas visuales en productos.
    /// </summary>
    public class StockBajoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Este converter se usa para detectar si stock <= StockMinimo
            // Como no tenemos acceso directo a StockMinimo en el XAML DataTrigger,
            // asumimos que el ViewModel debe exponer esta información.
            // Por ahora, retorna false (se actualizará en el ViewModel)
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
