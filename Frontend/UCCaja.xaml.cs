using ProyectoRuben.MVVM;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace ProyectoRuben.Frontend
{
    public partial class UCCaja : UserControl
    {
        public UCCaja()
        {
            InitializeComponent();
        }
    }

    /// <summary>
    /// Clase auxiliar para proporcionar los items del método de pago en el ComboBox.
    /// </summary>
    public static class MetodoPagoItems
    {
        public static List<string> Items { get; } = new List<string>
        {
            "Efectivo",
            "Tarjeta",
            "Transferencia",
            "Mixto"
        };
    }
}
