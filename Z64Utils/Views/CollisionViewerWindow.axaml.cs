using Avalonia.Controls;

namespace Z64Utils_Avalonia;

public partial class CollisionViewerWindow : Window
{
    public CollisionViewerWindow()
    {
        InitializeComponent();
        DataContextChanged += (sender, e) =>
        {
            var vm = (CollisionViewerWindowViewModel?)DataContext;
            if (vm == null)
                return;
            vm.OpenCollisionRenderSettings = OpenCollisionRenderSettings;
        };
    }

    public void OpenCollisionRenderSettings(CollisionRenderSettings vm)
    {
        var win = new CollisionViewerRenderSettingsWindow() { DataContext = vm };
        win.Show();
    }
}
