using HeyKodi.Messages;
using HeyKodi.Model;
using HeyKodi.ViewModels;
using KodiRPC.RPC.RequestResponse.Params.VideoLibrary;
using KodiRPC.RPC.Specifications;
using System;
using System.Windows;
using System.Windows.Controls;
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

            if (MainViewModel.SpeechSynthesizer != null)
            {
                MainViewModel.SpeechSynthesizer.SpeakCompleted += SpeechSynthesizer_SpeakCompleted;
            }
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            MainViewModel.Cleanup();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (zSpeechBalloon.ShowDialogBalloon(zSpeechBalloonIcon.Question, Application.Current.MainWindow.Title,
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
                    Dispatcher.Invoke(() =>
                    {
                        object title = msg.Title ?? Application.Current.MainWindow.Title;

                        var messageType = zSpeechBalloonIcon.Information;
                        //object message = msg.Exception == null ? msg.Message :
                        //    (MainViewModel.HeyKodiConfig.DebugMode ? (object)msg.Exception : msg.Exception.ToMessageRecursive());
                        object message = msg.Exception == null ? msg.Message : (object)msg.Exception;
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

                        var r = zSpeechBalloon.ShowDialogBalloon(messageType, title, message, buttons);

                        msg.Result = r == zSpeechBalloonDialogResult.Yes || r == zSpeechBalloonDialogResult.Ok;
                    });
                }
            );

/*            GalaSoft.MvvmLight.Messaging.Messenger.Default.Register<RunKodiCommandMsg>
            (
                Application.Current,
                msg =>
                {
                    //return;

                    Dispatcher.Invoke(() =>
                    {
                        MainViewModel.KodiService.Host = MainViewModel.HeyKodiConfig.KodiApiHost;
                        MainViewModel.KodiService.Port = MainViewModel.HeyKodiConfig.KodiApiPort.ToString();
                        MainViewModel.KodiService.Username = MainViewModel.HeyKodiConfig.KodiApiUserName;
                        MainViewModel.KodiService.Password = MainViewModel.HeyKodiConfig.KodiApiPassword;

                        switch (msg.Command)
                        {
                            case KodiMethods.ExecuteAddon:
                                MainViewModel.KodiService.ExecuteAddon(new ExecuteAddonParams()
                                {
                                    AddonId = "script.globalsearch",
                                    Wait = false,
                                    Params = new string[] { "searchstring=" + msg.Parameter }
                                });
                                break;
                            case KodiMethods.InputHome:
                                MainViewModel.KodiService.InputHome();
                                break;
                            case KodiMethods.InputBack:
                                MainViewModel.KodiService.InputBack();
                                break;
                            default:
                                break;
                        }
                    });
                }
            );*/

            GalaSoft.MvvmLight.Messaging.Messenger.Default.Register<PlaySoundMsg>
            (
                Application.Current,
                msg =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        ActivateMainWindow();

                        MainViewModel.KodiSpeechRecognizer.RecognizeAsyncCancel();

                        if (MainViewModel.HeyKodiConfig.UseSpeechSynthesizer &&
                            !string.IsNullOrWhiteSpace(msg.Speech) && MainViewModel.SpeechSynthesizer != null)
                        {
                            MainViewModel.SpeechSynthesizer.SpeakAsync(msg.Speech);
                        }
                        else
                        {
                            player.Source = new Uri(msg.SoundSource);
                        }
                    });
                }
            );


            GalaSoft.MvvmLight.Messaging.Messenger.Default.Register<ShowConfigurationMsg>
            (
                Application.Current,
                msg =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        zSpeechBalloon.ShowDialogBalloon(zSpeechBalloonIcon.Information, "Configuration de Hey Kodi", ConfigView.Instance, zSpeechBalloonButtonsType.Ok);
                    });
                }
            );

            GalaSoft.MvvmLight.Messaging.Messenger.Default.Register<MinimizeHeyKodiMsg>
            (
                Application.Current,
                msg =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (Application.Current?.MainWindow != null)
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
                    Dispatcher.Invoke(() =>
                    {
                        if (Application.Current?.MainWindow != null)
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

        private void SpeechSynthesizer_SpeakCompleted(object sender, System.Speech.Synthesis.SpeakCompletedEventArgs e)
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
