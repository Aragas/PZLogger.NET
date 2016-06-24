using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MySql.Data.MySqlClient;

using PCLExt.Config;
using PZLogger.NET.Exception;

namespace PZLogger.NET
{
    public class PZLogger : ConfigProperties
    {
        protected override string FileName { get; } = "Settings";
        protected override ConfigType ConfigType { get; }

        #region ConfigProperties

        public string Host { get; private set; } = "localhost";

        public ushort Port { get; private set; } = 3306;

        public string Database { get; private set; } = "";

        public string Username { get; private set; } = "";

        public string Password { get; private set; } = "";

        public string[] LoggedDirPaths { get; private set; } = { "PathToDirectory", "PathToDirectory" };

        public string[] LoggedFilePaths { get; private set; } = { "PathToFile", "PathToFile" };

        #endregion ConfigProperties

        [ConfigIgnore]
        public bool IsDisposing { get; set; }

        private List<LogFile> LoggedFiles { get; } = new List<LogFile>();

        private MySqlConnection Connection { get; set; }

        public PZLogger(ConfigType configType) { ConfigType = configType; }

        public void Start()
        {
            LoadConfig();

            CheckDatabase();
            Connection = new MySqlConnection(new MySqlConnectionStringBuilder
            {
                Server = Host,
                Port = Port,
                Database = Database,
                UserID = Username,
                Password = Password
            }.GetConnectionString(true));
            Connection.Open();

            if(LoggedDirPaths != null)
                foreach (var dirPath in LoggedDirPaths)
                    foreach (var filePath in Directory.GetFiles(dirPath))
                        AddLogFile(filePath);

            if (LoggedFilePaths != null)
                foreach (var filePath in LoggedFilePaths)
                    if(LoggedFiles.All(lFile => lFile.Path != filePath))
                        AddLogFile(filePath);
        }

        private void AddLogFile(string filePath)
        {
            try { LoggedFiles.Add(LogFile.Create(filePath, Connection)); }
            catch (ReportToConsoleException e) { Console.WriteLine(e.Message); }
        }
        private void CheckDatabase()
        {
            var connStr = new MySqlConnectionStringBuilder
            {
                Server = Host,
                Port = Port,
                UserID = Username,
                Password = Password
            }.GetConnectionString(true);
            using (var conn = new MySqlConnection(connStr))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"CREATE DATABASE IF NOT EXISTS `{Database}`;";
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }
        public void Stop()
        {
            SaveConfig();
        }

        public bool ExecuteCommand(string command)
        {
            return true;
        }
    }
}