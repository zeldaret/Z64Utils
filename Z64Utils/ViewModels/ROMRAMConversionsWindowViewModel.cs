using System;
using System.Globalization;
using System.IO;
using Common;
using CommunityToolkit.Mvvm.ComponentModel;
using Z64;

namespace Z64Utils.ViewModels;

public partial class ROMRAMConversionsWindowViewModel : ObservableObject
{
    private Z64Game _game;

    [ObservableProperty]
    private int _inputAddressTypeIndex;

    [ObservableProperty]
    private string _inputAddressStr = "";

    [ObservableProperty]
    private string _outputText = "";

    public enum InputAddressTypeEnum
    {
        ROM,
        VROM,
        VRAM,
    }; // keep in sync with the ComboBoxItem elements in the view axaml

    [ObservableProperty]
    private InputAddressTypeEnum _inputAddressType;

    [ObservableProperty]
    private uint? _inputAddress;

    public ROMRAMConversionsWindowViewModel(Z64Game game)
    {
        _game = game;
        PropertyChanged += (sender, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(InputAddressTypeIndex):
                    Utils.Assert(InputAddressTypeIndex >= 0 && InputAddressTypeIndex < 3);
                    InputAddressType = Enum.GetValues<InputAddressTypeEnum>()[
                        InputAddressTypeIndex
                    ];
                    break;
                case nameof(InputAddressStr):
                    string inAddrStr = InputAddressStr;
                    inAddrStr = inAddrStr.ToUpper();
                    if (inAddrStr.StartsWith("0X"))
                        inAddrStr = inAddrStr.Substring(2);
                    inAddrStr = inAddrStr.Replace(" ", "");
                    inAddrStr = inAddrStr.Replace("_", "");
                    uint inAddr;
                    if (
                        uint.TryParse(
                            inAddrStr,
                            NumberStyles.HexNumber,
                            CultureInfo.InvariantCulture,
                            out inAddr
                        )
                    )
                        InputAddress = inAddr;
                    else
                        InputAddress = null;
                    break;
                case nameof(InputAddressType):
                case nameof(InputAddress):
                    UpdateOutput();
                    break;
            }
        };
    }

    public void UpdateOutput()
    {
        if (InputAddressStr == "")
        {
            OutputText = "";
            return;
        }
        if (InputAddress == null)
        {
            OutputText = "Invalid input address";
            return;
        }
        OutputText = $"Input: {InputAddressType} 0x{InputAddress:X}";
        OutputText += "\n\n" + UpdateOutputImpl();
    }

    //Z64.Forms.ConversionForm.UpdateOutput
    private string UpdateOutputImpl()
    {
        Utils.Assert(InputAddress != null);
        uint addr = (uint)InputAddress;
        StringWriter sw = new StringWriter();
        switch (InputAddressType)
        {
            case InputAddressTypeEnum.ROM:
            {
                for (int i = 0; i < _game.GetFileCount(); i++)
                {
                    var file = _game.GetFileFromIndex(i);
                    if (addr >= file.RomStart && addr < file.RomEnd)
                    {
                        int diff = (int)(addr - file.RomStart);
                        if (file.Compressed)
                            sw.WriteLine("Cannot determine offset because the file is compressed");
                        string vromStr = file.Compressed
                            ? $"{file.VRomStart:X8} + ?"
                            : $"{file.VRomStart + diff:X8} ({file.VRomStart:X8} + 0x{diff:X})";
                        sw.WriteLine($"VROM: {vromStr}");

                        sw.WriteLine(
                            $"File: \"{_game.GetFileName(file.VRomStart)}\" + "
                                + (file.Compressed ? "?" : $"0x{diff:X}")
                        );
                        if (_game.Memory.VromToVram((uint)file.VRomStart, out uint vram))
                        {
                            string vramStr = file.Compressed
                                ? $"{vram:X8} + ?"
                                : $"{vram + diff:X8} ({vram:X8} + 0x{diff:X})";
                            sw.WriteLine($"VRAM: {vramStr}");
                        }
                        else
                            sw.WriteLine("VRAM: Not in VRAM");
                        break;
                    }
                }
                break;
            }
            case InputAddressTypeEnum.VROM:
            {
                for (int i = 0; i < _game.GetFileCount(); i++)
                {
                    var file = _game.GetFileFromIndex(i);
                    if (addr >= file.VRomStart && addr < file.VRomEnd)
                    {
                        int diff = (int)(addr - file.VRomStart);
                        sw.WriteLine(
                            $"ROM: {file.RomStart + diff:X8} ({file.RomStart:X8} + 0x{diff:X})"
                        );
                        sw.WriteLine($"File: \"{_game.GetFileName(file.VRomStart)}\" + 0x{diff:X}");
                        if (_game.Memory.VromToVram(addr, out uint vram))
                            sw.WriteLine($"VRAM: {vram:X8} ({(vram - diff):X8} + 0x{diff:X})");
                        else
                            sw.WriteLine("VRAM: Not in VRAM");
                        break;
                    }
                }
                break;
            }
            case InputAddressTypeEnum.VRAM:
            {
                if (_game.Memory.VramToVrom(addr, out uint vrom))
                {
                    for (int i = 0; i < _game.GetFileCount(); i++)
                    {
                        var file = _game.GetFileFromIndex(i);
                        if (vrom >= file.VRomStart && vrom < file.VRomEnd)
                        {
                            int diff = (int)(vrom - file.VRomStart);
                            sw.WriteLine(
                                $"VROM: {(file.VRomStart + diff):X8} ({file.VRomStart:X8} + 0x{diff:X})"
                            );
                            sw.WriteLine(
                                $"ROM: {(file.RomStart + diff):X8} ({file.RomStart:X8} + 0x{diff:X})"
                            );
                            sw.WriteLine(
                                $"File: \"{_game.GetFileName(file.VRomStart)}\" + 0x{diff:X}"
                            );
                            break;
                        }
                    }
                }
                else
                {
                    sw.WriteLine("Cannot convert to ROM/VROM");
                }
                break;
            }
        }

        return sw.ToString();
    }
}
