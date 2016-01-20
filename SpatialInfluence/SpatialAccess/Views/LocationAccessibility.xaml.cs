using System.Windows;
using SpatialAccess.ViewModels;

namespace SpatialAccess.Views
{
    /// <summary>
    /// Interaction logic for LocationAccessibility.xaml
    /// </summary>
    internal partial class LocationAccessibility : Window
    {
        public LocationAccessibility(LocationAccessibilityViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
        }
    }
}
