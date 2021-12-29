using HeyKodi.Model;
using HeyKodi.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
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
        Mutex mutex = new System.Threading.Mutex(false, "HeyKodiUniqueMutexName");

        public App()
        {
            this.Startup += App_Startup;
            this.Exit += App_Exit;
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            ReleaseMutex();
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            if (mutex.WaitOne(0, false))
            {
                // Run the application

                if (e.Args != null && e.Args.Length == 2 && e.Args[0] == "-delay" && int.TryParse(e.Args[1], out var delay))
                {
                    if (delay >= 0)
                    {
                        if (delay >= 60)
                        {
                            delay = 60;
                        }

                        Thread.Sleep(delay * 1000);                        
                    }
                }
            }
            else
            {
                ReleaseMutex();
                MessageBox.Show("Hey Kodi est déjà lancé.");
                Shutdown(-1);
            }
        }

        private void ReleaseMutex()
        {
            mutex?.Close();
            mutex = null;
        }

        private bool showingError = false;

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (showingError)
            {
                e.Handled = true;
                return;
            }

            e.Handled = !(e.Exception is CriticalApplicationException);

            showingError = true;

            try
            {
                try
                {
                    MainViewModel.Instance.ShowMessage(Messages.ShowMessageType.Error, e.Exception);
                }
                catch
                {
                    MessageBox.Show(e.Exception.ToString());
                }
            }
            finally
            {
                showingError = false;
            }
        }
    }
}
