using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Common;

namespace Z64Utils_Avalonia;

public class OHEDViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        Utils.Assert(param is IObjectHolderEntryDetailsViewModel);
        switch (param.GetType().Name)
        {
            case nameof(EmptyOHEDViewModel):
                // TODO should make an (empty) view
                // (or just remove this and use TextOHEDView instead)
                return new TextBlock()
                {
                    Text = "EmptyOHEDViewModel, TextBlock by OHEDViewLocator",
                };

            case nameof(TextOHEDViewModel):
                return new TextOHEDView();

            case nameof(ImageOHEDViewModel):
                return new ImageOHEDView();

            case nameof(VertexArrayOHEDViewModel):
                return new VertexArrayOHEDView();

            default:
                throw new NotImplementedException(
                    "Unknown View for the IObjectHolderEntryDetailsViewModel: "
                        + param.GetType().FullName
                );
        }
    }

    public bool Match(object? data)
    {
        return data is IObjectHolderEntryDetailsViewModel;
    }
}
