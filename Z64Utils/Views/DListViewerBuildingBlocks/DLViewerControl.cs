using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Common;
using F3DZEX.Command;
using F3DZEX.Render;
using OpenTK.Mathematics;

namespace Z64Utils_Avalonia;

public class DLViewerControl : OpenTKControlBaseWithCamera
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public static readonly StyledProperty<Renderer?> RendererProperty = AvaloniaProperty.Register<
        DLViewerControl,
        Renderer?
    >(nameof(Renderer), defaultValue: null);
    public Renderer? Renderer
    {
        get => GetValue(RendererProperty);
        set => SetValue(RendererProperty, value);
    }

    public static readonly StyledProperty<
        ObservableCollection<IDLViewerControlDisplayElement>
    > DisplayElementsProperty = AvaloniaProperty.Register<
        DLViewerControl,
        ObservableCollection<IDLViewerControlDisplayElement>
    >(nameof(DisplayElements), defaultValue: new());
    public ObservableCollection<IDLViewerControlDisplayElement> DisplayElements
    {
        get => GetValue(DisplayElementsProperty);
        set => SetValue(DisplayElementsProperty, value);
    }

    public static readonly StyledProperty<string?> RenderErrorProperty = AvaloniaProperty.Register<
        DLViewerControl,
        string?
    >(nameof(RenderError), defaultValue: null);
    public string? RenderError
    {
        get => GetValue(RenderErrorProperty);
        set => SetValue(RenderErrorProperty, value);
    }

    public DLViewerControl()
        : base(
            new CameraHandling(
                camPos: new Vector3(0, -2000, -15000),
                angle: new Vector3(20, -30, 0)
            )
        )
    {
        Logger.Debug("Name={Name}", Name);

        DisplayElements.CollectionChanged += OnDisplayElementsCollectionChanged;

        PropertyChanged += (sender, e) =>
        {
            if (e.Property == RendererProperty)
            {
                RequestNextFrameRenderingIfInitialized();
            }
            if (e.Property == DisplayElementsProperty)
            {
                var oldValue = e.GetOldValue<
                    ObservableCollection<IDLViewerControlDisplayElement>
                >();
                if (oldValue != null)
                    oldValue.CollectionChanged -= OnDisplayElementsCollectionChanged;

                var newValue = e.GetNewValue<
                    ObservableCollection<IDLViewerControlDisplayElement>
                >();
                Utils.Assert(newValue != null);
                newValue.CollectionChanged += OnDisplayElementsCollectionChanged;

                Redraw();
            }
        };
    }

    private void OnDisplayElementsCollectionChanged(
        object? sender,
        NotifyCollectionChangedEventArgs e
    )
    {
        Redraw();
    }

    public void Redraw()
    {
        if (Renderer != null)
            Renderer.ClearErrors();
        RequestNextFrameRenderingIfInitialized();
    }

    protected override void OnOpenTKInit() { }

    protected override void OnOpenTKRender()
    {
        Logger.Trace("Name={Name} in", Name);
        SetFullViewport();

        if (Renderer != null)
        {
            Logger.Trace("Name={Name} RenderStart...", Name);

            Renderer.RenderStart(Proj, View);

            Logger.Trace("Name={Name} RenderStart OK", Name);

            foreach (var de in DisplayElements)
            {
                if (Renderer.RenderFailed())
                    break;

                Dlist dList;
                Matrix4? mtx = null;

                if (de is DLViewerControlDListDisplayElement deDL)
                {
                    dList = deDL.dList;
                }
                else if (de is DLViewerControlDlistWithMatrixDisplayElement deDLwithMtx)
                {
                    dList = deDLwithMtx.dList;
                    mtx = deDLwithMtx.mtx;
                }
                else
                {
                    throw new NotImplementedException($"unsupported displayelement {de}");
                }

                if (mtx != null)
                {
                    Renderer.RdpMtxStack.Push();
                    // TODO idk why explicit cast is needed
                    Renderer.RdpMtxStack.Load((Matrix4)mtx);
                }

                Logger.Trace("Name={Name} RenderDList({dList})", Name, dList);
                Renderer.RenderDList(dList);
                Logger.Trace("Name={Name} RenderDList OK", Name);

                if (mtx != null)
                {
                    Renderer.RdpMtxStack.Pop();
                }
            }

            if (Renderer.RenderFailed())
            {
                var addr = Renderer.RenderErrorAddr;
                var msg = Renderer.ErrorMsg;
                RenderError = $"At 0x{addr:X8}: {msg}";
            }
            else
            {
                RenderError = null;
            }
        }

        Logger.Trace("Name={Name} out", Name);
    }
}
