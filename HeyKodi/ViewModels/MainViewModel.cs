using GalaSoft.MvvmLight.CommandWpf;
using HeyKodi.Messages;
using HeyKodi.Model;
using HeyKodi.Tools;
using KodiRPC.Responses.VideoLibrary;
using KodiRPC.RPC.RequestResponse.Params.VideoLibrary;
using KodiRPC.RPC.Specifications;
using KodiRPC.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using zComp.Core.Helpers;

namespace HeyKodi.ViewModels
{
    public class MainViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private static MainViewModel instance;

        public static MainViewModel Instance { get { return instance ?? (instance = new MainViewModel()); } }

        public Dictionary<KodiSpeechRecognizerSound, string> SoundsResources = new Dictionary<KodiSpeechRecognizerSound, string>()
        {
            { KodiSpeechRecognizerSound.CancelSound, "GS46.mp3" },
            { KodiSpeechRecognizerSound.WakeupSound, "GS155.mp3" },
            { KodiSpeechRecognizerSound.NeedMoreParamSound, "GS213.mp3" },
            { KodiSpeechRecognizerSound.RunCommandSound, "GS254.mp3" }
        };

        private bool stayActivated;

        private int minimizationDelay = 0;

        private MainViewModel()
        {
            try
            {
                ShowConfigurationCommand = new RelayCommand(() => ShowConfiguration());
                MinimizeCommand = new RelayCommand(() => Minimize());

                KodiService = new KodiService();

                HeyKodiConfig = HeyKodiConfigExtensions.Load();

                KodiSpeechRecognizer = new KodiSpeechRecognizer(HeyKodiConfig,
                    (c, p) => RunKodiCommand(c, p), s => PlaySound(s), (m, e) => ShowError(m, e));

                KodiSpeechRecognizer.PropertyChanged += KodiSpeechRecognizer_PropertyChanged;

                KodiSpeechRecognizer.Start();
            }
            catch (Exception ex)
            {
                throw new CriticalApplicationException("Une erreur critique s'est produite lors de l'initialisation de Hey Kodi, l'application va se fermer.", ex);
            }
        }

