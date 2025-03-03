using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Z64.Forms
{
    public class MicrosoftFontForm : Form
    {
        // https://docs.microsoft.com/en-us/dotnet/core/compatibility/fx-core#default-control-font-changed-to-segoe-ui-9-pt
        public MicrosoftFontForm()
            : base()
        {
            this.Font = new System.Drawing.Font(
                "Microsoft Sans Serif",
                8.25F,
                System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point
            );
        }
    }
}
