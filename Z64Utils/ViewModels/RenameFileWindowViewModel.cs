using CommunityToolkit.Mvvm.ComponentModel;

namespace Z64Utils_Avalonia;

public partial class RenameFileWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string _name = "";
}
