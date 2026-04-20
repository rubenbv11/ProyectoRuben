using System.Windows.Controls;
using ProyectoRuben.MVVM; // Importante para que reconozca MVInventario

namespace ProyectoRuben.Frontend
{
    /// <summary>
    /// Lógica de interacción para UCInventario.xaml
    /// </summary>
    public partial class UCInventario : UserControl
    {
        // 1. Añadimos MVInventario dentro de los paréntesis
        public UCInventario(MVInventario viewModel)
        {
            InitializeComponent();

            // 2. Le decimos a la vista que sus datos vienen de este ViewModel
            DataContext = viewModel;
        }
    }
}