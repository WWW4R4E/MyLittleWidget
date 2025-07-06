using CommunityToolkit.Mvvm.ComponentModel;
namespace MyLittleWidget.CustomBase
{
    public partial class WidgetConfig : ObservableObject
    {
        public Guid Id { get; init; } 
        public string Name { get; set; }
   
        public int UnitWidth { get; set; } = 2;
        public int UnitHeight { get; set; } = 2;
        [ObservableProperty]
        private double _positionX;

        [ObservableProperty]
        private double _positionY;
    }
}
