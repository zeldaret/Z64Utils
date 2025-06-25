using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using Common;

namespace Z64Utils_Avalonia;

public class CheckPatternControl : Control
{
    public static readonly StyledProperty<int> CheckerSizeProperty = AvaloniaProperty.Register<
        CheckPatternControl,
        int
    >(nameof(CheckerSize), defaultValue: 10);
    public int CheckerSize
    {
        get => GetValue(CheckerSizeProperty);
        set => SetValue(CheckerSizeProperty, value);
    }

    private ImageBrush? _checkPatternTileBrush;

    private Dictionary<ThemeVariant, (Color, Color)> _checkColors = new()
    {
        { ThemeVariant.Dark, (Color.FromRgb(50, 50, 50), Color.FromRgb(0, 0, 0)) },
        { ThemeVariant.Light, (Color.FromRgb(255, 255, 255), Color.FromRgb(200, 200, 200)) },
    };

    public CheckPatternControl()
    {
        ActualThemeVariantChanged += (sender, e) => UpdateCheckPatternTileBrush();
    }

    [MemberNotNull(nameof(_checkPatternTileBrush))]
    private void UpdateCheckPatternTileBrush()
    {
        var themeVariant = ActualThemeVariant ?? ThemeVariant.Default;
        if (!_checkColors.ContainsKey(themeVariant))
        {
            Utils.Assert(_checkColors.ContainsKey(ThemeVariant.Light));
            themeVariant = ThemeVariant.Light;
        }
        var (evenColor, oddColor) = _checkColors[themeVariant];

        int repeatsX = 10,
            repeatsY = 10; // purely for optimization so ImageBrush has less repeats to do
        var bitmap = new RenderTargetBitmap(
            new PixelSize(CheckerSize * 2 * repeatsX, CheckerSize * 2 * repeatsY)
        );
        using (var dc = bitmap.CreateDrawingContext())
        {
            dc.FillRectangle(new SolidColorBrush(oddColor), new Rect(bitmap.Size));
            Rect rect = new(new Size(CheckerSize, CheckerSize));
            for (int i = 0; i < repeatsX; i++)
            {
                int x = i * 2 * CheckerSize;
                for (int j = 0; j < repeatsY; j++)
                {
                    int y = j * 2 * CheckerSize;
                    dc.FillRectangle(
                        new SolidColorBrush(evenColor),
                        rect.WithX(x + 0).WithY(y + 0)
                    );
                    dc.FillRectangle(
                        new SolidColorBrush(evenColor),
                        rect.WithX(x + CheckerSize).WithY(y + CheckerSize)
                    );
                }
            }
        }
        _checkPatternTileBrush = new ImageBrush(bitmap)
        {
            TileMode = TileMode.Tile,
            DestinationRect = new RelativeRect(bitmap.Size, RelativeUnit.Absolute),
        };
    }

    public override void Render(DrawingContext context)
    {
        if (_checkPatternTileBrush == null)
            UpdateCheckPatternTileBrush();
        context.FillRectangle(_checkPatternTileBrush, Bounds);
    }
}
