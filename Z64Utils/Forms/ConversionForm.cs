using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

#nullable enable

namespace Z64.Forms
{
    public partial class ConversionForm : MicrosoftFontForm
    {
        Z64Game _game;

        public ConversionForm(Z64Game game)
        {
            InitializeComponent();
            comboBox1.SelectedIndex = 0;
            _game = game;
        }

        private void UpdateOutput(object sender, EventArgs e)
        {
            StringWriter sw = new StringWriter();
            if (
                uint.TryParse(
                    textBoxInput.Text,
                    NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture,
                    out uint addr
                )
            )
            {
                switch (comboBox1.SelectedIndex)
                {
                    case 0: // ROM
                    {
                        for (int i = 0; i < _game.GetFileCount(); i++)
                        {
                            var file = _game.GetFileFromIndex(i);
                            if (addr >= file.RomStart && addr < file.RomEnd)
                            {
                                int diff = (int)(addr - file.RomStart);
                                if (file.Compressed)
                                    sw.WriteLine(
                                        "Cannot determine offset because the file is compressed"
                                    );
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
                    case 1: // VROM
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
                                sw.WriteLine(
                                    $"File: \"{_game.GetFileName(file.VRomStart)}\" + 0x{diff:X}"
                                );
                                if (_game.Memory.VromToVram(addr, out uint vram))
                                    sw.WriteLine(
                                        $"VRAM: {vram:X8} ({(vram - diff):X8} + 0x{diff:X})"
                                    );
                                else
                                    sw.WriteLine("VRAM: Not in VRAM");
                                break;
                            }
                        }
                        break;
                    }
                    case 2: // VRAM
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
            }
            else
            {
                sw.WriteLine("Invalid address");
            }

            textBoxOutput.Text = sw.ToString();
        }
    }
}
