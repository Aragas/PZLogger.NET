using System;
using System.IO;
using System.Reflection;
using System.Text;

using PCLExt.FileStorage;

namespace PZLogger.NET
{
    public static partial class Program
    {
        private static void CatchException(object exceptionObject)
        {
            var exception = exceptionObject as System.Exception ?? new NotSupportedException("Unhandled exception doesn't derive from System.Exception: " + exceptionObject);

            var exceptionText = CatchError(exception);
            ReportErrorLocal(exceptionText);
            Stop();
        }
        private static void CatchException(System.Exception exception)
        {
            var exceptionText = CatchError(exception);
            ReportErrorLocal(exceptionText);
            Stop();
        }

        private static string CatchError(System.Exception ex)
        {
            var errorLog =
$@"[CODE]
PZLogger.NET Crash Log v {Assembly.GetExecutingAssembly().GetName().Version}
{BuildErrorStringRecursive(ex)}
You should report this error if it is reproduceable or you could not solve it by yourself.
[/CODE]";

            return errorLog;
        }
        private static string BuildErrorStringRecursive(System.Exception ex)
        {
            var sb = new StringBuilder();
            sb.AppendFormat(
$@"Error information:
Type: {ex.GetType().FullName}
Message: {ex.Message}
HelpLink: {(string.IsNullOrWhiteSpace(ex.HelpLink) ? "Empty" : ex.HelpLink)}
Source: {ex.Source}
TargetSite : {ex.TargetSite}
CallStack:
{ex.StackTrace}");

            if (ex.InnerException != null)
            {
                sb.AppendFormat($@"
--------------------------------------------------
InnerException:
{BuildErrorStringRecursive(ex.InnerException)}");
            }

            return sb.ToString();
        }


        private static void ReportErrorLocal(string exception)
        {
            var crashFile = Storage.CrashLogFolder.CreateFileAsync($"{DateTime.Now:yyyy-MM-dd_HH.mm.ss}.log", CreationCollisionOption.OpenIfExists).Result;
            using (var stream = crashFile.OpenAsync(PCLExt.FileStorage.FileAccess.ReadAndWrite).Result)
            using (var writer = new StreamWriter(stream))
                writer.Write(exception);
        }
    }
}