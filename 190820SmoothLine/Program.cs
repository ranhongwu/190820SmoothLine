using ESRI.ArcGIS.esriSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _190820SmoothLine
{
    static class Program
    {
        private static LicenseInitializer m_AOLicenseInitializer = new _190820SmoothLine.LicenseInitializer();

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ESRI.ArcGIS.RuntimeManager.Bind(ESRI.ArcGIS.ProductCode.Desktop);
            ESRI.ArcGIS.RuntimeManager.BindLicense(ESRI.ArcGIS.ProductCode.EngineOrDesktop);
            IAoInitialize m_aoinitialize = new AoInitializeClass();
            m_aoinitialize.Initialize(esriLicenseProductCode.esriLicenseProductCodeEngine);
            Application.Run(new Form1());
        }
    }
}
