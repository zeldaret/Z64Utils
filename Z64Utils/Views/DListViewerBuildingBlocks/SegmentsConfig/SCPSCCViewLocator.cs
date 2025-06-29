using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Common;
using Z64Utils.ViewModels;
using Z64Utils.Views.DListViewerBuildingBlocks.SegmentsConfig.SCPSCC;

namespace Z64Utils.Views.DListViewerBuildingBlocks.SegmentsConfig;

public class SCPSCCViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        Utils.Assert(param is ISegmentConfigPickSegmentContentConfigViewModel);
        switch (param.GetType().Name)
        {
            case nameof(EmptySCPSCCViewModel):
                return null;

            case nameof(AddressSCPSCCViewModel):
                return new AddressSCPSCCView();

            case nameof(ROMFileSystemSCPSCCViewModel):
                return new ROMFileSystemSCPSCCView();

            case nameof(FileSCPSCCViewModel):
                return new FileSCPSCCView();

            case nameof(PrimColorDListSCPSCCViewModel):
                return new PrimColorDListSCPSCCView();

            case nameof(EnvColorDListSCPSCCViewModel):
                return new EnvColorDListSCPSCCView();

            default:
                throw new NotImplementedException(
                    "Unknown View for the ISegmentConfigPickSegmentContentConfigViewModel: "
                        + param.GetType().FullName
                );
        }
    }

    public bool Match(object? data)
    {
        return data is ISegmentConfigPickSegmentContentConfigViewModel;
    }
}
