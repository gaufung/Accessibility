using System;
using System.Windows;
using System.Windows.Threading;

namespace HighTrainSpatialInfluence.Views
{
    /// <summary>
    /// Interaction logic for Wait.xaml
    /// </summary>
    public partial class Wait : Window
    {
        public Wait()
        {
            InitializeComponent();
            InitControls();
        }
        public Wait(string title)
        {
            InitializeComponent();
            InitControls(title);
        }
        private void InitControls(string caption = " ")
        {
            labelCaption.Content = caption;
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.Value = 0;
        }
        /// <summary>
        /// 设置等待窗体的标题
        /// </summary>
        /// <param name="caption">标题名称</param>
        public void SetWaitCaption(string caption)
        {
            labelCaption.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                labelCaption.Content = caption;
            }));
        }
        /// <summary>
        /// 设置等待窗体进度条
        /// </summary>
        /// <param name="progress"></param>
        public void SetProgress(double progress)
        {
            const double scaleFactor = 100;
            progressBar.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                progressBar.Value = progress * scaleFactor;
            }));
        }
        /// <summary>
        /// 关闭等待窗口
        /// </summary>
        public void CloseWait()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(Close));
        }
    }
}
