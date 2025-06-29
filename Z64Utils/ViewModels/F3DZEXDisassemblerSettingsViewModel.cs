using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using F3DZEX.Command;

namespace Z64Utils.ViewModels;

public partial class F3DZEXDisassemblerSettingsViewModel : ObservableObject
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    [ObservableProperty]
    private bool _showAddress;

    [ObservableProperty]
    private bool _relativeAddress;

    [ObservableProperty]
    private bool _disasMultiCmdMacro;

    [ObservableProperty]
    private bool _addressLiteral;

    [ObservableProperty]
    private bool _static;

    public F3DZEX.Disassembler.Config DisasConfig
    {
        get =>
            new()
            {
                ShowAddress = ShowAddress,
                RelativeAddress = RelativeAddress,
                DisasMultiCmdMacro = DisasMultiCmdMacro,
                AddressLiteral = AddressLiteral,
                Static = Static,
            };
        set
        {
            ShowAddress = value.ShowAddress;
            RelativeAddress = value.RelativeAddress;
            DisasMultiCmdMacro = value.DisasMultiCmdMacro;
            AddressLiteral = value.AddressLiteral;
            Static = value.Static;
        }
    }
    public event EventHandler? DisasConfigChanged;

    [ObservableProperty]
    private string _outputDisasPreview = "";

    public F3DZEXDisassemblerSettingsViewModel()
    {
        PropertyChanged += (sender, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(ShowAddress):
                case nameof(RelativeAddress):
                case nameof(DisasMultiCmdMacro):
                case nameof(AddressLiteral):
                case nameof(Static):
                    UpdateDisassemblyPreview();
                    DisasConfigChanged?.Invoke(this, new());
                    break;
            }
        };
        UpdateDisassemblyPreview();
    }

    public void UpdateDisassemblyPreview()
    {
        var dlistBytes = new byte[] { 0x01, 0x01, 0x20, 0x24, 0x06, 0x00, 0x0F, 0xC8 };
        var dlist = new Dlist(dlistBytes, 0x060002C8);
        F3DZEX.Disassembler disas = new F3DZEX.Disassembler(dlist, DisasConfig);
        var lines = disas.Disassemble();
        StringWriter sw = new StringWriter();
        foreach (var line in lines)
            sw.Write($"{line}\n");

        OutputDisasPreview = sw.ToString();
    }
}
