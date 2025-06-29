using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace Z64Utils.Views.OHED;

public partial class ImageOHEDView : UserControl
{
    public ImageOHEDView()
    {
        InitializeComponent();
        ImageWrapperPanel.SizeChanged += (sender, e) => UpdateImageZoom();
    }

    double _imageZoom = 1;

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        _imageZoom *= 1 + e.Delta.Y / 10;
        if (_imageZoom > 1)
            _imageZoom = 1;
        if (_imageZoom < 0.05)
            _imageZoom = 0.05;
        UpdateImageZoom();
    }

    public void UpdateImageZoom()
    {
        var fullSize = ImageWrapperPanel.Bounds.Size;
        var targetSize = new Size(fullSize.Width * _imageZoom, fullSize.Height * _imageZoom);
        Image.Margin = new(
            (fullSize.Width - targetSize.Width) / 2,
            (fullSize.Height - targetSize.Height) / 2
        );
    }
}
