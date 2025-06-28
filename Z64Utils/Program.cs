using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Reflection;
using Avalonia;
using Common;

namespace Z64Utils
{
    static class Program
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public static readonly string Version =
            Assembly
                .GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion ?? "UnknownVersion";

        public const int EXIT_SUCCESS = 0;

        public class ParsedArgsData
        {
            public FileInfo? RomFile;
            public string[]? ObjectAnalyzerFileNames;
            public string? DListViewerOHEName;
            public string? SkeletonViewerOHEName;
        }

        public static ParsedArgsData? ParsedArgs = null;

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static int Main(string[] args)
        {
            try
            {
                Logger.Info("Starting up Z64Utils Version {Version}...", Version);

                int code = HandleArgs(args);
                if (code != EXIT_SUCCESS)
                    return code;

                BuildAvaloniaApp()
                    .With(
                        new Win32PlatformOptions()
                        {
                            RenderingMode = new List<Win32RenderingMode>()
                            {
                                // Replace using ANGLE with native OpenGL
                                // However this is "not recommended"
                                // https://github.com/AvaloniaUI/Avalonia/discussions/6396
                                // https://github.com/AvaloniaUI/Avalonia/discussions/9393
                                // A better option would be to keep Avalonia rendering with whatever it wants,
                                // and use OpenGL to render to a surface that we would then pass back to Avalonia
                                // https://github.com/AvaloniaUI/Avalonia/discussions/5432
                                // ? https://github.com/AvaloniaUI/Avalonia/discussions/6842
                                // ? https://github.com/AvaloniaUI/Avalonia/pull/9639
                                // https://github.com/AvaloniaUI/Avalonia/discussions/16188
                                Win32RenderingMode.Wgl,
                                Win32RenderingMode.Software,
                            },
                        }
                    )
                    .StartWithClassicDesktopLifetime(args);
                return EXIT_SUCCESS;
            }
            catch (Exception e)
            {
                Logger.Fatal(e);
                throw;
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp() =>
            AppBuilder.Configure<App>().UsePlatformDetect().WithInterFont().LogSinkToNLog();

        public static int HandleArgs(string[] args)
        {
            // To enable tab-completion in your shell see:
            // https://learn.microsoft.com/en-us/dotnet/standard/commandline/tab-completion

            var romFileOption = new Option<FileInfo?>(
                name: "--rom",
                description: "The ROM to load.",
                parseArgument: result =>
                {
                    string filePath = result.Tokens.Single().Value;
                    var fi = new FileInfo(filePath);
                    if (fi.Exists)
                    {
                        return fi;
                    }
                    else
                    {
                        result.ErrorMessage = $"ROM \"{fi.FullName}\" does not exist";
                        return null;
                    }
                }
            );
            var objectAnalyzerOption = new Option<string[]>(
                name: "--object-analyzer",
                description: "Open files in the ROM, by name, in the object analyzer."
            );
            var dListViewerOption = new Option<string>(
                name: "--dlist-viewer",
                // TODO should really be by offset but it's impractical with the exposed Z64Object info
                description: "Open a DList in the object, by name, in the dlist viewer."
            );
            var skeletonViewerOption = new Option<string>(
                name: "--skeleton-viewer",
                // TODO should really be by offset but it's impractical with the exposed Z64Object info
                description: "Open a skeleton in the object, by name, in the skeleton viewer."
            );

            var rootCommand = new RootCommand("Z64Utils");
            rootCommand.AddOption(romFileOption);
            rootCommand.AddOption(objectAnalyzerOption);
            rootCommand.AddOption(dListViewerOption);
            rootCommand.AddOption(skeletonViewerOption);

            ParsedArgs = new();
            rootCommand.SetHandler(
                (file, names, dlName, skeletonName) =>
                {
                    ParsedArgs.RomFile = file;
                    ParsedArgs.ObjectAnalyzerFileNames = names;
                    ParsedArgs.DListViewerOHEName = dlName;
                    ParsedArgs.SkeletonViewerOHEName = skeletonName;
                },
                romFileOption,
                objectAnalyzerOption,
                dListViewerOption,
                skeletonViewerOption
            );

            int code = rootCommand.Invoke(args);
            return code;
        }
    }

    public class AvaloniaLogSinkToNLog : Avalonia.Logging.ILogSink
    {
        private static readonly Dictionary<
            Avalonia.Logging.LogEventLevel,
            NLog.LogLevel
        > LOG_LEVELS_MAP = new()
        {
            { Avalonia.Logging.LogEventLevel.Verbose, NLog.LogLevel.Trace },
            { Avalonia.Logging.LogEventLevel.Debug, NLog.LogLevel.Debug },
            { Avalonia.Logging.LogEventLevel.Information, NLog.LogLevel.Info },
            { Avalonia.Logging.LogEventLevel.Warning, NLog.LogLevel.Warn },
            { Avalonia.Logging.LogEventLevel.Error, NLog.LogLevel.Error },
            { Avalonia.Logging.LogEventLevel.Fatal, NLog.LogLevel.Fatal },
        };

        static AvaloniaLogSinkToNLog()
        {
            // Assert LOG_LEVELS_MAP contains all of Avalonia's log levels
            foreach (var e in Enum.GetValues<Avalonia.Logging.LogEventLevel>())
            {
                Utils.Assert(LOG_LEVELS_MAP.ContainsKey(e));
            }
        }

        public NLog.Logger GetAreaLogger(string area)
        {
            return NLog.LogManager.GetLogger("Avalonia-" + area);
        }

        public bool IsEnabled(Avalonia.Logging.LogEventLevel level, string area)
        {
            return GetAreaLogger(area).IsEnabled(LOG_LEVELS_MAP[level]);
        }

        private void LogImpl(
            Avalonia.Logging.LogEventLevel level,
            string area,
            Action<NLog.LogEventBuilder> setMessage
        )
        {
            NLog.Logger logger = GetAreaLogger(area);
            NLog.LogEventBuilder logEventBuilder = new(logger, LOG_LEVELS_MAP[level]);
            setMessage(logEventBuilder);
            logger.Log(typeof(AvaloniaLogSinkToNLog), logEventBuilder.LogEvent);
        }

        public void Log(
            Avalonia.Logging.LogEventLevel level,
            string area,
            object? source,
            string messageTemplate
        )
        {
            LogImpl(level, area, eb => eb.Message(messageTemplate));
        }

        public void Log(
            Avalonia.Logging.LogEventLevel level,
            string area,
            object? source,
            string messageTemplate,
            params object?[] propertyValues
        )
        {
            LogImpl(level, area, eb => eb.Message(messageTemplate, propertyValues));
        }
    }

    public static class MyLogExtensions
    {
        public static AppBuilder LogSinkToNLog(this AppBuilder builder)
        {
            Avalonia.Logging.Logger.Sink = new AvaloniaLogSinkToNLog();
            return builder;
        }
    }
}
