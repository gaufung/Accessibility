using System.Windows;
using HighTrainSpatialInfluence.ViewModels;

namespace HighTrainSpatialInfluence.Views
{
    /// <summary>
    /// Interaction logic for LocationAccessiblityView.xaml
    /// </summary>
    internal partial class LocationAccessiblityView : Window
    {
        public LocationAccessiblityView(LocationAccessiblityViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
        }
    }
}
