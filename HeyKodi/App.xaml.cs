using HeyKodi.Model;
using HeyKodi.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using zComp.Wpf;

namespace HeyKodi
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = !(e.Exception is CriticalApplicationException);

            try
            {
                MainViewModel.Instance.ShowError(e.Exception.Message, e.Exception);
                //zSpeechBalloon.ShowDialogBalloon(zSpeechBalloonIcon.Error, MainWindow?.Title ?? "HeyKodi", e.Exception, zSpeechBalloonButtonsType.Ok);
            }
            catch
            {
                MessageBox.Show(e.Exception.ToString());
            }
        }
    }
}
