using System;

using ConsoleManager;

namespace PZLogger.NET
{
    public static partial class Program
    {
        private static bool ExecuteCommand(string message)
        {
            var command = message.Remove(0, 1).ToLower();
            message = message.Remove(0, 1);

            if (message.StartsWith("stop"))
            {
                FastConsole.Stop();

                Logger?.Stop();

                Console.WriteLine("Stopped the server. Press any key to continue...");
                Console.ReadKey();
                Environment.Exit((int) ExitCodes.Success);
            }

            else if (message.StartsWith("clear"))
                FastConsole.ClearOutput();

            else if (command.StartsWith("help logger"))
                return Logger.ExecuteCommand(message.Remove(0, 11));

            else if (command.StartsWith("help"))
                return ExecuteHelpCommand(message.Remove(0, 4));

            else
                return Logger.ExecuteCommand(message);

            return true;
        }

        private static bool ExecuteHelpCommand(string command) { return true; }
    }
}