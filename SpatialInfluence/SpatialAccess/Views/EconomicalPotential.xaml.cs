using System.Windows;
using SpatialAccess.ViewModels;

namespace SpatialAccess.Views
{
    /// <summary>
    /// Interaction logic for EconomicalPotential.xaml
    /// </summary>
    internal partial class EconomicalPotential : Window
    {
        public EconomicalPotential(EconomicalPotentialViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
        }
    }
}
