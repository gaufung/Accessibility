using System.Windows.Forms;
using ESRI.ArcGIS.Controls;
using GalaSoft.MvvmLight.Messaging;
using SpatialAccess.ViewModels;

namespace SpatialAccess
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    internal partial class MainWindow
    {
        private AxMapControl _mainMapControl;   
        public MainWindow()
        {
            InitializeComponent();
            //TestOrderedBag();
            CreateEngineControls();
            var mainVm = new MainWindowViewModel(_mainMapControl);
            DataContext = mainVm;
            RegisterMessage();
        }

        private void RegisterMessage()
        {
            Messenger.Default.Register<GenericMessage<string>>(this, "Exception", msg =>
            {
                MessageBox.Show("出现异常:" + msg.Content);
            });
            Messenger.Default.Register<GenericMessage<string>>(this, "ArgumentError", msg =>
            {
                MessageBox.Show("无效:" + msg.Content);
            });
            Messenger.Default.Register<GenericMessage<string>>(this, "Message", msg =>
            {
                MessageBox.Show("提示:" + msg.Content);
            });
        }
        private void CreateEngineControls()
        {
            _mainMapControl = new AxMapControl
            {
                Dock = DockStyle.None,
                BackColor = System.Drawing.Color.AliceBlue
            };
            MainFormsHost.Child = _mainMapControl;
        }   

        //private void TestOrderedBag()
        //{
        //    OrderedBag<RasterPositionValue> bag=new OrderedBag<RasterPositionValue>();
        //    bag.Add(new RasterPositionValue(){XIndex = 1,YIndex = 2,RasterValue = 0});
        //    bag.Add(new RasterPositionValue() { XIndex = 3, YIndex = 2, RasterValue = 1 });
        //    bag.Add(new RasterPositionValue() { XIndex = 4, YIndex = 2, RasterValue = -3 });
        //    bag.Add(new RasterPositionValue() { XIndex = 1, YIndex = 4, RasterValue = 3 });
        //    var first = bag.RemoveFirst();
        //    bag.Add(new RasterPositionValue() { XIndex = 1, YIndex = 4, RasterValue = -4 });
        //    first = bag.RemoveFirst();
        //}
    }
}
