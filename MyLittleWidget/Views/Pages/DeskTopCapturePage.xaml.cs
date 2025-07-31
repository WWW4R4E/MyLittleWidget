using Microsoft.Graphics.Canvas.UI.Xaml;
using MyLittleWidget.Contracts;
using MyLittleWidget.Services;
using MyLittleWidget.ViewModels;
using Windows.ApplicationModel.DataTransfer;

namespace MyLittleWidget.Views.Pages
{
  public sealed partial class DeskTopCapturePage : Page
  {
    internal DeskTopCaptureViewModel viewModel =new();
    private WidgetBase _draggingWidgetInstance;
    public DeskTopCapturePage()
    {
      this.InitializeComponent();
      viewModel.timer.Interval = TimeSpan.FromMilliseconds(30);
      Loaded += DeskTopCapturePage_Loaded;
    }

    private void DeskTopCapturePage_Loaded(object sender, RoutedEventArgs e)
    {
      viewModel.PreviewFrameReady += OnPreviewFrameReady;
      LoadAndApplyWindowSize();
      ((App)App.Current).MainWindow.VisibilityChanged += viewModel.ChildenWindow_VisibilityChanged;
      viewModel.DesktopCanvas = DesktopCanvas;
    }

    private void AdjustWindowSizeToContent()
    {
      var window = ((App)Application.Current).MainWindow;
      DisplayArea displayArea = DisplayArea.GetFromWindowId(window.AppWindow.Id, DisplayAreaFallback.Primary);
      double scale = 5.0 / 7.0;
      int width = (int)(displayArea.WorkArea.Width * scale);
      int height = (int)(displayArea.WorkArea.Height * scale);


      int centerX = displayArea.WorkArea.Width / 2 - width / 2 + displayArea.WorkArea.X;
      int centerY = displayArea.WorkArea.Height / 2 - height / 2 + displayArea.WorkArea.Y;

      RectInt32 rect = new RectInt32
      {
        X = centerX,
        Y = centerY,
        Width = width,
        Height = height
      };
      window.AppWindow.MoveAndResize(rect, displayArea);
    }
    private void OnPreviewFrameReady()
    {
      DesktopCanvas.Invalidate();
    }
    private void DesktopCanvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
    {
      if (viewModel.LatestBitmap != null)
      {
        var canvasSize = sender.Size;
        var imageSize = viewModel.LatestBitmap.Size;
        double scale = Math.Min(canvasSize.Width / imageSize.Width, canvasSize.Height / imageSize.Height);
        double scaledWidth = imageSize.Width * scale;
        double scaledHeight = imageSize.Height * scale;
        double x = (canvasSize.Width - scaledWidth) / 2;
        double y = (canvasSize.Height - scaledHeight) / 2;
        args.DrawingSession.DrawImage(viewModel.LatestBitmap, new Rect(x, y, scaledWidth, scaledHeight));
        DropInteractiveCanvas.Width = scaledWidth;
        DropInteractiveCanvas.Height = scaledHeight;

        SharedViewModel.Instance.Scale = scale * viewModel.Dpiscale;
      }
      else
      {
        args.DrawingSession.Clear(Color.FromArgb(0, 0, 0, 0));
      }
    }

    private void LoadAndApplyWindowSize()
    {
      var windowSize = Properties.Settings.Default.WindowSize;
      if (windowSize.Width > 200 &&
          windowSize.Height > 200)
      {
        var window = ((App)Application.Current).MainWindow;
        window.AppWindow.Resize(new SizeInt32(windowSize.Width, windowSize.Height));
      }
      else
      {
        AdjustWindowSizeToContent();
      }
    }
    private void InteractiveCanvas_DragEnter(object sender, DragEventArgs e)
    {
      e.AcceptedOperation = DataPackageOperation.Move;
    }
    private void AddWidgetToCanvas(WidgetBase widget, Point positionOnPreview)
    {
      var scale = SharedViewModel.Instance.Scale;
      if (scale == 0) return;

      double desktopX = positionOnPreview.X / scale;
      double desktopY = positionOnPreview.Y / scale;

      widget.Config.PositionX = desktopX;
      widget.Config.PositionY = desktopY;

      SharedViewModel.Instance.WidgetList.Add(widget);
    }
    private void InteractiveCanvas_Drop(object sender, DragEventArgs e)
    {
      if (_draggingWidgetInstance != null)
      {
        try
        {

          var WidgetFactory = new WidgetFactoryService(new AppSettings(), new WidgetToolService(WindowNative.GetWindowHandle((App.Current as App).WidgetWindow)));
          var newWidget = WidgetFactory.CreateWidgetFromType(_draggingWidgetInstance.Config, _draggingWidgetInstance.GetType());
          if (newWidget != null && sender is FrameworkElement canvas)
          {
            Point dropPosition = e.GetPosition(canvas);
            AddWidgetToCanvas(newWidget, dropPosition);
          }
        }
        finally
        {
          _draggingWidgetInstance = null;
        }
      }
    }

    private void GridView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
    {
      if (e.Items.FirstOrDefault() is WidgetBase draggedWidget)
      {
        _draggingWidgetInstance = draggedWidget;
      }
    }


    private void InteractiveCanvas_DragOver(object sender, DragEventArgs e)
    {
      e.AcceptedOperation = DataPackageOperation.Move;
    }

    private void SaveButton_OnClick(object sender, RoutedEventArgs e)
    {
      // TODO : 完成保存逻辑
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      if (viewModel.SelectedWidget?.Config != null)
      {
        viewModel.SelectedWidget.Config.ExecuteMethod();
      }
    }
  }
}