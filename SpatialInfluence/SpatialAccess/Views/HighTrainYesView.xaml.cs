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
using System.Windows.Shapes;
using SpatialAccess.ViewModels;

namespace SpatialAccess.Views
{
    /// <summary>
    /// Interaction logic for HighTrainYesView.xaml
    /// </summary>
    internal partial class HighTrainYesView : Window
    {
        public HighTrainYesView(HighTrainYesViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
        }
    }
}
