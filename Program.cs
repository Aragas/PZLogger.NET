using System;
using System.Diagnostics;
using System.Threading;

using ConsoleManager;

namespace PZLogger.NET
{
    public static partial class Program
    {
        private static PZLogger Logger { get; set; }


        public static void Main(string[] args)
        {
#if !DEBUG
            try { AppDomain.CurrentDomain.UnhandledException += (sender, e) => CatchException(e.ExceptionObject); }
            catch (System.Exception exception) { CatchException(exception); }
#endif

            ParseArgs(args);

            Start();
        }

        private static void Start()
        {
            Logger = new PZLogger(ConfigType);
            Logger.Start();

            Update();
        }
        private static void Stop()
        {
            FastConsole.Stop();

            Logger?.Stop();

            Environment.Exit((int) ExitCodes.UnknownError);
        }

        public static long MainThreadTime { get; private set; }
        private static void Update()
        {
            var watch = Stopwatch.StartNew();
            while (true)
            {
                if (FastConsole.InputAvailable)
                {
                    var input = FastConsole.ReadLine();

                    if (input.StartsWith("/") && !ExecuteCommand(input))
                        Console.WriteLine("Invalid command!");
                }

                if (Logger == null || (Logger != null && Logger.IsDisposing))
                    break;

                

                if (watch.ElapsedMilliseconds < 100)
                {
                    MainThreadTime = watch.ElapsedMilliseconds;

                    var time = (int) (100 - watch.ElapsedMilliseconds);
                    if (time < 0) time = 0;
                    Thread.Sleep(time);
                }

                watch.Reset();
                watch.Start();
            }
        }
    }
}