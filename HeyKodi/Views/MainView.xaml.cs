using HeyKodi.Messages;
using HeyKodi.Model;
using HeyKodi.ViewModels;
using KodiRPC.RPC.RequestResponse.Params.VideoLibrary;
using KodiRPC.RPC.Specifications;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using zComp.Core;
using zComp.Core.Helpers;
using zComp.Wpf;
using zComp.Wpf.Helpers;

namespace HeyKodi.Views
{
    /// <summary>
    /// Logique d'interaction pour MainView.xaml
    /// </summary>
    public partial class MainView : UserControl, IReadOnlyElement
    {
        private static MainView instance;

        public static MainView Instance { get { return instance ?? (instance = new MainView()); } }

        private void ActivateMainWindow()
        {
            if (Application.Current?.MainWindow != null && Application.Current.MainWindow.IsVisible)
            {
                Application.Current.MainWindow.Activate();
            }
        }

        public MainView()
        {
            InitializeComponent();

            if (instance != null)
            {
                throw new Exception($"{nameof(MainView)} must be a singleton");
            }

            RegisterMessagesResponses();

            this.Loaded += MainView_Loaded;
            Application.Current.MainWindow.Closing += MainWindow_Closing;
            Application.Current.MainWindow.Closed += MainWindow_Closed;
        }

        private void MainView_Loaded(object sender, RoutedEventArgs e)
        {
            this.MainViewModel = MainViewModel.Instance;

            this.MainViewModel.Init();
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            MainViewModel?.Cleanup();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MainViewModel != null && zSpeechBalloon.ShowDialogBalloon(zSpeechBalloonIcon.Question, MainViewModel.Title,
                "Etes-vous certain de vouloir quitter Hey Kodi ?", zSpeechBalloonButtonsType.YesNo) == zSpeechBalloonDialogResult.No)
            {
                e.Cancel = true;
            }
        }

        private bool messagesResponsesRegistred = false;

        private void RegisterMessagesResponses()
        {
            if (messagesResponsesRegistred)
            {
                return;
            }

            messagesResponsesRegistred = true;

            // Réponse au message d'affichage d'un message d'information
            GalaSoft.MvvmLight.Messaging.Messenger.Default.Register<ShowMessageMsg>
            (
                Application.Current,
                msg =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        object title = msg.Title ?? MainViewModel.Title;

                        var messageType = zSpeechBalloonIcon.Information;
                        var message = msg.Exception == null ? msg.Message : 
                            (MainViewModel?.HeyKodiConfig?.DebugMode == true ? (object)msg.Exception : (object)msg.Exception.ToMessageRecursive());
                        var buttons = zSpeechBalloonButtonsType.Ok;

                        switch (msg.MessageType)
                        {
                            case ShowMessageType.Warning:
                                messageType = zSpeechBalloonIcon.Warning;
                                break;
                            case ShowMessageType.Error:
                                messageType = zSpeechBalloonIcon.Error;
                                break;
                            case ShowMessageType.Question:
                                messageType = zSpeechBalloonIcon.Question;
                                buttons = zSpeechBalloonButtonsType.YesNo;
                                break;
                        }

                        var dialogResult = zSpeechBalloon.ShowDialogBalloon(messageType, title, message, buttons);

                        msg.Result = dialogResult == zSpeechBalloonDialogResult.Yes || dialogResult == zSpeechBalloonDialogResult.Ok;
                    });
                }
            );

            GalaSoft.MvvmLight.Messaging.Messenger.Default.Register<PlaySoundMsg>
            (
                Application.Current,
                msg =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ActivateMainWindow();

                        if (MainViewModel.HeyKodiConfig.Volume > 0.05)
                        {
                            MainViewModel.KodiSpeechRecognizer.RecognizeAsyncCancel();

                            if (MainViewModel.HeyKodiConfig.UseSpeechSynthesizer &&
                                !string.IsNullOrWhiteSpace(msg.Speech) && MainViewModel.SpeechSynthesizer != null)
                            {
                                MainViewModel.SpeechSynthesizer.Volume = (int)(MainViewModel.HeyKodiConfig.Volume * 100.0);
                                MainViewModel.SpeechSynthesizer.SpeakAsync(msg.Speech);
                            }
                            else
                            {
                                player.Volume = MainViewModel.HeyKodiConfig.Volume * 0.20;
                                player.Source = new Uri(msg.SoundSource);
                            }
                        }
                    });
                }
            );


            GalaSoft.MvvmLight.Messaging.Messenger.Default.Register<ShowConfigurationMsg>
            (
                Application.Current,
                msg =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        zSpeechBalloon.ShowDialogBalloon(zSpeechBalloonIcon.Information, HeyKodi.Properties.Resources.CONFIGVIEW_TITLE, ConfigView.Instance, zSpeechBalloonButtonsType.Ok);
                    });
                }
            );

            GalaSoft.MvvmLight.Messaging.Messenger.Default.Register<MinimizeHeyKodiMsg>
            (
                Application.Current,
                msg =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (Application.Current?.MainWindow != null && Application.Current.MainWindow.IsVisible)
                        {
                            Application.Current.MainWindow.WindowState = WindowState.Minimized;
                        }
                    });
                }
            );

            GalaSoft.MvvmLight.Messaging.Messenger.Default.Register<ActivateHeyKodiMsg>
            (
                Application.Current,
                msg =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (Application.Current?.MainWindow != null && Application.Current.MainWindow.IsVisible)
                        {
                            Application.Current.MainWindow.WindowState = WindowState.Normal;
                            ActivateMainWindow();
                        }
                    });
                }
            );
        }

        private void player_MediaEnded(object sender, RoutedEventArgs e)
        {
            MainViewModel.KodiSpeechRecognizer.RecognizeAsync();
        }

        private void btClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }



        private MainViewModel mainViewModel;

        public MainViewModel MainViewModel
        {
            get
            {
                return mainViewModel;
            }
            set
            {
                DataContext = mainViewModel = value;
            }
        }

        #region IReadOnlyElement Membres

        /// <summary>Etat de lecture seule</summary>
        private zReadOnlyMode readOnly = zReadOnlyMode.ParentReadOnly;

        /// <summary>Peut-on modifier le contrôle (cliquer dessus)</summary>
        /// <returns>true si on peut modifier le contrôle</returns>
        public virtual bool CanModify()
        {
            return this.GetCanModify();
        }

        /// <summary>etat de lecture seule</summary>
        public zReadOnlyMode ReadOnly
        {
            get
            {
                return readOnly;
            }
            set
            {
                readOnly = value;
            }
        }

        #endregion
    }
}
