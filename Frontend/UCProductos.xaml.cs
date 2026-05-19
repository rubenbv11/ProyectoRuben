using ProyectoRuben.Backen.Modelo;
using ProyectoRuben.MVVM;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ProyectoRuben.Frontend
{
    public partial class UCProductos : UserControl
    {
        public UCProductos()
        {
            InitializeComponent();
        }

        // ── Ajuste de stock inline ────────────────────────────────────────────
        // XAML no puede construir Tuple<Producto,int> directamente,
        // así que los botones usan Click → code-behind → comando del VM.
        // El Tag de cada botón lleva el Producto del DataContext de la card.

        private void BtnMenos_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn
                && btn.Tag is Producto producto
                && DataContext is MVProductos vm)
            {
                vm.AjustarStockCommand.Execute(Tuple.Create(producto, -1));
            }
        }

        private void BtnMas_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn
                && btn.Tag is Producto producto
                && DataContext is MVProductos vm)
            {
                vm.AjustarStockCommand.Execute(Tuple.Create(producto, +1));
            }
        }
    }
}