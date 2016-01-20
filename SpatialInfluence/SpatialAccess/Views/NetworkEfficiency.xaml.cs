using System.Windows;
using SpatialAccess.ViewModels;

namespace SpatialAccess.Views
{
    /// <summary>
    /// Interaction logic for NetworkEfficiency.xaml
    /// </summary>
    internal partial class NetworkEfficiency : Window
    {
        public NetworkEfficiency(NetworkEfficiencyViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
        }
    }
}
