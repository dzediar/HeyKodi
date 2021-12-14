using HeyKodi.Messages;
using HeyKodi.ViewModels;
using KodiRPC.RPC.RequestResponse.Params.VideoLibrary;
using KodiRPC.RPC.Specifications;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using zComp.Core;
using zComp.Wpf;
using zComp.Wpf.Helpers;

namespace HeyKodi.Views
{
    /// <summary>
    /// Logique d'interaction pour ConfigView.xaml
    /// </summary>
    public partial class ConfigView : UserControl, IReadOnlyElement
    {
        private static ConfigView instance;

        public static ConfigView Instance { get { return instance ?? (instance = new ConfigView()); } }

        public ConfigView()
        {
            InitializeComponent();

            if (instance != null)
            {
                throw new Exception($"{nameof(ConfigView)} must be a singleton");
            }

            this.Loaded += ConfigView_Loaded;

            this.MainViewModel = MainViewModel.Instance;
        }

        private void ConfigView_Loaded(object sender, RoutedEventArgs e)
        {
            //Matrix m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
            //ScaleTransform dpiTransform = new ScaleTransform(1 / m.M11, 1 / m.M22);
            //if (dpiTransform.CanFreeze)
            //    dpiTransform.Freeze();
            //this.LayoutTransform = dpiTransform;
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