        private void KodiSpeechRecognizer_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(KodiSpeechRecognizer.State))
            {
                if (KodiSpeechRecognizer.State == KodiSpeechRecognizerState.Pending)
                {
                    if (HeyKodiConfig.MinimizeWhenPending)
                    {
                        if (stayActivated)
                        {
                            stayActivated = false;
                        }
                        else
                        {
                            Task.Run(() => { System.Threading.Thread.Sleep(minimizationDelay); Minimize(); });
                        }
                    }
                }
                else
                {
                    Activate();
                }

                minimizationDelay = 1500;
            }
        }

        private bool RunKodiCommand(HeyKodiCommandEnum command, string parameter = null)
        {
            var result = false;

            if (command == HeyKodiCommandEnum.ShowHeyKodiConfig)
            {
                ShowConfiguration();
                result = true;
            }
            else if (command == HeyKodiCommandEnum.CancelHeyKodi)
            {
                KodiSpeechRecognizer.Cancel(false);
                result = false;
            }
            else
            {
                List<Player> players = null;

                if (command == HeyKodiCommandEnum.Search)
                {
                    if (!(RunKodiCommand(HeyKodiCommandEnum.Back) && RunKodiCommand(HeyKodiCommandEnum.Home)))
                    {
                        return false;
                    }
                }
                else if (command == HeyKodiCommandEnum.Stop ||
                    command == HeyKodiCommandEnum.Play ||
                    command == HeyKodiCommandEnum.Pause)
                {
                    players = KodiService.GetPlayers(new GetPlayersParams()
                    {
                        Media = "all"
                    }).Result.Items;
                }

                KodiService.Host = HeyKodiConfig.KodiApiHost;
                KodiService.Port = HeyKodiConfig.KodiApiPort.ToString();
                KodiService.Username = HeyKodiConfig.KodiApiUserName;
                KodiService.Password = HeyKodiConfig.KodiApiPassword;

                //var kodiApiMethod = HeyKodiConfigExtensions.CommandRepository[command].KodiApiMethod;

                switch (command)
                {
                    case HeyKodiCommandEnum.Search:
                        KodiService.ExecuteAddon(new ExecuteAddonParams()
                        {
                            AddonId = "script.globalsearch",
                            Wait = false,
                            Params = new string[] { "searchstring=" + parameter }
                        });
                        break;
                    case HeyKodiCommandEnum.Home:
                        KodiService.InputHome();
                        break;
                    case HeyKodiCommandEnum.Back:
                        KodiService.InputBack();
                        break;
                    case HeyKodiCommandEnum.Quit:
                        KodiService.QuitApplication();
                        break;                        
                    case HeyKodiCommandEnum.Stop:
                        if (players != null && players.Count > 0)
                        {
                            foreach (var p in players)
                            {
                                KodiService.StopPlayer(new StopPlayerParams() { PlayerId = p.Name });
                            }
                        }
                        break;
                    case HeyKodiCommandEnum.Play:
                        if (players != null && players.Count > 0)
                        {
                            foreach (var p in players)
                            {
                                KodiService.TogglePlayerPlayPause(new TogglePlayPausePlayerParams() { Play = true.ToString() });
                            }
                        }
                        break;
                    case HeyKodiCommandEnum.Pause:
                        if (players != null && players.Count > 0)
                        {
                            foreach (var p in players)
                            {
                                KodiService.TogglePlayerPlayPause(new TogglePlayPausePlayerParams() { Play = false.ToString() });
                            }
                        }
                        break;
                    case HeyKodiCommandEnum.MuteUnmute:
                        KodiService.SetMute(new SetMuteParams());
                        break;
                    case HeyKodiCommandEnum.ShowVideos: 
                        KodiService.ActivateWindow("videos");
                        break;
                    case HeyKodiCommandEnum.ShowTV: 
                        KodiService.ActivateWindow("tvsearch");
                        break;
                    case HeyKodiCommandEnum.ShowGames: 
                        KodiService.ActivateWindow("games");
                        break;
                    case HeyKodiCommandEnum.ShowMusic: 
                        KodiService.ActivateWindow("music");
                        break;
                    case HeyKodiCommandEnum.ShowWeather: 
                        KodiService.ActivateWindow("weather");
                        break;
                    case HeyKodiCommandEnum.ShowFavourites: 
                        KodiService.ActivateWindow("favourites");
                        break;
                    case HeyKodiCommandEnum.EjectOpticalDrive:
                        KodiService.EjectOpticalDrive(new EjectOpticalDriveParams());
                        break;
                    default:
                        throw new Exception($"Unknown command : {command}");
                }

                result = true;
            }

            PlaySound(result ? KodiSpeechRecognizerSound.RunCommandSound : KodiSpeechRecognizerSound.CancelSound);

            return result;
        }

        private void PlaySound(KodiSpeechRecognizerSound sound)
        {
            var soundsDir = UnpackSounds();
            var soundFileName = System.IO.Path.Combine(soundsDir, SoundsResources[sound]);

            var msg = new PlaySoundMsg(this, soundFileName);
            GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(msg);
        }

        private string UnpackSounds()
        {
            var soundsDir = Path.Combine(System.IO.Path.GetDirectoryName(HeyKodiConfigExtensions.GetConfigFilePath()), "Sounds");

            if (!System.IO.Directory.Exists(soundsDir))
            {
                System.IO.Directory.CreateDirectory(soundsDir);
                foreach (var s in SoundsResources)
                {
                    var sr = "pack://application:,,,/HeyKodi;component/Sounds/" + s.Value;
                    var srp = Path.Combine(soundsDir, s.Value);
                    var sri = Application.GetResourceStream(new Uri(sr));

                    using (var srs = sri.Stream)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            srs.CopyTo(ms);
                            System.IO.File.WriteAllBytes(srp, ms.ToArray());
                        }
                    }
                }
            }

            return soundsDir;
        }

        private void ShowError(string message, Exception exception)
        {
            var msg = exception == null ? new ShowMessageMsg(this, null, message, ShowMessageType.Error) :
                new ShowMessageMsg(this, null, exception);
            GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(msg);
        }

        private void ShowConfiguration()
        {
            stayActivated = true;
            KodiSpeechRecognizer.Stop();
            var msg = new ShowConfigurationMsg(this);
            GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(msg);
            HeyKodiConfig.Save();
            KodiSpeechRecognizer.Start();
        }

        private void Minimize()
        {
            var msg = new MinimizeHeyKodiMsg(this);
            GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(msg);
        }

        private void Activate()
        {
            var msg = new ActivateHeyKodiMsg(this);
            GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(msg);
        }

        public override void Cleanup()
        {
            KodiSpeechRecognizer.Dispose();

            HeyKodiConfig.Save();

            base.Cleanup();
        }

        private KodiService kodiService;

        public KodiService KodiService
        {
            get 
            {
                return kodiService;
            }
            private set
            {
                kodiService = value;
                RaisePropertyChanged(nameof(KodiService));
            }
        }

        private KodiSpeechRecognizer kodiSpeechRecognizer;

        public KodiSpeechRecognizer KodiSpeechRecognizer
        {
            get
            {
                return kodiSpeechRecognizer;
            }
            private set
            {
                kodiSpeechRecognizer = value;
                RaisePropertyChanged(nameof(KodiSpeechRecognizer));
            }
        }

        private HeyKodiConfig heyKodiConfig;

        public HeyKodiConfig HeyKodiConfig
        {
            get
            {
                return heyKodiConfig;
            }
            private set
            {
                heyKodiConfig = value;
                RaisePropertyChanged(nameof(HeyKodiConfig));
            }
        }

        private RelayCommand showConfigurationCommand;

        public RelayCommand ShowConfigurationCommand
        {
            get
            {
                return showConfigurationCommand;
            }
            private set
            {
                showConfigurationCommand = value;
                RaisePropertyChanged(nameof(ShowConfigurationCommand));
            }
        }

        private RelayCommand minimizeCommand;

        public RelayCommand MinimizeCommand
        {
            get
            {
                return minimizeCommand;
            }
            private set
            {
                minimizeCommand = value;
                RaisePropertyChanged(nameof(MinimizeCommand));
            }
        }        
    }
}
