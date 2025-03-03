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
    public partial class DmaFileSelectForm : MicrosoftFontForm
    {
        public Z64File SelectedFile { get; private set; }

        Z64Game _game;
        string[] _fileItemsText;
        string _lastSearch = null;

        public DmaFileSelectForm(Z64Game game)
        {
            InitializeComponent();
            _game = game;

            _fileItemsText = new string[_game.GetFileCount()];
            for (int i = 0; i < _game.GetFileCount(); i++)
            {
                var file = _game.GetFileFromIndex(i);
                if (!file.Valid())
                    continue;

                _fileItemsText[i] = (
                    $"{_game.GetFileName(file.VRomStart).ToLower()} {file.VRomStart:x8} {file.VRomEnd:x8}"
                );
            }
            UpdateFileList();
        }

        private void searchBox_TextChanged(object sender, EventArgs e)
        {
            UpdateFileList();
        }

        private void UpdateFileList()
        {
            fileListView.BeginUpdate();
            string search = searchBox.Text.ToLower();
            if (_lastSearch != null && search.Contains(_lastSearch))
            {
                for (int i = 0; i < fileListView.Items.Count; i++)
                    if (!_fileItemsText[(int)fileListView.Items[i].Tag].Contains(search))
                        fileListView.Items.RemoveAt(i--);
            }
            else
            {
                fileListView.Items.Clear();
                for (int i = 0; i < _game.GetFileCount(); i++)
                {
                    var file = _game.GetFileFromIndex(i);
                    if (!file.Valid())
                        continue;

                    if (_fileItemsText[i].Contains(search))
                    {
                        string name = _game.GetFileName(file.VRomStart);
                        string vrom = $"{file.VRomStart:X8}-{file.VRomEnd:X8}";
                        string rom = $"{file.RomStart:X8}-{file.RomEnd:X8}";
                        string type = "Unknow";

                        var item = fileListView.Items.Add(name);
                        item.SubItems.AddRange(new string[] { vrom, rom, type });
                        item.Tag = i;
                    }
                }
            }
            _lastSearch = search;
            fileListView.EndUpdate();
        }

        private void listView_files_DoubleClick(object sender, EventArgs e)
        {
            if (fileListView.SelectedItems.Count == 1)
            {
                SelectedFile = _game.GetFileFromIndex((int)fileListView.SelectedItems[0].Tag);
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
