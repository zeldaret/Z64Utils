using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;
using OpenTK;
using RDP;
using System.IO;
using System.Globalization;
using Syroot.BinaryData;
using Z64;
using Common;
using F3DZEX.Command;

namespace Z64.Forms
{
    public partial class DListViewerForm : MicrosoftFontForm
    {
        class RenderRoutine
        {
            public uint Address;
            public int X;
            public int Y;
            public int Z;
            public Dlist Dlist;

            public RenderRoutine(uint addr, int x = 0, int y = 0, int z = 0)
            {
                Address = addr;
                X = x;
                Y = y;
                Z = z;
                Dlist = null;
            }

            public override string ToString() => $"{Address:X8} [{X};{Y};{Z}]";
        }

        public static DListViewerForm Instance { get; set; }

        string _dlistError;
        Z64Game _game;
        F3DZEX.Render.Renderer _renderer;
        SegmentEditorForm _segForm;
        DisasmForm _disasForm;
        SettingsForm _settingsForm;
        F3DZEX.Render.Renderer.Config _rendererCfg;

        List<RenderRoutine> _routines;


        private DListViewerForm(Z64Game game)
        {
            _game = game;
            _rendererCfg = new F3DZEX.Render.Renderer.Config();

            InitializeComponent();
            Toolkit.Init();

            _renderer = new F3DZEX.Render.Renderer(game, _rendererCfg);
            modelViewer.RenderCallback = RenderCallback;


            RemoveRoutineMenuItem.Visible = false;
            EditRoutineMenuItem.Visible = false;

            _routines = new List<RenderRoutine>();
            DecodeDlists();


            if ((Control.ModifierKeys & Keys.Control) == 0)
            {
                for (int i = 8; i < 16; i++)
                {
                    if (i != 13)
                        _renderer.Memory.Segments[i] = F3DZEX.Memory.Segment.FromFill("Empty Dlist", new byte[] { 0xDF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
                }
            }


            NewRender();
        }

        void DecodeDlists()
        {
            _renderer.ClearErrors();
            _dlistError = null;

            foreach (RenderRoutine routine in _routines)
            {
                try
                {
                    routine.Dlist = _renderer.GetDlist(routine.Address);
                }
                catch (Exception ex)
                {
                    _dlistError = $"Error while decoding dlist 0x{routine.Address:X8} : {ex.Message}";
                    return;
                }
            }
        }

        void RenderCallback(Matrix4 proj, Matrix4 view)
        {
            _renderer.RenderStart(proj, view);

            if (_dlistError != null)
            {
                toolStripStatusErrorLabel.Text = _dlistError;
                return;
            }

            foreach (var routine in _routines)
            {
                _renderer.ModelMtxStack.Push(Matrix4.CreateTranslation(routine.X, routine.Y, routine.Z));
                _renderer.RenderDList(routine.Dlist);
                _renderer.ModelMtxStack.Pop();
            }

            toolStripStatusErrorLabel.Text = _renderer.RenderFailed()
                ? $"RENDER ERROR AT 0x{_renderer.RenderErrorAddr:X8}! ({_renderer.ErrorMsg})"
                : "";
        }
        private void NewRender(object sender = null, EventArgs e = null)
        {
            _renderer.ClearErrors();

            toolStripStatusErrorLabel.Text = "";

            modelViewer.Render();
        }





        public static void OpenInstance(Z64Game game)
        {
            if (Instance == null)
            {
                Instance = new DListViewerForm(game);
                Instance.Show();
            }
            else
            {
                Instance.Activate();
            }
        }

        public void SetSegment(int index, F3DZEX.Memory.Segment segment)
        {
            if (index >= 0 && index < F3DZEX.Memory.Segment.COUNT)
            {
                _renderer.Memory.Segments[index] = segment;
                DecodeDlists();
                NewRender();
            }
        }

        public void SetSingleDlist(uint vaddr, int x = 0, int y = 0, int z = 0)
        {
            listBox_routines.Items.Clear();
            _routines.Clear();

            var routine = new RenderRoutine(vaddr);
            listBox_routines.Items.Add(routine.ToString());
            _routines.Add(routine);

            DecodeDlists();
            NewRender();
        }

        public void AddDList(uint vaddr, int x = 0, int y = 0, int z = 0)
        {
            var routine = new RenderRoutine(vaddr, x, y, z);
            listBox_routines.Items.Add(routine.ToString());
            _routines.Add(routine);

            DecodeDlists();
            NewRender();
        }








        private void toolStripTextBoxEntrypoint_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                modelViewer.Focus();
            }
        }

        private void toolStripSegmentsBtn_Click(object sender, EventArgs e)
        {
            if (_segForm != null)
            {
                _segForm.Activate();
            }
            else
            {
                _segForm = new SegmentEditorForm(_game, _renderer);
                _segForm.SegmentsChanged += SegForm_SegmentsChanged;
                _segForm.FormClosed += (sender, e) => _segForm = null;
                _segForm.Show();
            }
        }

        private void SegForm_SegmentsChanged(object sender, F3DZEX.Memory.Segment e)
        {
            _renderer.Memory.Segments[(int)sender] = e;

            DecodeDlists();
            NewRender();
        }

