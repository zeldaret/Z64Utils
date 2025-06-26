using Avalonia.Controls;
using Common;

namespace Z64Utils_Avalonia;

public partial class SkeletonViewerWindow : Window
{
    public SkeletonViewerWindowViewModel ViewModel;

    public SkeletonViewerWindow()
    {
        ViewModel = new();
        DataContext = ViewModel;
        InitializeComponent();
    }

    public void OnAnimationEntriesDataGridSelectionChanged(
        object? sender,
        SelectionChangedEventArgs ev
    )
    {
        var selectedItem = AnimationEntriesDataGrid.SelectedItem;
        if (selectedItem == null)
            return;
        Utils.Assert(selectedItem is SkeletonViewerWindowViewModel.AnimationEntry);
        var animationEntry = (SkeletonViewerWindowViewModel.AnimationEntry)selectedItem;
        ViewModel.OnAnimationEntrySelected(animationEntry);
    }
}
