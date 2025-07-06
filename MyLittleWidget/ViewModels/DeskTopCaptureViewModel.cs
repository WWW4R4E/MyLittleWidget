using Microsoft.Graphics.Canvas;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using MyLittleWidget.CustomBase;
using MyLittleWidget.Models;
using MyLittleWidget.Utils;

namespace MyLittleWidget.ViewModels
{
    internal class DeskTopCaptureViewModel : INotifyPropertyChanged
    {
        internal List<LittleWidget> littleWidgets = new List<LittleWidget>() {
        new() { Title = "小组件1",widget = new CustomControl1()},
        new LittleWidget{ Title = "小组件2",widget = new CustomControl2()},
        };

        internal ObservableCollection<GridItem> GridData { get; set; }
        internal CanvasBitmap? latestBitmap;
        internal DispatcherTimer timer = new();
        internal float scale;
        internal bool isdo = false;
        internal float Dpiscale = GetDesktop.GetSystemDpiScale();
        private Thickness _workAreaMargin;
        public Thickness WorkAreaMargin
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
        private Point _desktopItemSpace;
        public Point DesktopItemSpace
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