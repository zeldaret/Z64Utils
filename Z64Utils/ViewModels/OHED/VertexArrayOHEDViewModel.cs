using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Z64Utils.ViewModels.OHED;

public partial class VertexArrayOHEDViewModel : ObservableObject, IObjectHolderEntryDetailsViewModel
{
    public class VertexEntry
    {
        public int Index { get; init; }
        public uint Address { get; init; }
        public int CoordX { get; init; }
        public int CoordY { get; init; }
        public int CoordZ { get; init; }
        public int TexCoordS { get; init; }
        public int TexCoordT { get; init; }
        public byte ColorRorNormalX { get; init; }
        public byte ColorGorNormalY { get; init; }
        public byte ColorBorNormalZ { get; init; }
        public byte ColorR { get; init; }
        public byte ColorG { get; init; }
        public byte ColorB { get; init; }
        public float NormalX { get; init; }
        public float NormalY { get; init; }
        public float NormalZ { get; init; }
        public byte Alpha { get; init; }

        public VertexEntry(
            int index,
            uint address,
            int coordX,
            int coordY,
            int coordZ,
            int texCoordS,
            int texCoordT,
            byte colorRorNormalX,
            byte colorGorNormalY,
            byte colorBorNormalZ,
            byte alpha
        )
        {
            Index = index;
            Address = address;
            CoordX = coordX;
            CoordY = coordY;
            CoordZ = coordZ;
            TexCoordS = texCoordS;
            TexCoordT = texCoordT;
            ColorRorNormalX = colorRorNormalX;
            ColorGorNormalY = colorGorNormalY;
            ColorBorNormalZ = colorBorNormalZ;
            // TODO this logic probably should be in Model, not ViewModel...
            ColorR = colorRorNormalX;
            ColorG = colorGorNormalY;
            ColorB = colorBorNormalZ;
            float NormalByteToFloat(byte n)
            {
                return (float)(n >= 0x80 ? n - 0x100 : n) / 0x80;
            }
            NormalX = NormalByteToFloat(colorRorNormalX);
            NormalY = NormalByteToFloat(colorGorNormalY);
            NormalZ = NormalByteToFloat(colorBorNormalZ);
            Alpha = alpha;
        }
    }

    [ObservableProperty]
    private ObservableCollection<VertexEntry> _vertices = new();

    [ObservableProperty]
    public bool _showVertexColorNormalAsUnk = true;

    [ObservableProperty]
    public bool _showVertexColorNormalAsColor = false;

    [ObservableProperty]
    public bool _showVertexColorNormalAsNormal = false;

    [ObservableProperty]
    public bool? _vertexColorOrNormalIsChecked = null;

    public VertexArrayOHEDViewModel()
    {
        PropertyChanged += (sender, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(VertexColorOrNormalIsChecked):
                    bool? c = VertexColorOrNormalIsChecked;
                    ShowVertexColorNormalAsUnk = c == null;
                    ShowVertexColorNormalAsColor = c == true;
                    ShowVertexColorNormalAsNormal = c == false;
                    break;
            }
        };
    }
}
