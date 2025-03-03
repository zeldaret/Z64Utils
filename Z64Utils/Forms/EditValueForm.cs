using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Z64.Forms
{
    public partial class EditValueForm : MicrosoftFontForm
    {
        Func<string, string> _callback = s => null;
        public string Result { get; private set; }

        public EditValueForm(
            string title,
            string desc,
            Func<string, string> valid,
            string defaultValue = null
        )
        {
            InitializeComponent();
            if (valid != null)
                _callback = valid;
            labelDesc.Text = desc;
            labelWarn.Text = "";
            Text = title;
            textBox1.Text = defaultValue ?? "";
            textBox1_TextChanged(null, null);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Result = textBox1.Text;
            Close();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            var error = _callback(textBox1.Text);
            buttonOK.Enabled = string.IsNullOrEmpty(error);
            labelWarn.Text = error;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (buttonOK.Enabled && e.KeyCode == Keys.Enter)
            {
                buttonOK_Click(null, null);
            }
        }
    }
}
