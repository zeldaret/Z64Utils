using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Z64Utils.ViewModels;
using Z64Utils_Avalonia;

namespace Z64;

public partial class App : Application
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var winVM = new MainWindowViewModel();
            var win = new MainWindow() { DataContext = winVM };
            desktop.MainWindow = win;

            win.Opened += (sender, ev) =>
            {
                string? romPath = Program.ParsedArgs?.RomFile?.FullName;
                Logger.Debug("romPath={romPath}", romPath);
                if (romPath != null)
                {
                    winVM.OpenROM(romPath);
                    var objectAnalyzerFileNames = Program.ParsedArgs?.ObjectAnalyzerFileNames;
                    if (objectAnalyzerFileNames != null)
                    {
                        Logger.Debug(
                            "objectAnalyzerFileNames={objectAnalyzerFileNames}",
                            objectAnalyzerFileNames
                        );
                        foreach (var name in objectAnalyzerFileNames)
                        {
                            // TODO un-hardcode segment 6
                            var oavm = winVM.OpenObjectAnalyzerByFileName(name, 6);

                            if (oavm == null)
                            {
                                // TODO maybe show error window
                                Logger.Error(
                                    "Could not find an object with name {objectName}",
                                    name
                                );
                                continue;
                            }

                            // TODO put find and analyze behind more command line args
                            // TODO don't use the Command funcs themselves?
                            oavm.FindDListsCommand();
                            oavm.AnalyzeDListsCommand();

                            var dListViewerOHEName = Program.ParsedArgs?.DListViewerOHEName;
                            if (dListViewerOHEName != null)
                            {
                                ObjectAnalyzerWindowViewModel.ObjectHolderEntry? ohe;
                                try
                                {
                                    ohe = oavm.ObjectHolderEntries.First(ohe =>
                                        ohe.ObjectHolder.Name == dListViewerOHEName
                                    );
                                }
                                catch (InvalidOperationException)
                                {
                                    ohe = null;
                                }
                                if (ohe != null)
                                {
                                    oavm.OpenDListViewerObjectHolderEntryCommand.Execute(ohe);
                                }
                                else
                                {
                                    // TODO maybe show error window
                                    Logger.Error(
                                        "Could not find an entry with name {0}",
                                        dListViewerOHEName
                                    );
                                }
                            }

                            var skeletonViewerOHEName = Program.ParsedArgs?.SkeletonViewerOHEName;
                            if (skeletonViewerOHEName != null)
                            {
                                ObjectAnalyzerWindowViewModel.ObjectHolderEntry? ohe;
                                try
                                {
                                    ohe = oavm.ObjectHolderEntries.First(ohe =>
                                        ohe.ObjectHolder.Name == skeletonViewerOHEName
                                    );
                                }
                                catch (InvalidOperationException)
                                {
                                    ohe = null;
                                }
                                if (ohe != null)
                                {
                                    oavm.OpenSkeletonViewerObjectHolderEntryCommand.Execute(ohe);
                                }
                                else
                                {
                                    // TODO maybe show error window
                                    Logger.Error(
                                        "Could not find an entry with name {0}",
                                        skeletonViewerOHEName
                                    );
                                }
                            }
                        }
                    }
                }
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
