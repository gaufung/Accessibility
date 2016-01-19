using System;
using System.Windows;
using ESRI.ArcGIS;
using ESRI.ArcGIS.esriSystem;

namespace SpatialAccess
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void InitializeEngineLicense()
        {
            AoInitialize aoi = new AoInitializeClass();
            const esriLicenseProductCode productCode = esriLicenseProductCode.esriLicenseProductCodeAdvanced;
            if (aoi.IsProductCodeAvailable(productCode) == esriLicenseStatus.esriLicenseAvailable)
            {
                aoi.Initialize(productCode);
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
