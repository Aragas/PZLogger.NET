using System;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

using MySql.Data.MySqlClient;

namespace PZLogger.NET.LogFiles
{
    public class ChatLogFile : LogFile
    {
        protected override string TableName { get; } = "Chat";

        public ChatLogFile(string path, MySqlConnection connection) : base(path, connection) { }

        protected override MySqlCommand CreateCommand()
        {
            return new MySqlCommand($@"
CREATE TABLE IF NOT EXISTS `{TableName}` (
    `ID`            INT(11) UNSIGNED NOT NULL AUTO_INCREMENT,
    `Date`          INT(11) UNSIGNED NOT NULL,
    `PlayerCode`    INT(18) NOT NULL,
    `PlayerName`    VARCHAR(64) NOT NULL,
    `Type`          VARCHAR(1) NOT NULL,
    `Message`       VARCHAR(512) NOT NULL,
    `Path`          VARCHAR(260) NOT NULL COMMENT 'file it was read from',
    PRIMARY KEY (`ID`)
)  ENGINE=INNODB DEFAULT CHARSET=UTF8;", Connection);
        }
        protected override MySqlCommand InsertCommand(dynamic data)
        {
            return new MySqlCommand($@"
INSERT INTO `{TableName}` (`Date`, `PlayerCode`, `PlayerName`, `Type`, `Message`, `Path`)
VALUES ({data.Date}, {data.PlayerCode}, '{data.PlayerName}', '{data.Type}', '{data.Message}', '{Path_DB}');", Connection);
        }

        protected override void ProcessLine(string line)
        {
            base.ProcessLine(line);


            dynamic data = new ExpandoObject();

            line = line.Replace("'", "''");
            var split = Regex.Matches(line, @"[\[].+?[\]]|[\""].+?[\""]|[^ ]+").Cast<Match>().Select(m => m.Value).ToList();

            if (!line.StartsWith("/"))
            {
                data.Date = DateTime.ParseExact(split[0].Trim('[', ']'), "dd-MM-yy HH:mm:ss", CultureInfo.InvariantCulture).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                data.PlayerCode = split[1];
                data.PlayerName = split[2].TrimStart('"').TrimEnd('"');
                data.Type = split[3];
                data.Message = split[4].TrimStart('/', '"', '[').TrimEnd('/', '"', ']');
            }
            else
            {
                data.Date = 0;
                data.PlayerCode = 0;
                data.PlayerName = "SERVER";
                data.Type = "?";
                data.Message = line;
            }
            

            using (var command = InsertCommand(data))
                command.ExecuteNonQuery();
        }
    }
}