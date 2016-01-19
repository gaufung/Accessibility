using SpatialAccess.ViewModels;

namespace SpatialAccess.Views
{
    /// <summary>
    /// Interaction logic for RasterTimeCostView.xaml
    /// </summary>
    internal partial class RasterTimeCostView
    {
        public RasterTimeCostView(RasterTimeCostViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
        }
    }
}
