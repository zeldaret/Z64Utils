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
using RDP;

namespace Z64.Forms
{
    public partial class DisasmSettingsForm : MicrosoftFontForm
    {
        public static DisasmSettingsForm Instance { get; set; }

        F3DZEX.Disassembler _disas;

        public static void OpenInstance()
        {
            if (Instance == null)
            {
                Instance = new DisasmSettingsForm();
                Instance.Show();
            }
            else
            {
                Instance.Activate();
            }
        }

        private DisasmSettingsForm()
        {
            InitializeComponent();

            byte[] dlist = new byte[] { 0x01, 0x01, 0x20, 0x24, 0x06, 0x00, 0x0F, 0xC8 };
            _disas = new F3DZEX.Disassembler(new F3DZEX.Command.Dlist(dlist, 0x060002C8));

            propertyGrid1.SelectedObject = F3DZEX.Disassembler.StaticConfig;
            DisassemblyOptionForm_OnOptionUpdate(null, null);
        }

        private void DisassemblyOptionForm_OnOptionUpdate(
            object sender,
            PropertyValueChangedEventArgs e
        )
        {
            Application.OpenForms.OfType<DisasmForm>().ToList().ForEach(f => f.UpdateDisassembly());
            Application
                .OpenForms.OfType<ObjectAnalyzerForm>()
                .ToList()
                .ForEach(f => f.UpdateDisassembly());

            var lines = _disas.Disassemble();

            StringWriter sw = new StringWriter();
            foreach (var line in lines)
                sw.WriteLine(line);

            previewTextBox.Text = sw.ToString();
        }

        private void DisasmSettingsForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Instance = null;
        }
    }
}
