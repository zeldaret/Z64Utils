using System;
using System.Diagnostics;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;

namespace Z64Utils_Avalonia;

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
            if (Value == null)
                return;
            int offset;
            switch (e.KeyModifiers)
            {
                case KeyModifiers.Shift | KeyModifiers.Alt:
                    offset = 1;
                    break;
                case KeyModifiers.Control | KeyModifiers.Alt:
                    offset = 1;
                    break;
                case KeyModifiers.Alt:
                    offset = 4;
                    break;
                case KeyModifiers.Control:
                    offset = 0x100;
                    break;
                case KeyModifiers.Shift:
                    offset = 0x100;
                    break;
                case KeyModifiers.Shift | KeyModifiers.Control:
                    offset = 0x1000;
                    break;
                case KeyModifiers.None:
                    offset = 0x10;
                    break;
                default:
                    offset = 0;
                    break;
            }
            if (offset == 0)
                return;
            var newValue = Value + offset * -Math.Sign(e.Delta.Y);
            if (newValue < 0)
                Value = 0;
            else if (newValue > uint.MaxValue)
                Value = uint.MaxValue;
            else
                Value = (uint)newValue;
        };
    }

    public static uint? ParseHexText(string s)
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
