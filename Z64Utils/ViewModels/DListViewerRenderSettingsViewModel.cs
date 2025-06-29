using System;
using CommunityToolkit.Mvvm.ComponentModel;
using F3DZEX.Render;

namespace Z64Utils.ViewModels;

public partial class DListViewerRenderSettingsViewModel : ObservableObject
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    [ObservableProperty]
    private float _gridScale;

    [ObservableProperty]
    private bool _showGrid;

    [ObservableProperty]
    private bool _showAxis;

    [ObservableProperty]
    private bool _showGLInfo;

    [ObservableProperty]
    private RdpVertexDrawer.ModelRenderMode _renderMode;

    [ObservableProperty]
    private bool _enabledLighting;

    [ObservableProperty]
    private bool _drawNormals;

    [ObservableProperty]
    private Avalonia.Media.Color _normalColor;

    [ObservableProperty]
    private Avalonia.Media.Color _highlightColor;

    [ObservableProperty]
    private Avalonia.Media.Color _wireframeColor;

    [ObservableProperty]
    private Avalonia.Media.Color _backColor;

    [ObservableProperty]
    private Avalonia.Media.Color _initialPrimColor;

    [ObservableProperty]
    private Avalonia.Media.Color _initialEnvColor;

    [ObservableProperty]
    private Avalonia.Media.Color _initialFogColor;

    [ObservableProperty]
    private Avalonia.Media.Color _initialBlendColor;

    // A reference to the _rendererConfig object passed to the constructor is kept,
    // and that object is updated directly.
    // The RendererConfigChanged event is raised when the config object is updated.
    private readonly Renderer.Config _rendererConfig;
    public event EventHandler? RendererConfigChanged;

    public DListViewerRenderSettingsViewModel(Renderer.Config rendererConfig)
    {
        _rendererConfig = rendererConfig;

        GridScale = rendererConfig.GridScale;
        ShowGrid = rendererConfig.ShowGrid;
        ShowAxis = rendererConfig.ShowAxis;
        ShowGLInfo = rendererConfig.ShowGLInfo;
        RenderMode = rendererConfig.RenderMode;
        EnabledLighting = rendererConfig.EnabledLighting;
        DrawNormals = rendererConfig.DrawNormals;
        NormalColor = BuiltinToAvaloniaColor(rendererConfig.NormalColor);
        HighlightColor = BuiltinToAvaloniaColor(rendererConfig.HighlightColor);
        WireframeColor = BuiltinToAvaloniaColor(rendererConfig.WireframeColor);
        BackColor = BuiltinToAvaloniaColor(rendererConfig.BackColor);
        InitialPrimColor = BuiltinToAvaloniaColor(rendererConfig.InitialPrimColor);
        InitialEnvColor = BuiltinToAvaloniaColor(rendererConfig.InitialEnvColor);
        InitialFogColor = BuiltinToAvaloniaColor(rendererConfig.InitialFogColor);
        InitialBlendColor = BuiltinToAvaloniaColor(rendererConfig.InitialBlendColor);

        PropertyChanged += (sender, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(GridScale):
                    _rendererConfig.GridScale = GridScale;
                    break;
                case nameof(ShowGrid):
                    _rendererConfig.ShowGrid = ShowGrid;
                    break;
                case nameof(ShowAxis):
                    _rendererConfig.ShowAxis = ShowAxis;
                    break;
                case nameof(ShowGLInfo):
                    _rendererConfig.ShowGLInfo = ShowGLInfo;
                    break;
                case nameof(RenderMode):
                    _rendererConfig.RenderMode = RenderMode;
                    break;
                case nameof(EnabledLighting):
                    _rendererConfig.EnabledLighting = EnabledLighting;
                    break;
                case nameof(DrawNormals):
                    _rendererConfig.DrawNormals = DrawNormals;
                    break;
                case nameof(NormalColor):
                    _rendererConfig.NormalColor = AvaloniaToBuiltinColor(NormalColor);
                    break;
                case nameof(HighlightColor):
                    _rendererConfig.HighlightColor = AvaloniaToBuiltinColor(HighlightColor);
                    break;
                case nameof(WireframeColor):
                    _rendererConfig.WireframeColor = AvaloniaToBuiltinColor(WireframeColor);
                    break;
                case nameof(BackColor):
                    _rendererConfig.BackColor = AvaloniaToBuiltinColor(BackColor);
                    break;
                case nameof(InitialPrimColor):
                    _rendererConfig.InitialPrimColor = AvaloniaToBuiltinColor(InitialPrimColor);
                    break;
                case nameof(InitialEnvColor):
                    _rendererConfig.InitialEnvColor = AvaloniaToBuiltinColor(InitialEnvColor);
                    break;
                case nameof(InitialFogColor):
                    _rendererConfig.InitialFogColor = AvaloniaToBuiltinColor(InitialFogColor);
                    break;
                case nameof(InitialBlendColor):
                    _rendererConfig.InitialBlendColor = AvaloniaToBuiltinColor(InitialBlendColor);
                    break;
            }
            RendererConfigChanged?.Invoke(this, new());
        };
    }

    public static System.Drawing.Color AvaloniaToBuiltinColor(Avalonia.Media.Color c)
    {
        return System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B);
    }

    public static Avalonia.Media.Color BuiltinToAvaloniaColor(System.Drawing.Color c)
    {
        return Avalonia.Media.Color.FromArgb(c.A, c.R, c.G, c.B);
    }
}
