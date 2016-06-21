using System;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

using MySql.Data.MySqlClient;

namespace PZLogger.NET.LogFiles
{
    public class ItemLogFile : LogFile
    {
        protected override string TableName { get; } = "Item";

        public ItemLogFile(string path, MySqlConnection connection) : base(path, connection) { }

        protected override MySqlCommand CreateCommand()
        {
            return new MySqlCommand($@"
CREATE TABLE IF NOT EXISTS `{TableName}` (
    `ID`            INT(11) UNSIGNED NOT NULL AUTO_INCREMENT,
    `Date`          INT(11) UNSIGNED NOT NULL,
    `PlayerCode`    INT(18) NOT NULL,
    `PlayerName`    VARCHAR(64) NOT NULL,
    `Place`         VARCHAR(32) NOT NULL,
    `Quantity`      INT(3) NOT NULL,
    `X`             INT(11) NOT NULL,
    `Y`             INT(11) NOT NULL,
    `Z`             INT(11) NOT NULL,
    `Item`          VARCHAR(32) NOT NULL,
    `Path`          VARCHAR(260) NOT NULL COMMENT 'file it was read from',
    PRIMARY KEY (`ID`)
) ENGINE=INNODB DEFAULT CHARSET=UTF8;", Connection);
        }
        protected override MySqlCommand InsertCommand(dynamic data)
        {
            return new MySqlCommand($@"
INSERT INTO `{TableName}` (`Date`, `PlayerCode`, `PlayerName`, `Place`, `Quantity`, `X`, `Y`, `Z`, `Item`, `Path`)
VALUES ({data.Date}, {data.PlayerCode}, '{data.PlayerName}', '{data.Place}', {data.Quantity}, {data.X}, {data.Y}, {data.Z}, '{data.Item}', '{Path_DB}');", Connection);
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
            data.Place = split[3];
            data.Quantity = split[4];

            var location = split[5].Split(',');
            data.X = location[0];
            data.Y = location[1];
            data.Z = location[2];

            if (split.Count > 6)
                data.Item = split[6].TrimEnd('.').Trim('[', ']');
            else
                data.Item = "no item";
            
                    
            using (var command = InsertCommand(data))
                command.ExecuteNonQuery();
        }
    }
}