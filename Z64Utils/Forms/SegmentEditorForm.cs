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

namespace Z64.Forms
{
    public partial class SegmentEditorForm : MicrosoftFontForm
    {
        public class SegmentChangedEvent
        {
            public int SegmentID;
            public F3DZEX.Memory.Segment Segment;

            public SegmentChangedEvent(int segmentID, F3DZEX.Memory.Segment segment)
            {
                SegmentID = segmentID;
                Segment = segment;
            }
        }

        public event EventHandler<SegmentChangedEvent>? SegmentsChanged;

        public SegmentEditorForm(Z64Game? game, F3DZEX.Render.Renderer renderer)
        {
            InitializeComponent();
            for (int i = 0; i < 16; i++)
            {
                SegmentControl seg = (SegmentControl)Controls[$"segmentControl{i}"];
                seg.SetGame(game);
                seg.SetSegment(renderer.Memory.Segments[i]);
                seg.SegmentChanged += Seg_SegmentChanged;
            }
        }

        private void Seg_SegmentChanged(object? sender, SegmentChangedEvent e)
        {
            SegmentsChanged?.Invoke(this, e);
        }
    }
}
