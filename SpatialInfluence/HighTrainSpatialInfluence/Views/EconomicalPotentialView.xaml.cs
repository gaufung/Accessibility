using System.Windows;
using HighTrainSpatialInfluence.ViewModels;

namespace HighTrainSpatialInfluence.Views
{
    /// <summary>
    /// Interaction logic for EconomicalPotentialView.xaml
    /// </summary>
    internal partial class EconomicalPotentialView : Window
    {
        public EconomicalPotentialView(EconomicalPotentialViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
        }
    }
}
