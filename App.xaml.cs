using Nancy.TinyIoc;
using Sentry;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace RfReader_demo
{
    public partial class App : Application
    {        
        protected override void OnStartup(StartupEventArgs e)
        {
            SplashScreen splashScreen = new SplashScreen("logo-bar-rupali-edit.jpg");
            splashScreen.Show(true);            
            string dsnKey = ConfigurationManager.ConnectionStrings["DSN"].ConnectionString;
            if (!string.IsNullOrEmpty(dsnKey))
            {
                SentrySdk.Init(dsnKey);
            }
            else { MessageBox.Show("Sentry Key not Found", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning); }
        }
    }
}
