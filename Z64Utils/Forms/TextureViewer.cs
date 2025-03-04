using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using N64;

#nullable enable

namespace Z64.Forms
{
    public partial class TextureViewer : MicrosoftFontForm
    {
        Z64Game _game;

        public TextureViewer(Z64Game game)
        {
            _game = game;
            InitializeComponent();

            var values = Enum.GetValues(typeof(N64TexFormat));
            comboBoxTexFmt.Items.Clear();
            foreach (var value in values)
                comboBoxTexFmt.Items.Add(value.ToString());
            comboBoxTexFmt.SelectedIndex = 0;

            comboBoxAddressType.SelectedIndex = 0;
        }

        private byte[] ReadBytes(uint addr, int size)
        {
            if (comboBoxAddressType.SelectedIndex == 0) // VRAM
            {
                return _game.Memory.ReadBytes(addr, size);
            }
            else // VROM
            {
                for (int i = 0; i < _game.GetFileCount(); i++)
                {
                    var file = _game.GetFileFromIndex(i);
                    if (addr >= file.VRomStart && addr < file.VRomEnd)
                    {
                        byte[] buffer = new byte[size];
                        Debug.Assert(file.Valid());
                        Buffer.BlockCopy(
                            file.Data,
                            (int)(addr - (uint)file.VRomStart),
                            buffer,
                            0,
                            size
                        );
                        return buffer;
                    }
                }
            }
            throw new Exception();
        }

        private void UpdateTexture(object? sender = null, EventArgs? e = null)
        {
            N64TexFormat fmt = (N64TexFormat)comboBoxTexFmt.SelectedIndex;
            int w = (int)valueW.Value;
            int h = (int)valueH.Value;
            int texSize = N64Texture.GetTexSize(w * h, fmt);

            if (
                !uint.TryParse(
                    textBoxTexAddr.Text,
                    NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture,
                    out uint texAddr
                )
            )
            {
                return;
            }

            byte[]? tlut = null;
            if (
                (fmt == N64TexFormat.CI4 || fmt == N64TexFormat.CI8)
                && uint.TryParse(
                    textBoxTlutAddr.Text,
                    NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture,
                    out uint tlutAddr
                )
            )
            {
                try
                {
                    if (fmt == N64TexFormat.CI4)
                        tlut = ReadBytes(tlutAddr, 64);
                    else if (fmt == N64TexFormat.CI8)
                        tlut = ReadBytes(tlutAddr, 256);
                }
                catch (Exception)
                {
                    return;
                }
            }

            try
            {
                byte[] tex = ReadBytes(texAddr, texSize);
                textureBox1.Image = N64Texture.DecodeBitmap(w, h, fmt, tex, tlut);
            }
            catch (Exception)
            {
                return;
            }
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                textureBox1.Focus();
            }
        }
    }
}
