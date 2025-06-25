using System;
using Avalonia.Media;
using Common;
using CommunityToolkit.Mvvm.ComponentModel;
using N64;
using Z64;

namespace Z64Utils_Avalonia;

public partial class TextureViewerWindowViewModel : ObservableObject
{
    private Z64Game _game;

    [ObservableProperty]
    AddressType _textureAddressType = AddressType.VROM;

    [ObservableProperty]
    uint? _textureAddress = null;

    [ObservableProperty]
    uint? _TLUTAddress = null;

    [ObservableProperty]
    N64TexFormat _format;

    [ObservableProperty]
    int _width = 16;

    [ObservableProperty]
    int _height = 16;

    [ObservableProperty]
    private IImage? _image;

    public TextureViewerWindowViewModel(Z64Game game)
    {
        _game = game;
        PropertyChanged += (sender, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(TextureAddress):
                    if (TextureAddress > int.MaxValue)
                        TextureAddress = int.MaxValue;
                    UpdateImage();
                    break;
                case nameof(TextureAddressType):
                case nameof(TLUTAddress):
                case nameof(Format):
                case nameof(Width):
                case nameof(Height):
                    UpdateImage();
                    break;
            }
        };
    }

    public void UpdateImage()
    {
        if (!UpdateImageImpl())
            Image = null;
    }

    public bool UpdateImageImpl()
    {
        if (TextureAddress == null)
            return false;
        int texSize = N64Texture.GetTexSize(Width * Height, N64Texture.ConvertFormat(Format).Item2);
        byte[]? texData = ReadBytes((uint)TextureAddress, texSize);
        if (texData == null)
            return false;
        byte[]? tlutData = null;
        if (Format == N64TexFormat.CI4 || Format == N64TexFormat.CI8)
        {
            if (TLUTAddress == null)
                return false;
            tlutData = ReadBytes((uint)TLUTAddress, 2 * (Format == N64TexFormat.CI4 ? 16 : 256));
            if (tlutData == null)
                return false;
        }
        Image = N64Texture
            .DecodeBitmap(Width, Height, Format, texData, tlutData)
            .ToAvaloniaBitmap();
        return true;
    }

    // Z64.Forms.TextureViewer.ReadBytes
    private byte[]? ReadBytes(uint addr, int size)
    {
        switch (TextureAddressType)
        {
            case AddressType.VRAM:
                try
                {
                    return _game.Memory.ReadBytes(addr, size);
                }
                catch (Z64MemoryException)
                {
                    return null;
                }
            case AddressType.ROM:
                return _game.Rom.RawRom[(int)addr..((int)addr + size)];
            case AddressType.VROM:
                for (int i = 0; i < _game.GetFileCount(); i++)
                {
                    var file = _game.GetFileFromIndex(i);
                    if (addr >= file.VRomStart && addr + size <= file.VRomEnd)
                    {
                        byte[] buffer = new byte[size];
                        Utils.Assert(file.Valid());
                        Buffer.BlockCopy(
                            file.Data,
                            (int)(addr - (uint)file.VRomStart),
                            buffer,
                            0,
                            size
                        );
                        return buffer;
                    }
                }
                break;
        }
        return null;
    }
}
