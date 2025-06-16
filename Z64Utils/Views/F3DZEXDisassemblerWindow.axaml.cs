using Avalonia.Controls;

namespace Z64Utils_Avalonia;

public partial class F3DZEXDisassemblerWindow : Window
{
    public F3DZEXDisassemblerViewModel ViewModel { get; }

    private F3DZEXDisassemblerSettingsWindow? CurrentSettingsWindow;

    public F3DZEXDisassemblerWindow()
    {
        ViewModel = new() { OpenF3DZEXDisassemblerSettings = OpenF3DZEXDisassemblerSettings };
        DataContext = ViewModel;
        InitializeComponent();
    }

    private F3DZEXDisassemblerSettingsViewModel? OpenF3DZEXDisassemblerSettings()
    {
        if (CurrentSettingsWindow != null)
        {
            CurrentSettingsWindow.Activate();
            return null;
        }

        CurrentSettingsWindow = new F3DZEXDisassemblerSettingsWindow();
        CurrentSettingsWindow.Closed += (sender, e) =>
        {
            CurrentSettingsWindow = null;
        };
        CurrentSettingsWindow.Show();
        return CurrentSettingsWindow.ViewModel;
    }
}
