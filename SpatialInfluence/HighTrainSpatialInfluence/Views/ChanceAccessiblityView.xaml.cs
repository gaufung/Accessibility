using System.Windows;
using HighTrainSpatialInfluence.ViewModels;

namespace HighTrainSpatialInfluence.Views
{
    /// <summary>
    /// Interaction logic for ChanceAccessiblityView.xaml
    /// </summary>
    internal partial class ChanceAccessiblityView : Window
    {
        public ChanceAccessiblityView(ChanceAccessiblityViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
        }
    }
}
