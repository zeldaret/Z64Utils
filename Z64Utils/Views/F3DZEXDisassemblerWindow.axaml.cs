using System;
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

    private F3DZEXDisassemblerSettingsViewModel? OpenF3DZEXDisassemblerSettings(
        Func<F3DZEXDisassemblerSettingsViewModel> vmFactory
    )
    {
        if (CurrentSettingsWindow != null)
        {
            CurrentSettingsWindow.Activate();
            return null;
        }

        var vm = vmFactory();
        CurrentSettingsWindow = new F3DZEXDisassemblerSettingsWindow() { DataContext = vm };
        CurrentSettingsWindow.Closed += (sender, e) =>
        {
            CurrentSettingsWindow = null;
        };
        CurrentSettingsWindow.Show();
        return vm;
    }
}
