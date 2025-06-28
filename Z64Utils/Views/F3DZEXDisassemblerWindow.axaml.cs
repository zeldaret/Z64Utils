using System;
using Avalonia.Controls;
using Z64Utils.ViewModels;

namespace Z64Utils_Avalonia;

public partial class F3DZEXDisassemblerWindow : Window
{
    private F3DZEXDisassemblerSettingsWindow? _currentSettingsWindow;

    public F3DZEXDisassemblerWindow()
    {
        InitializeComponent();
        DataContextChanged += (sender, e) =>
        {
            var vm = (F3DZEXDisassemblerViewModel?)DataContext;
            if (vm == null)
                return;
            vm.OpenF3DZEXDisassemblerSettings = OpenF3DZEXDisassemblerSettings;
        };
    }

    private F3DZEXDisassemblerSettingsViewModel? OpenF3DZEXDisassemblerSettings(
        Func<F3DZEXDisassemblerSettingsViewModel> vmFactory
    )
    {
        if (_currentSettingsWindow != null)
        {
            _currentSettingsWindow.Activate();
            return null;
        }

        var vm = vmFactory();
        _currentSettingsWindow = new F3DZEXDisassemblerSettingsWindow() { DataContext = vm };
        _currentSettingsWindow.Closed += (sender, e) =>
        {
            _currentSettingsWindow = null;
        };
        _currentSettingsWindow.Show();
        return vm;
    }
}
