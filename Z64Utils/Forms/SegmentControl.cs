using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Common;
using F3DZEX;

namespace Z64.Forms
{
    public partial class SegmentControl : UserControl
    {
        public event EventHandler<SegmentEditorForm.SegmentChangedEvent>? SegmentChanged;
        public int SegmentID
        {
            get { return _segmentId; }
            set
            {
                if (value < 0)
                    _segmentId = 0;
                else if (value >= 15)
                    _segmentId = 15;
                else
                    _segmentId = value;

                indexLabel.Text = $"{SegmentID:D2} :";
            }
        }

        private int _segmentId;
        Z64Game? _game;

        public SegmentControl()
        {
            InitializeComponent();
            indexLabel.Text = $"{SegmentID:D2} :";
        }

        public void SetGame(Z64Game? game)
        {
            _game = game;
        }

        public void SetSegment(Memory.Segment seg)
        {
            label1.Text = seg.Label;
        }

        private void importFileButton_Click(object sender, EventArgs e)
        {
            SegmentEditForm form = new SegmentEditForm(_game);
            form.Text += " " + SegmentID;
            if (form.ShowDialog() == DialogResult.OK)
            {
                Utils.Assert(form.ResultSegment != null);
                SetSegment(form.ResultSegment);
                SegmentChanged?.Invoke(this, new(_segmentId, form.ResultSegment));
            }
        }
    }
}
