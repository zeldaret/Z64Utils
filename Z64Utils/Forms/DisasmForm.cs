using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Common;
using F3DZEX.Command;

#nullable enable

namespace Z64.Forms
{
    public partial class DisasmForm : MicrosoftFontForm
    {
        Dlist? _dlist;

        public DisasmForm(bool showByteInputBox = false, string? defaultText = null)
        {
            InitializeComponent();

            _dlist = new Dlist();

            if (!showByteInputBox)
            {
                textBox_bytes.Visible = label_bytes.Visible = label_disas.Visible = false;
                textBox_disassembly.Location = new Point(10, 30);
                textBox_disassembly.Size = new Size(Width - 40, Height - 80);
            }

            textBox_disassembly.Text = defaultText ?? "";
        }

        public void UpdateDlist(Dlist? dlist)
        {
            _dlist = dlist;
            UpdateDisassembly();
        }

        public void SetMessage(string text)
        {
            textBox_disassembly.Text = text;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            bool valid = Utils.IsValidHex(textBox_bytes.Text);
            textBox_disassembly.Text = "";

            _dlist = new Dlist();

            if (valid)
            {
                byte[] data = Utils.HexToBytes(textBox_bytes.Text);
                try
                {
                    _dlist = new Dlist(data);
                }
                catch
                {
                    valid = false;
                }
            }
            label_bytes.ForeColor = valid ? Color.Green : Color.Red;

            UpdateDisassembly();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            DisasmSettingsForm.OpenInstance();
        }

        public void UpdateDisassembly()
        {
            F3DZEX.Disassembler disas = new F3DZEX.Disassembler(_dlist);
            var lines = disas.Disassemble();
            StringWriter sw = new StringWriter();
            foreach (var line in lines)
                sw.Write($"{line}\r\n");

            textBox_disassembly.Text = sw.ToString();
        }
    }
}
