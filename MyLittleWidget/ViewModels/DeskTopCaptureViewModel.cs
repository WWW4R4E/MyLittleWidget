using Microsoft.Graphics.Canvas;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using MyLittleWidget.Models;
using MyLittleWidget.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyLittleWidget.ViewModels
{
    internal class DeskTopCaptureViewModel : INotifyPropertyChanged
    {
        internal List<LittleWidget> littleWidgets = new List<LittleWidget>() {
        new() { Title = "小组件1"},
        new LittleWidget{ Title = "小组件2"},
        };

        internal ObservableCollection<GridItem> GridData { get; set; }
        internal CanvasBitmap? latestBitmap;
        internal DispatcherTimer timer = new();
        internal float scale;
        internal bool isdo = false;
        internal float Dpiscale = GetDesktop.GetSystemDpiScale();
        private Thickness _workAreaMargin;
        public Thickness WorkAreaMargin// x=0,y=0,width=1920,height=1080
        {
            get => _workAreaMargin;
            set
            {
                if (_workAreaMargin != value)
                {
                    _workAreaMargin = value;
                    OnPropertyChanged();
                }
            }
        }
        private Windows.Foundation.Point _desktopItemSpace;
        public Windows.Foundation.Point DesktopItemSpace// x=115,y=156
        {
            get => _desktopItemSpace;
            set
            {
                if (_desktopItemSpace != value)
                {
                    _desktopItemSpace = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        internal DeskTopCaptureViewModel()
        {
            GridData = new ObservableCollection<GridItem>();
        }
    }
}