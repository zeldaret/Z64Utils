using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Z64Utils.ViewModels;

public partial class PickSegmentIDWindowViewModel : ObservableObject
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    [ObservableProperty]
    private string _segmentIDStr = "";

    [ObservableProperty]
    private bool _invalidSegmentID = true;

    public int? SegmentID
    {
        get
        {
            // Return SegmentIDStr as an integer, if it is between 0 and 15,
            // otherwise return null. SegmentIDStr may be decimal or hexadecimal.
            string segmentIDStrTrimmed = SegmentIDStr.Trim();
            int? segmentID;
            try
            {
                segmentID = Convert.ToInt32(segmentIDStrTrimmed);
            }
            catch
            {
                segmentID = null;
            }
            if (segmentID == null)
            {
                try
                {
                    segmentID = Convert.ToInt32(segmentIDStrTrimmed, 16);
                }
                catch
                {
                    segmentID = null;
                }
            }
            if (segmentID != null && segmentID >= 0 && segmentID <= 15)
            {
                return segmentID;
            }
            else
            {
                return null;
            }
        }
        set
        {
            if (value == null)
            {
                SegmentIDStr = "";
            }
            else
            {
                SegmentIDStr = $"{value}";
            }
        }
    }

    public PickSegmentIDWindowViewModel()
    {
        PropertyChanged += (sender, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(SegmentIDStr):
                    InvalidSegmentID = SegmentID == null;
                    break;
            }
        };
    }
}
