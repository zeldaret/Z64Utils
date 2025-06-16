using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Z64Utils_Avalonia;

public partial class ImageOHEDViewModel : ObservableObject, IObjectHolderEntryDetailsViewModel
{
    [ObservableProperty]
    private string _infoText = "";

    [ObservableProperty]
    private IImage? _image;
}
