using System;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

using MySql.Data.MySqlClient;

namespace PZLogger.NET.LogFiles
{
    public class CmdLogFile : LogFile
    {
        protected override string TableName { get; } = "Cmd";

        public CmdLogFile(string path, MySqlConnection connection) : base(path, connection) { }

        protected override MySqlCommand CreateCommand()
        {
            return new MySqlCommand($@"
CREATE TABLE IF NOT EXISTS `{TableName}` (
    `ID`            INT(11) UNSIGNED NOT NULL AUTO_INCREMENT,
    `Date`          INT(11) UNSIGNED NOT NULL,
    `PlayerCode`    INT(18) NOT NULL,
    `PlayerName`    VARCHAR(64) NOT NULL,
    `Command`       VARCHAR(64) NOT NULL,
    `Path`          VARCHAR(260) NOT NULL COMMENT 'file it was read from',
    PRIMARY KEY (`ID`)
)  ENGINE=INNODB DEFAULT CHARSET=UTF8;", Connection);
        }
        protected override MySqlCommand InsertCommand(dynamic data)
        {
            return new MySqlCommand($@"
INSERT INTO `{TableName}` (`Date`, `PlayerCode`, `PlayerName`, `Command`, `Path`)
VALUES ({data.Date}, '{data.PlayerCode}', '{data.PlayerName}', '{data.Command}', '{Path_DB}');", Connection);
        }

        protected override void ProcessLine(string line)
        {
            base.ProcessLine(line);


            dynamic data = new ExpandoObject();

            line = line.Replace("'", "''");
            var split = Regex.Matches(line, @"[\[].+?[\]]|[\""].+?[\""]|[^ ]+").Cast<Match>().Select(m => m.Value).ToList();

            data.Date = DateTime.ParseExact(split[0].Trim('[', ']'), "dd-MM-yy HH:mm:ss", CultureInfo.InvariantCulture).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            data.PlayerCode = split[1];
            data.PlayerName = split[2].Trim('"');
            data.Command = split[3];


            using (var command = InsertCommand(data))
                command.ExecuteNonQuery();
        }
    }
}