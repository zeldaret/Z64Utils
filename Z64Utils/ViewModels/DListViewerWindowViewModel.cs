using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia.Metadata;
using Common;
using CommunityToolkit.Mvvm.ComponentModel;
using Z64;

namespace Z64Utils.ViewModels;

public partial class DListViewerWindowViewModel : ObservableObject
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    private Z64Game? _game;

    // Used by the view to redraw when needed
    public event EventHandler? RenderContextChanged;

    [ObservableProperty]
    public F3DZEX.Render.Renderer? _renderer;

    public ObservableCollection<uint> DLAddresses { get; } = new();

    [ObservableProperty]
    private ObservableCollection<IDLViewerControlDisplayElement> _displayElements = new();

    [ObservableProperty]
    private string? _decodeError;

    [ObservableProperty]
    private string? _renderError;

    [ObservableProperty]
    private string? _GLInfoText;

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
        DLAddresses.CollectionChanged += (sender, e) => DecodeDLists();
    }

    private void OnRendererPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Utils.Assert(Renderer != null);
        switch (e.PropertyName)
        {
            case nameof(Renderer.RenderErrorAddr):
            case nameof(Renderer.ErrorMsg):
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

            case nameof(Renderer.GLInfoText):
                GLInfoText = Renderer.GLInfoText;
                break;
        }
    }

    public void SetSingleDlist(uint vaddr)
    {
        DLAddresses.Clear();
        DLAddresses.Add(vaddr);
    }

    private void DecodeDLists()
    {
        if (Renderer == null)
            throw new Exception("Renderer is null");

        DisplayElements.Clear();
        var decodeErrors = new List<string>();

        foreach (var vaddr in DLAddresses)
        {
            Logger.Debug("vaddr=0x{vaddr:X8}", vaddr);

            F3DZEX.Command.Dlist? dList;
            try
            {
                dList = Renderer.GetDlist(vaddr);
            }
            catch (Exception e)
            {
                decodeErrors.Add($"Could not decode DL 0x{vaddr:X8}: {e.Message}");
                dList = null;
            }
            if (dList != null)
            {
                DisplayElements.Add(new DLViewerControlDListDisplayElement(dList));
            }
        }

        if (decodeErrors.Count == 0)
            DecodeError = null;
        else
            DecodeError = string.Join("\n", decodeErrors);
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

    public void OpenDisassemblyCommand()
    {
        // TODO
    }

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
        vm.SegmentsConfigChanged += (sender, e) =>
        {
            Logger.Debug("SegmentsConfigChanged");
            for (int i = 0; i < F3DZEX.Memory.Segment.COUNT; i++)
                Logger.Debug("{segmentIndex} {segmentLabel}", i, Renderer.Memory.Segments[i].Label);
            DecodeDLists();
            RenderContextChanged?.Invoke(this, new());
        };
    }
}
