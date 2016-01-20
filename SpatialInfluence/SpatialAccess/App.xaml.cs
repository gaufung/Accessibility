using System;
using System.Reflection;
using System.Windows;
using ESRI.ArcGIS;
using ESRI.ArcGIS.esriSystem;
using log4net;


namespace SpatialAccess
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private static log4net.ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private void InitializeEngineLicense()
        {
            try
            {
                AoInitialize aoi = new AoInitializeClass();
                const esriLicenseProductCode productCode = esriLicenseProductCode.esriLicenseProductCodeAdvanced;
                if (aoi.IsProductCodeAvailable(productCode) == esriLicenseStatus.esriLicenseAvailable)
                {
                    aoi.Initialize(productCode);
                }
            }
            catch (Exception e)
            {
                _log.Fatal(e.Message);
            }
           

        }
        protected override void OnStartup(StartupEventArgs e)
        {

            StartupUri = new Uri("MainWindow.xaml", UriKind.RelativeOrAbsolute);
            base.OnStartup(e);
            RuntimeManager.Bind(ProductCode.EngineOrDesktop);
            InitializeEngineLicense();
        }   
    }
}
