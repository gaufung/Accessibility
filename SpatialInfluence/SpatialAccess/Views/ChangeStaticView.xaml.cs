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
    /// Interaction logic for ChangeStaticView.xaml
    /// </summary>
     partial class ChangeStaticView : Window
    {
        public ChangeStaticView(ChangeStaticViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
        }
    }
}
