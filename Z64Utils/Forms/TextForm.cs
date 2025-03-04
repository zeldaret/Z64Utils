using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

#nullable enable

namespace Z64.Forms
{
    public partial class TextForm : MicrosoftFontForm
    {
        public TextForm(Icon icon, string title, string message)
        {
            InitializeComponent();
            Icon = icon;
            Text = title;
            textBox1.Text = message;
        }

        private void TextForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.Enter)
            {
                this.Close();
            }
        }
    }
}
