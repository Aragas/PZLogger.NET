using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using MySql.Data.MySqlClient;

using PZLogger.NET.Exception;
using PZLogger.NET.LogFiles;

namespace PZLogger.NET
{
    public abstract class LogFile
    {
        public static LogFile Create(string path, MySqlConnection connection)
        {
            var name = System.IO.Path.GetFileNameWithoutExtension(path)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(name))
            {

            }
            else if (name.Contains("user"))
            {
                return new UserLogFile(path, connection);
            }
            else if (name.Contains("admin"))
            {
                return new AdminLogFile(path, connection);
            }
            else if (name.Contains("chat"))
            {
                return new ChatLogFile(path, connection);
            }
            else if (name.Contains("item"))
            {
                return new ItemLogFile(path, connection);
            }
            else if (name.Contains("map"))
            {
                return new MapLogFile(path, connection);
            }
            else if (name.Contains("cmd"))
            {
                return new CmdLogFile(path, connection);
            }

            throw new ReportToConsoleException("Wrong file, be sure it contains 'user', 'admin', 'chat', 'item', 'map' or 'cmd'");
        }
        private static IEnumerable<string> ReadFileLines(string path)
        {
            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 0x1000, FileOptions.SequentialScan))
            using (var stream = new StreamReader(fileStream, Encoding.UTF8))
            {
                string line;
                while ((line = stream.ReadLine()) != null)
                    yield return line;
            }
        }


        public string Path { get; }
        protected string Path_DB => Path.Replace(@"\", @"\\");

        protected MySqlConnection Connection { get; }

        protected abstract string TableName { get; }
       
        private int LastLine { get; set; }
        private IEnumerable<string> Text => ReadFileLines(Path).Skip(LastLine);

        private FileSystemWatcher FileSystemWatcher { get; }

        protected LogFile(string path, MySqlConnection connection)
        {
            Path = path;
            Connection = connection;

            using (var command = CreateCommand())
                command.ExecuteNonQuery();

            using (var command = CountCommand())
                LastLine = int.Parse(command.ExecuteScalar().ToString(), CultureInfo.InvariantCulture);

            FileSystemWatcher = new FileSystemWatcher
            {
                NotifyFilter = NotifyFilters.LastWrite,
                Path = System.IO.Path.GetDirectoryName(Path),
                Filter = System.IO.Path.GetFileName(Path),
                EnableRaisingEvents = true
            };
            FileSystemWatcher.Changed += OnChanged;

        }
        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            // FileSystemWatcher has some bug that fires event twice, this is a fix
            try
            {
                FileSystemWatcher.EnableRaisingEvents = false;

                foreach (var line in Text)
                    ProcessLine(line);
            }

            finally { FileSystemWatcher.EnableRaisingEvents = true; }
        }

        protected abstract MySqlCommand CreateCommand();
        protected abstract MySqlCommand InsertCommand(dynamic data);
        protected virtual MySqlCommand CountCommand()
        {
            return new MySqlCommand($@"
SELECT COUNT(*)
FROM {TableName}
WHERE {TableName}.Path = '{Path_DB}';", Connection);
        }
        protected virtual void ProcessLine(string line) { LastLine++; }
    }
}