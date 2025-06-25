using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using Avalonia.Metadata;
using Common;
using CommunityToolkit.Mvvm.ComponentModel;
using Z64;

namespace Z64Utils_Avalonia;

public partial class DListViewerWindowViewModel : ObservableObject
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    private Z64Game? _game;

    // Used by the view to redraw when needed
    public event EventHandler? RenderContextChanged;

    [ObservableProperty]
    private string _someTextForNow = "";

    [ObservableProperty]
    public F3DZEX.Render.Renderer? _renderer;

    [ObservableProperty]
    private ObservableCollection<IDLViewerControlDisplayElement> _displayElements = new();

    [ObservableProperty]
    private string? _decodeError;

    [ObservableProperty]
    private string? _renderError;

    // Provided by the view
    public Func<
        Func<DListViewerRenderSettingsViewModel>,
        DListViewerRenderSettingsViewModel?
    >? OpenDListViewerRenderSettings;
    public Func<
        Func<SegmentsConfigWindowViewModel>,
        SegmentsConfigWindowViewModel?
    >? OpenSegmentsConfig;

    public DListViewerWindowViewModel(Z64Game? game)
    {
        _game = game;
        PropertyChanging += (sender, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(Renderer):
                    if (Renderer != null)
                        Renderer.PropertyChanged -= OnRendererPropertyChanged;
                    DisplayElements.Clear();
                    DecodeError = null;
                    RenderError = null;
                    break;
            }
        };
        PropertyChanged += (sender, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(Renderer):
                    if (Renderer != null)
                        Renderer.PropertyChanged += OnRendererPropertyChanged;
                    break;
            }
        };
    }

    private void OnRendererPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Utils.Assert(Renderer != null);
        switch (e.PropertyName)
        {
            case nameof(Renderer.HasError):
                if (Renderer.HasError)
                {
                    RenderError =
                        $"RENDER ERROR AT 0x{Renderer.RenderErrorAddr:X8}! ({Renderer.ErrorMsg})";
                }
                else
                {
                    RenderError = null;
                }
                break;
        }
    }

    public void SetSegment(int index, F3DZEX.Memory.Segment segment)
    {
        if (Renderer == null)
            throw new Exception("Renderer is null");

        if (index >= 0 && index < F3DZEX.Memory.Segment.COUNT)
        {
            Renderer.Memory.Segments[index] = segment;

            // TODO redecode dlist, rerender
        }
    }

    public void SetSingleDlist(uint vaddr)
    {
        if (Renderer == null)
            throw new Exception("Renderer is null");

        Logger.Debug("vaddr={vaddr}", vaddr);

        F3DZEX.Command.Dlist? dList;
        try
        {
            dList = Renderer.GetDlist(vaddr);
        }
        catch (Exception e)
        {
            DecodeError = $"Could not decode DL 0x{vaddr:X8}: {e.Message}";
            dList = null;
        }
        if (dList != null)
        {
            DisplayElements.Clear();
            DisplayElements.Add(new DLViewerControlDListDisplayElement(dList));
        }
    }

    public void OpenRenderSettingsCommand()
    {
        Utils.Assert(OpenDListViewerRenderSettings != null);
        Utils.Assert(Renderer != null);
        var vm = OpenDListViewerRenderSettings(
            () => new DListViewerRenderSettingsViewModel(Renderer.CurrentConfig)
        );
        if (vm == null)
        {
            // Was already open
            return;
        }

        vm.RendererConfigChanged += (sender, e) =>
        {
            RenderContextChanged?.Invoke(this, new());
        };
    }

    [DependsOn(nameof(Renderer))]
    public bool CanOpenRenderSettingsCommand(object arg)
    {
        return Renderer != null;
    }

    public void OpenDisassemblyCommand() { }

    public void OpenSegmentsConfigCommand()
    {
        Utils.Assert(OpenSegmentsConfig != null);
        Utils.Assert(Renderer != null);
        var vm = OpenSegmentsConfig(
            () => new SegmentsConfigWindowViewModel(Renderer.Memory, _game)
        );
        if (vm == null)
        {
            // Was already open
            return;
        }
        vm.SegmentsConfigChanged += (sender, e) => {
            // TODO
        };
    }
}
