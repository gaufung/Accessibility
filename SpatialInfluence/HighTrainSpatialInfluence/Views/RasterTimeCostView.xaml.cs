using HighTrainSpatialInfluence.ViewModels;

namespace HighTrainSpatialInfluence.Views
{
    /// <summary>
    /// Interaction logic for RasterTimeCostView.xaml
    /// </summary>
    internal partial class RasterTimeCostView
    {
        public RasterTimeCostView(RasterTimeCostViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
