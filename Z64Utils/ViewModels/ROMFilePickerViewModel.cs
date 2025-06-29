using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Z64;

namespace Z64Utils.ViewModels;

public partial class ROMFilePickerViewModel : ObservableObject
{
    public partial class ROMFile : ObservableObject
    {
        public Z64File File;

        [ObservableProperty]
        private string _name;

        public ROMFile(Z64File file, string name)
        {
            File = file;
            Name = name;
        }
    }

    private IEnumerable<ROMFile> _allROMFiles;

    [ObservableProperty]
    private IEnumerable<ROMFile> _ROMFiles;

    [ObservableProperty]
    private ROMFile? _selectedROMFile;

    [ObservableProperty]
    private string _filterText = "";

    public ROMFilePickerViewModel(IEnumerable<ROMFile> files)
    {
        _allROMFiles = files;
        ROMFiles = files;
        PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(FilterText))
            {
                var filterTextLower = FilterText.ToLower();
                ROMFiles = _allROMFiles.Where(rf => rf.Name.ToLower().Contains(filterTextLower));
            }
        };
    }
}
