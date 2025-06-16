using CommunityToolkit.Mvvm.ComponentModel;

namespace Z64Utils_Avalonia;

public partial class TextOHEDViewModel : ObservableObject, IObjectHolderEntryDetailsViewModel
{
    [ObservableProperty]
    private string _text = "";
}
