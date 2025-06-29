using System;
using System.Diagnostics;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;

namespace Z64Utils.Controls.HexTextBox;

// like TextBox but accepts (0[xX])?[ _0-9a-fA-F]+
// Also supports scrollwheel like NumericUpDown,
// and modifier keys (Alt, Ctrl, Shift) for different increments.
public class HexTextBox : TextBox
{
    public static readonly StyledProperty<uint?> ValueProperty = AvaloniaProperty.Register<
        HexTextBox,
        uint?
    >(nameof(Value), defaultValue: null, defaultBindingMode: BindingMode.TwoWay);
    public uint? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly StyledProperty<uint> ScrollAmountBaseProperty =
        AvaloniaProperty.Register<HexTextBox, uint>(nameof(ScrollAmountBase), defaultValue: 0x10);
    public uint ScrollAmountBase
    {
        get => GetValue(ScrollAmountBaseProperty);
        set => SetValue(ScrollAmountBaseProperty, value);
    }

    public static readonly StyledProperty<bool> ReverseScrollDirectionProperty =
        AvaloniaProperty.Register<HexTextBox, bool>(
            nameof(ReverseScrollDirection),
            defaultValue: false
        );
    public bool ReverseScrollDirection
    {
        get => GetValue(ReverseScrollDirectionProperty);
        set => SetValue(ReverseScrollDirectionProperty, value);
    }

    public HexTextBox()
    {
        PropertyChanged += (sender, e) =>
        {
            switch (e.Property.Name)
            {
                case nameof(Value):
                    if (Value == null)
                        Text = "";
                    else if (Text == null || ParseHexText(Text) != Value)
                        Text = $"0x{Value:X8}";
                    break;
                case nameof(Text):
                    if (Text == null || Text == "")
                        Value = null;
                    else
                    {
                        Value = ParseHexText(Text);

                        if (Value == null)
                        {
                            // Invalid value -> red text
                            Classes.Add("badHexValue");
                        }
                        else
                        {
                            // Valid value -> restore text color
                            Classes.Remove("badHexValue");
                        }
                    }
                    break;
            }
        };

        PointerWheelChanged += (sender, e) =>
        {
            ScrollImpl(Math.Sign(e.Delta.Y), e.KeyModifiers);
        };

        KeyDown += (sender, e) =>
        {
            if (e.Key == Key.Down)
                ScrollImpl(-1, e.KeyModifiers);
            else if (e.Key == Key.Up)
                ScrollImpl(+1, e.KeyModifiers);
        };
    }

    private void ScrollImpl(int direction, KeyModifiers keyModifiers)
    {
        if (Value == null)
            return;
        var offset = GetScrollOffset(keyModifiers);
        if (offset == 0)
            return;
        var newValue = Value + offset * direction * (ReverseScrollDirection ? -1 : 1);
        if (newValue < 0)
            Value = 0;
        else if (newValue > uint.MaxValue)
            Value = uint.MaxValue;
        else
            Value = (uint)newValue;
    }

    private uint GetScrollOffset(KeyModifiers keyModifiers)
    {
        switch (keyModifiers)
        {
            case KeyModifiers.Shift | KeyModifiers.Alt:
                return 1;
            case KeyModifiers.Control | KeyModifiers.Alt:
                return 1;
            case KeyModifiers.Alt:
                return ScrollAmountBase / 4;
            case KeyModifiers.Control:
                return ScrollAmountBase * 16;
            case KeyModifiers.Shift:
                return ScrollAmountBase * 16;
            case KeyModifiers.Shift | KeyModifiers.Control:
                return ScrollAmountBase * 16 * 16;
            case KeyModifiers.None:
                return ScrollAmountBase;
            default:
                return 0;
        }
    }

    private static uint? ParseHexText(string s)
    {
        s = s.ToUpper();
        if (s.StartsWith("0X"))
            s = s.Substring(2);
        s = s.Replace(" ", "");
        s = s.Replace("_", "");
        uint v;
        if (uint.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out v))
            return v;
        else
            return null;
    }
}
