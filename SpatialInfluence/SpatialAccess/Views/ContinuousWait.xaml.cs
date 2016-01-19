using System;
using System.Windows;
using System.Windows.Threading;

namespace SpatialAccess.Views
{
    /// <summary>
    /// Interaction logic for ContinuousWait.xaml
    /// </summary>
    public partial class ContinuousWait : Window
    {
        public ContinuousWait(string info="")
        {
            InitializeComponent();
            SetInfoInvoke(info);
        }
        public void SetInfoInvoke(string info)
        {
            //this.LabelInfo.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            //{
            //    this.LabelInfo.Content = info;
            //}));
            this.LabelInfo.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                this.LabelInfo.Content = info;
            }));
        }
        public void CloseWait()
        {
            //this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>Close()));
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => Close()));
        }
    }
}