        private void toolStripDisasBtn_Click(object sender, EventArgs e)
        {
            if (_disasForm != null)
            {
                _disasForm.Activate();
            }
            else
            {
                _disasForm = new DisasmForm(defaultText: "No Dlist selected");

                _disasForm.FormClosed += (sender, e) => _disasForm = null;
                _disasForm.Show();
            }

            if (listBox_routines.SelectedIndex != -1)
            {
                var dlist = _routines[listBox_routines.SelectedIndex].Dlist;
                if (dlist == null)
                    _disasForm.SetMessage("Error");
                else
                    _disasForm.UpdateDlist(dlist);
            }
        }


        private void DListViewerForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Instance = null;
            _disasForm?.Close();
            _segForm?.Close();
            _settingsForm?.Close();
        }

        private void toolStripRenderCfgBtn_Click(object sender, EventArgs e)
        {
            if (_settingsForm != null)
            {
                _settingsForm.Activate();
            }
            else
            {
                _settingsForm = new SettingsForm(_rendererCfg);
                _settingsForm.FormClosed += (sender, e) => _settingsForm = null;
                _settingsForm.SettingsChanged += (sender, e) => { modelViewer.Render(); };
                _settingsForm.Show();
            }
        }

        private void saveScreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = "";
            saveFileDialog1.Filter = Filters.PNG;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var bmp = modelViewer.CaptureScreen();
                bmp.Save(saveFileDialog1.FileName);
            }
        }

        private void copyToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var bmp = modelViewer.CaptureScreen();
            Clipboard.SetImage(bmp);
        }

        private void modelViewer_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                renderContextMenuStrip.Show(modelViewer.PointToScreen(e.Location));
            }
        }

        private void listBox_routines_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = listBox_routines.SelectedIndex;
            if (idx >= 0 && idx < _routines.Count)
            {
                RemoveRoutineMenuItem.Visible = true;
                EditRoutineMenuItem.Visible = true;

                var dlist = _routines[idx].Dlist;

                if (dlist == null)
                    _disasForm?.SetMessage("Error");
                else
                    _disasForm?.UpdateDlist(dlist);
            }
            else
            {
                RemoveRoutineMenuItem.Visible = false;
                EditRoutineMenuItem.Visible = false;
            }
        }

        private string IsInputValid(string input)
        {
            string err = "Invalid format, must be \"<address in hex>(; <x>; <y>; <z>)\"";

            var parts = input.Replace(" ", "").Split(";");
            if (parts.Length != 1 && parts.Length != 4)
                return err;

            string addrStr = parts[0];
            if (addrStr.StartsWith("0x"))
                addrStr = addrStr.Substring(2);

            if (!SegmentedAddress.TryParse(addrStr, true, out SegmentedAddress addr))
                return err;

            for (int i = 1; i < parts.Length; i++)
            {
                if (!int.TryParse(parts[i], out int res))
                    return err;
            }

            return null;
        }

        private void AddRoutineMenuItem_Click(object sender, System.EventArgs e)
        {
            EditValueForm form = new EditValueForm("Add Dlist", "Enter the address and coordinates of the dlist to add.", IsInputValid);
            if (form.ShowDialog() == DialogResult.OK)
            {
                var parts = form.Result.Replace(" ", "").Split(";");
                int x = 0, y = 0, z = 0;
                var addr = SegmentedAddress.Parse(parts[0], true);
                if (parts.Length > 1)
                {
                    x = int.Parse(parts[1]);
                    y = int.Parse(parts[2]);
                    z = int.Parse(parts[3]);
                }

                AddDList(addr.VAddr, x, y, z);
            }
        }

        private void EditRoutineMenuItem_Click(object sender, EventArgs e)
        {
            int idx = listBox_routines.SelectedIndex;
            if (idx >= 0 && idx < listBox_routines.Items.Count)
            {
                var routine = _routines[idx];

                EditValueForm form = new EditValueForm("Edit Dlist",
                    "Enter the address and coordinates of the dlist to add.",
                    IsInputValid, $"{routine.Address:X8}; {routine.X}; {routine.Y}; {routine.Z}");

                if (form.ShowDialog() == DialogResult.OK)
                {
                    var parts = form.Result.Replace(" ", "").Split(";");

                    routine.X =
                    routine.Y =
                    routine.Z = 0;
                    routine.Address = SegmentedAddress.Parse(parts[0], true);

                    if (parts.Length > 1)
                    {
                        routine.X = int.Parse(parts[1]);
                        routine.Y = int.Parse(parts[2]);
                        routine.Z = int.Parse(parts[3]);
                    }
                    NewRender();
                }
            }

        }


        private void RemoveRoutineMenuItem_Click(object sender, System.EventArgs e)
        {
            int idx = listBox_routines.SelectedIndex;
            if (idx >= 0 && idx < _routines.Count)
            {
                listBox_routines.Items.RemoveAt(idx);
                _routines.RemoveAt(idx);

                DecodeDlists();
                NewRender();
            }
        }

    }
}
