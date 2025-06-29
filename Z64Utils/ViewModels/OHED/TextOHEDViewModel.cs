using CommunityToolkit.Mvvm.ComponentModel;

namespace Z64Utils.ViewModels.OHED;

public partial class TextOHEDViewModel : ObservableObject, IObjectHolderEntryDetailsViewModel
{
    [ObservableProperty]
    private string _text = "";
}
