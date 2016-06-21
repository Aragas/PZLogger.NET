using System;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

using MySql.Data.MySqlClient;

namespace PZLogger.NET.LogFiles
{
    public class UserLogFile : LogFile
    {
        protected override string TableName { get; } = "User";

        public UserLogFile(string path, MySqlConnection connection) : base(path, connection) { }

        protected override MySqlCommand CreateCommand()
        {
            return new MySqlCommand($@"
CREATE TABLE IF NOT EXISTS `{TableName}` (
    `ID`            INT(11) UNSIGNED NOT NULL AUTO_INCREMENT,
    `Date`          INT(11) UNSIGNED NOT NULL,
    `PlayerCode`    INT(18) NOT NULL,
    `PlayerName`    VARCHAR(64) NOT NULL,
    `Action`        VARCHAR(256) NOT NULL,
    `Path`          VARCHAR(260) NOT NULL COMMENT 'file it was read from',
    PRIMARY KEY (`ID`)
)  ENGINE=INNODB DEFAULT CHARSET=UTF8;", Connection);
        }
        protected override MySqlCommand InsertCommand(dynamic data)
        {
            return new MySqlCommand($@"
INSERT INTO `{TableName}` (`Date`, `PlayerCode`, `PlayerName`, `Action`, `Path`)
VALUES ({data.Date}, '{data.PlayerCode}', '{data.PlayerName}', '{data.Action}', '{Path_DB}');", Connection);
        }

        protected override void ProcessLine(string line)
        {
            base.ProcessLine(line);
            //return;


            dynamic data = new ExpandoObject();

            line = line.Replace("'", "''");
            var split = Regex.Matches(line, @"[\[].+?[\]]|[\""].+?[\""]|[^ ]+").Cast<Match>().Select(m => m.Value).ToList();

            data.Date = DateTime.ParseExact(split[0].Trim('[', ']'), "dd-MM-yy HH:mm:ss", CultureInfo.InvariantCulture).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            if (Regex.IsMatch(split[1], @"^\d+$"))
            {
                data.PlayerCode = split[1];
                data.PlayerName = split[2].Trim('"');
                data.Action = string.Join(" ", split.ToArray(), 3, split.Count - 3);
            }
            else
            {
                data.PlayerCode = split[4].TrimEnd('.');
                data.PlayerName = "";
                data.Action = string.Join(" ", split.ToArray(), 1, split.Count - 2);
            }


            using (var command = InsertCommand(data))
                command.ExecuteNonQuery();
        }
    }
}