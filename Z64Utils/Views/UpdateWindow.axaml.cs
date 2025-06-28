using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Z64Utils.Views;

public partial class UpdateWindow : Window
{
    UpdateWindowViewModel ViewModel;

    public UpdateWindow()
    {
        InitializeComponent();
        ViewModel = new();
        DataContext = ViewModel;
    }

    public void SetNewRelease(
        string currentReleaseTagName,
        string latestReleaseTagName,
        bool isUpToDate
    )
    {
        ViewModel.CurrentReleaseTagName = currentReleaseTagName;
        ViewModel.LatestReleaseTagName = latestReleaseTagName;
        ViewModel.IsUpToDate = isUpToDate;
    }
}

public partial class UpdateWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string _currentReleaseTagName = "";

    [ObservableProperty]
    private string _latestReleaseTagName = "";

    [ObservableProperty]
    private bool _isUpToDate = true;
}
