using System;
using System.Linq;
using Avalonia;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Common;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using static Avalonia.OpenGL.GlConsts;

namespace Z64Utils_Avalonia;

public abstract class OpenTKControlBase : OpenGlControlBase
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    private void CheckError(GlInterface gl, int[]? ignoredErrors = null)
    {
        int err;
        while ((err = gl.GetError()) != GL_NO_ERROR)
        {
            Logger.Error("Name={Name} GLerror {err}", Name, err);
#if DEBUG
            if (ignoredErrors != null && ignoredErrors.Contains(err))
            {
                // ignore
            }
            else
            {
                throw new Exception($"Name={Name} GLerror {err}");
            }
#endif
        }
    }

    bool _initialized = false;

    // Circumvent what I think is a bug in OpenGlControlBase,
    // RequestNextFrameRendering can set _updateQueued to true
    // without actually calling RequestCompositionUpdate
    // (if _compositor is null), leading to essentially a softlock.
    public void RequestNextFrameRenderingIfInitialized()
    {
        if (_initialized)
            RequestNextFrameRendering();
        // If not initialized yet no need to call RequestNextFrameRendering,
        // OpenGlControlBase will basically do it when it's ready to.
    }

    protected override void OnOpenGlInit(GlInterface gl)
    {
        _initialized = true;

        Logger.Debug("Name={Name} in", Name);

        // ignore GL_INVALID_ENUM errors, Avalonia bug. cf https://github.com/AvaloniaUI/Avalonia/issues/13807
        CheckError(gl, new[] { GL_INVALID_ENUM });

        LoadOpenTKBindings(gl);
        CheckError(gl);

        Logger.Debug("GL Version: " + GL.GetString(StringName.Version));
        Logger.Debug("GL Renderer: " + GL.GetString(StringName.Renderer));
        Logger.Debug("GL Vendor: " + GL.GetString(StringName.Vendor));
        Logger.Debug(
            "GL Shading Language Version: " + GL.GetString(StringName.ShadingLanguageVersion)
        );

        try
        {
            OnOpenTKInit();
        }
        catch (Exception e)
        {
            Logger.Error(e, "Unhandled exception raised from OnOpenTKInit");
            throw;
        }
        CheckError(gl);

        Logger.Debug("Name={Name} out", Name);
    }

    private static bool _isOpenTKBindingsLoaded = false;

    private class BindingsContextImpl : IBindingsContext
    {
        private GlInterface _gl;

        public BindingsContextImpl(GlInterface gl)
        {
            _gl = gl;
        }

        public IntPtr GetProcAddress(string procName)
        {
            return _gl.GetProcAddress(procName);
        }
    }

    private void LoadOpenTKBindings(GlInterface gl)
    {
        if (_isOpenTKBindingsLoaded)
            return;
        GL.LoadBindings(new BindingsContextImpl(gl));
        _isOpenTKBindingsLoaded = true;
    }

    protected override void OnOpenGlDeinit(GlInterface gl)
    {
        _initialized = false;
    }

    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        Logger.Trace("Name={Name} in", Name);

        CheckError(gl);

        try
        {
            OnOpenTKRender();
        }
        catch (Exception e)
        {
            Logger.Error(e, "Unhandled exception raised from OnOpenTKRender");
            throw;
        }
        CheckError(gl);

        Logger.Trace("Name={Name} out", Name);
    }

    protected PixelSize GetPixelSize()
    {
        // VisualRoot is set "if the control is attached to a visual tree".
        // This will always be true in OnOpenGlInit and OnOpenGlRender,
        // as the parent class OpenGlControlBase basically waits for
        // OnAttachedToVisualTree to do anything, plus it later checks for
        // VisualRoot to be non-null before calling child init/render.
        // https://github.com/AvaloniaUI/Avalonia/blob/release/11.0.5/src/Avalonia.OpenGL/Controls/OpenGlControlBase.cs
        Utils.Assert(VisualRoot != null);

        // Copy-paste of OpenGlControlBase.GetPixelSize
        var scaling = VisualRoot.RenderScaling;
        return new PixelSize(
            Math.Max(1, (int)(Bounds.Width * scaling)),
            Math.Max(1, (int)(Bounds.Height * scaling))
        );
    }

    protected void SetFullViewport()
    {
        var pixelSize = GetPixelSize();
        GL.Viewport(0, 0, pixelSize.Width, pixelSize.Height);
    }

    protected abstract void OnOpenTKInit();
    protected abstract void OnOpenTKRender();
}
