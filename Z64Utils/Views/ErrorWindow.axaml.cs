using System;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input.Platform;

namespace Z64Utils.Views;

public partial class ErrorWindow : Window
{
    private class CopyToClipboardCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        IClipboard Clipboard;
        Func<string> GetContent;

        public CopyToClipboardCommand(IClipboard clipboard, Func<string> getContent)
        {
            Clipboard = clipboard;
            GetContent = getContent;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            Clipboard.SetTextAsync(GetContent());
        }
    }

    private string FullMessage = "";

    public ErrorWindow()
    {
        InitializeComponent();
        if (Clipboard == null)
        {
            CopyToClipboardButton.IsVisible = false;
        }
        else
        {
            CopyToClipboardButton.Command = new CopyToClipboardCommand(
                Clipboard,
                () => FullMessage
            );
        }
    }

    public void SetMessage(string? message = null, string? monospaceMessage = null)
    {
        var newInlines = new InlineCollection();
        var newFullMessage = "";

        if (message != null)
        {
            newInlines.Add(new Run(message));
            newFullMessage += message;
        }

        if (message != null && monospaceMessage != null)
        {
            newInlines.Add(new Run("\n"));
            newFullMessage += "\n";
        }

        if (monospaceMessage != null)
        {
            newInlines.Add(
                new Run(monospaceMessage)
                {
                    // TODO use app's monospace font (`{StaticResource MonospacedFont}`)
                    FontFamily = "Monospace",
                }
            );
            newFullMessage += monospaceMessage;
        }

        MessageTextBlock.Inlines = newInlines;
        FullMessage = newFullMessage;
    }
}
