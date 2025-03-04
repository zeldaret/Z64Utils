using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using F3DZEX.Command;

#nullable enable

namespace Z64.Forms
{
    public partial class AnalyzerSettingsForm : MicrosoftFontForm
    {
        public Z64ObjectAnalyzer.Config Result { get; set; }

        public AnalyzerSettingsForm()
        {
            InitializeComponent();
            labelOpCodeListError.Text = "";
            labelPatternError.Text = "";
            buttonNormal_Click(null, null);
        }

        private void buttonNoRestriction_Click(object sender, EventArgs e)
        {
            Result = new Z64ObjectAnalyzer.Config();
            UpdateTextBoxes();
        }

        [MemberNotNull(nameof(Result))]
        private void buttonNormal_Click(object? sender, EventArgs? e)
        {
            Result = new Z64ObjectAnalyzer.Config();
            Result.ImprobableOpCodes = new List<CmdID>()
            {
                CmdID.G_BRANCH_Z,
                CmdID.G_CULLDL,
                CmdID.G_NOOP,
                CmdID.G_SPNOOP,
                CmdID.G_LOAD_UCODE,
            };
            UpdateTextBoxes();
        }

        private void buttonRestrivtive_Click(object sender, EventArgs e)
        {
            Result = new Z64ObjectAnalyzer.Config();
            Result.ImprobableOpCodes = new List<CmdID>()
            {
                CmdID.G_BRANCH_Z,
                CmdID.G_CULLDL,
                CmdID.G_NOOP,
                CmdID.G_SPNOOP,
                CmdID.G_LOAD_UCODE,
            };
            var patternsStr = new List<string>()
            {
                "G_VTX: *, G_TRI1|G_TRI2",
                "G_TRI1: G_TRI1|G_TRI2|G_VTX, *",
                "G_TRI2: G_TRI1|G_TRI2|G_VTX, *",
                "G_LOADTLUT: *, G_RDPPIPESYNC",
                "G_LOADBLOCK: *, G_RDPPIPESYNC",
                "G_RDPHALF_1: *, G_LOAD_UCODE",
                "G_RDPHALF_1: G_TEXRECT, *, G_RDPHALF_2",
                "G_RDPHALF_1: G_TEXRECTFLIP, *, G_RDPHALF_2",
                "G_RDPHALF_2: G_TEXRECT, G_RDPHALF_1, *",
                "G_RDPHALF_2: G_TEXRECTFLIP, G_RDPHALF_1, *",
                "G_TEXRECT: *, G_RDPHALF_1, G_RDPHALF_2",
                "G_TEXRECTFLIP: *, G_RDPHALF_1, G_RDPHALF_2",
                "G_RDPLOADSYNC: *, G_LOADBLOCK",
                "G_RDPLOADSYNC: *, G_LOADTLUT",
            };
            Result.Patterns = new List<Z64ObjectAnalyzer.Config.OpCodePattern>();
            foreach (var patternStr in patternsStr)
            {
                var pattern = Z64ObjectAnalyzer.Config.OpCodePattern.Parse(patternStr);
                Debug.Assert(pattern != null);
                Result.Patterns.Add(pattern);
            }
            UpdateTextBoxes();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string text =
                @"Format : <id>: <token1>, <token2>, ...

id : the opcode that should follow the pattern
token : can be ""*"", ""?"" or an opcode id to check
""*"" means the current opcode (this can only be present once)
""?"" means any opcode
you can specify multiple valid opcode ids for a single token: e.g. ""G_NOOP|G_LOADTLUT"" means both G_NOOP and G_LOADTLUT are accepted

Example:
""G_NOOP: G_LOADTLUT, *, ?, ?, G_TEXRECT|G_TEXRECTFLIP""
means: When encountering a G_NOOP opcode, check if the last instruction is G_LOADTLUT, don't check the 2 following instructions and check if the 3rd instruction is either G_TEXRECT or G_TEXRECTFLIP";
            TextForm form = new TextForm(SystemIcons.Question, "Syntax Help", text);
            form.Show();
        }

        private static bool ValidOpCodeID(string id)
        {
            var values = Enum.GetValues(typeof(CmdID));
            foreach (var v in values)
                if (v.ToString() == id)
                    return true;
            return false;
        }

        private void UpdateOKButton()
        {
            buttonOK.Enabled = labelOpCodeListError.Text == "" && labelPatternError.Text == "";
        }

        private void UpdateTextBoxes()
        {
            StringWriter sw = new StringWriter();
            foreach (var id in Result.ImprobableOpCodes)
                sw.WriteLine(id);
            textBoxOpCodeList.Text = sw.ToString();

            sw = new StringWriter();
            foreach (var p in Result.Patterns)
                sw.WriteLine(p.ToString());
            textBoxPatterns.Text = sw.ToString();
        }

        private void textBoxOpCodeList_TextChanged(object sender, EventArgs e)
        {
            labelOpCodeListError.Text = "";
            List<CmdID> ids = new List<CmdID>();
            var lines = textBoxOpCodeList
                .Text.Replace(" ", "")
                .Split(new string[] { "\r\n" }, StringSplitOptions.None);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i] == "")
                    continue;

                if (!ValidOpCodeID(lines[i]))
                {
                    labelOpCodeListError.Text = $"Error at line {i}";
                    return;
                }
                ids.Add((CmdID)Enum.Parse(typeof(CmdID), lines[i]));
            }
            Result.ImprobableOpCodes = ids;
            UpdateOKButton();
        }

        private void textBoxPatterns_TextChanged(object sender, EventArgs e)
        {
            labelPatternError.Text = "";
            var patterns = new List<Z64ObjectAnalyzer.Config.OpCodePattern>();
            var lines = textBoxPatterns
                .Text.Replace(" ", "")
                .Split(new string[] { "\r\n" }, StringSplitOptions.None);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i] == "")
                    continue;

                var p = Z64ObjectAnalyzer.Config.OpCodePattern.Parse(lines[i]);
                if (p == null)
                {
                    labelPatternError.Text = $"Error at line {i}";
                    return;
                }
                patterns.Add(p);
            }
            Result.Patterns = patterns;
            UpdateOKButton();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
