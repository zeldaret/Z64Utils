using System.Threading.Tasks;
using Avalonia.Controls;

namespace Z64Utils_Avalonia;

public partial class SegmentsConfigWindow : Window
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public SegmentsConfigWindow()
    {
        InitializeComponent();
        DataContextChanged += (sender, e) =>
        {
            if (DataContext == null)
                return;
            var vm = (SegmentsConfigWindowViewModel)DataContext;
            vm.PickSegmentContent = PickSegmentContent;
        };
    }

    private async Task<F3DZEX.Memory.Segment?> PickSegmentContent(
        SegmentsConfigPickSegmentContentWindowViewModel vm
    )
    {
        var pickSegmentContentWin = new SegmentsConfigPickSegmentContentWindow()
        {
            DataContext = vm,
        };
        var dialogResultTask = pickSegmentContentWin.ShowDialog<F3DZEX.Memory.Segment?>(this);
        var segmentContent = await dialogResultTask;
        return segmentContent;
    }
}
