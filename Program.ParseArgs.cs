using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

using ConsoleManager;

using NDesk.Options;

using PCLExt.Config;

namespace PZLogger.NET
{
    public static partial class Program
    {
        private const int SW_HIDE = 0;
        private const int SW_SHOWMINIMIZED = 2;
        private const int SW_RESTORE = 9;

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);


        private static ConfigType ConfigType { get; set; } = ConfigType.YamlConfig;

        private static void ParseArgs(IEnumerable<string> args)
        {
            var options = new OptionSet();
            try
            {
                options = new OptionSet()
                    .Add("c|console", "enables the console.", StartFastConsole)
                    .Add("fps=", "{FPS} of the console, integer.", fps => FastConsole.ScreenFPS = int.Parse(fps))
                    .Add("s|silent", "enables silent mode.", SilentMode)
                    .Add("m|minimize", "enables minimize mode.", MinimizeMode)
                    .Add("config=", "used {CONFIG_TYPE}. Use YAML or JSON", ParseConfig)
                    .Add("h|help", "show help.", str => ShowHelp(options));

                options.Parse(args);
            }
            catch (System.Exception ex) when (ex is OptionException || ex is FormatException)
            {
                FastConsole.Stop();

                Console.Write("PokeD.Server.Desktop: ");
                Console.WriteLine(ex.Message);
                Console.WriteLine("Try `PokeD.Server.Desktop --help' for more information.");

                ShowHelp(options, true);

                Console.WriteLine();
                Console.WriteLine("Press any key to continue$(SolutionDir).");
                Console.ReadKey();
                Environment.Exit((int) ExitCodes.Success);
            }
        }
        private static void StartFastConsole(string s)
        {
            FastConsole.ConstantAddLine(
                "Main              thread execution time: {0} ms", () => new object[] { MainThreadTime });
            FastConsole.ConstantAddLine(
                "ConsoleManager    thread execution time: {0} ms", () => new object[] { FastConsole.ConsoleManagerThreadTime });

            FastConsole.Start();
        }
        private static void SilentMode(string s)
        {
            try
            {
                var winHandle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
                ShowWindow(winHandle, SW_HIDE);
            }
            catch (DllNotFoundException)
            {
                if (FastConsole.Enabled)
                    FastConsole.WriteLine("user32.dll not found, silent and minimize not available");
                else
                    Console.WriteLine("Usage: PZLogger.NET [OPTIONS]");
            }
        }
        private static void MinimizeMode(string s)
        {
            try
            {
                var winHandle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
                ShowWindow(winHandle, SW_SHOWMINIMIZED);
            }
            catch (DllNotFoundException)
            {
                if (FastConsole.Enabled)
                    FastConsole.WriteLine("user32.dll not found, silent and minimize not available");
                else
                    Console.WriteLine("Usage: PZLogger.NET [OPTIONS]");
            }
        }
        private static void ShowHelp(OptionSet options, bool direct = false)
        {
            if (direct)
            {
                Console.WriteLine("Usage: PZLogger.NET [OPTIONS]");
                Console.WriteLine();
                Console.WriteLine("Options:");

                options.WriteOptionDescriptions(Console.Out);
            }
            else
            {
                FastConsole.WriteLine("Usage: PZLogger.NET [OPTIONS]");
                FastConsole.WriteLine();
                FastConsole.WriteLine("Options:");

                var opt = new StringWriter();
                options.WriteOptionDescriptions(opt);
                foreach (var line in opt.GetStringBuilder().ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                    FastConsole.WriteLine(line);
            }
        }
        private static void ParseConfig(string config)
        {
            switch (config.ToLowerInvariant())
            {
                case "json":
                    ConfigType = ConfigType.JsonConfig;
                    break;

                case "yml":
                case "yaml":
                    ConfigType = ConfigType.YamlConfig;
                    break;

                default:
                    throw new FormatException("CONFIG_WRAPPER not correct.");
            }
        }
    }
}