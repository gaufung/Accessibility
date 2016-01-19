using System.Windows;
using SpatialAccess.ViewModels;

namespace SpatialAccess.Views
{
    /// <summary>
    /// Interaction logic for HighTrainNoView.xaml
    /// </summary>
    internal partial class HighTrainNoView : Window
    {
        public HighTrainNoView(HighTrainNoViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
        }
    }
}
