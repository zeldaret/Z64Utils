using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Common;

namespace Z64.Forms
{
    public partial class AboutForm : MicrosoftFontForm
    {
        public AboutForm()
        {
            InitializeComponent();
            Icon = SystemIcons.Question;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Utils.OpenBrowser(@"https://icons8.com");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Utils.OpenBrowser(@"https://github.com/Random06457/Z64Utils");
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Utils.OpenBrowser(@"https://wiki.cloudmodding.com");
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Utils.OpenBrowser(@"https://github.com/zeldaret");
        }
    }
}
