using System.Text;
using Avalonia;
using Avalonia.Controls;
using Common;

namespace Z64Utils.Views.OHED;

public partial class HexViewerControl : UserControl
{
    private const int BYTES_PER_ROW = 16;

    public static readonly StyledProperty<byte[]?> DataBytesProperty = AvaloniaProperty.Register<
        HexViewerControl,
        byte[]?
    >(nameof(DataBytes), defaultValue: null);
    public byte[]? DataBytes
    {
        get => GetValue(DataBytesProperty);
        set => SetValue(DataBytesProperty, value);
    }

    public static readonly StyledProperty<uint> FirstByteAddressProperty =
        AvaloniaProperty.Register<HexViewerControl, uint>(
            nameof(FirstByteAddress),
            defaultValue: 0
        );
    public uint FirstByteAddress
    {
        get => GetValue(FirstByteAddressProperty);
        set => SetValue(FirstByteAddressProperty, value);
    }

    public HexViewerControl()
    {
        InitializeComponent();
        PropertyChanged += (sender, e) =>
        {
            switch (e.Property.Name)
            {
                case nameof(DataBytes):
                case nameof(FirstByteAddress):
                    var data = DataBytes ?? new byte[0];
                    var firstByteAddress = FirstByteAddress;

                    var headerText = new StringBuilder();
                    var contentText = new StringBuilder();

                    // round down address of first byte to previous multiple
                    uint firstRowAddress = firstByteAddress / BYTES_PER_ROW * BYTES_PER_ROW;
                    // round down address of last byte to previous multiple
                    uint lastRowAddress =
                        data.Length == 0
                            ? firstRowAddress
                            : (firstByteAddress + (uint)data.Length - 1)
                                / BYTES_PER_ROW
                                * BYTES_PER_ROW;
                    int maxRowAddressNumChars = lastRowAddress.ToString("X").Length;
                    string romAddressFormat = "{0:X" + maxRowAddressNumChars + "} ";

                    headerText.Append(new string(' ', string.Format(romAddressFormat, 0).Length));
                    headerText[0] = '_';
                    Utils.Assert(BYTES_PER_ROW <= 0xFF);
                    for (int i = 0; i < BYTES_PER_ROW; i++)
                    {
                        headerText.AppendFormat(" {0:X2}", i);
                    }

                    for (
                        uint rowAddress = firstRowAddress;
                        rowAddress <= lastRowAddress;
                        rowAddress += BYTES_PER_ROW
                    )
                    {
                        if (rowAddress != firstRowAddress)
                            contentText.Append('\n');
                        contentText.AppendFormat(romAddressFormat, rowAddress);
                        for (uint i = 0; i < BYTES_PER_ROW; i++)
                        {
                            uint address = rowAddress + i;
                            if (address < firstByteAddress)
                            {
                                contentText.Append("   ");
                            }
                            else
                            {
                                uint offset = address - firstByteAddress;
                                if (offset < data.Length)
                                {
                                    byte b = data[offset];
                                    contentText.AppendFormat(" {0:X2}", b);
                                }
                                else
                                {
                                    contentText.Append("   ");
                                }
                            }
                        }
                    }

                    HeaderTextBlock.Text = headerText.ToString();
                    ContentTextBlock.Text = contentText.ToString();
                    break;
            }
        };
    }
}
