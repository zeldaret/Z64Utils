using CommunityToolkit.Mvvm.ComponentModel;

namespace Z64Utils.ViewModels;

public partial class RenameFileWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string _name = "";
}
