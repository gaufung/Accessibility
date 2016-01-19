using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ESRI.ArcGIS.Geodatabase;
using SpatialAccess.Models;
using Wintellect.PowerCollections;

namespace SpatialAccess
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            TestOrderedBag();
        }

        private void TestOrderedBag()
        {
            OrderedBag<RasterPositionValue> bag=new OrderedBag<RasterPositionValue>();
            bag.Add(new RasterPositionValue(){XIndex = 1,YIndex = 2,RasterValue = 0});
            bag.Add(new RasterPositionValue() { XIndex = 3, YIndex = 2, RasterValue = 1 });
            bag.Add(new RasterPositionValue() { XIndex = 4, YIndex = 2, RasterValue = -3 });
            bag.Add(new RasterPositionValue() { XIndex = 1, YIndex = 4, RasterValue = 3 });
            var first = bag.RemoveFirst();
            bag.Add(new RasterPositionValue() { XIndex = 1, YIndex = 4, RasterValue = -4 });
            first = bag.RemoveFirst();
        }
    }
}
