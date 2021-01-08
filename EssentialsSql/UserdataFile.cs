using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration.CommandLine;
using MySql.Data.MySqlClient;
using Serilog;
using SQLFS;
using SQLFS.Database;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace EssentialsSql
{
    public class UserdataFile : FileBase
    {
        public const string MoneyColumn = "money";
        public const string LastNameColumn = "last_name";
        public const string LastSeenColumn = "last_seen";
        public const string NpcColumn = "npc";

        [DatabaseColumn(MoneyColumn, "DECIMAL(20, 10) NOT NULL DEFAULT 0", MySqlDbType.Decimal, IndexName = "money_index")]
        public decimal Money { get; set; }

        [DatabaseColumn(LastNameColumn, "TINYTEXT", MySqlDbType.TinyText)]
        public string LastName { get; set; }

        [DatabaseColumn(LastSeenColumn, "BIGINT DEFAULT 0", MySqlDbType.Int64)]
        public long LastSeen { get; set; }

        [DatabaseColumn(NpcColumn, "TINYINT DEFAULT 1", MySqlDbType.UByte)]
        public bool IsNpc { get; set; }

        private static IDeserializer _deserializer;

        #region Columns
        public new static Dictionary<string, DatabaseColumn> StaticColumns = new Dictionary<string, DatabaseColumn>();
        public override Dictionary<string, DatabaseColumn> Columns { get; protected set; } = StaticColumns;

        static UserdataFile()
        {
            var properties = typeof(UserdataFile).GetProperties();

            foreach (var property in properties)
            {
                var attributes = property.GetCustomAttributes(typeof(DatabaseColumn), false);
                if (attributes.Length == 0)
                    continue;

                if (!(attributes.First() is DatabaseColumn attribute))
                    continue;

                StaticColumns.Add(attribute.Name, attribute);
            }

            _deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
        }
        #endregion

        public UserdataFile(DbDataReader dataReader) : base(dataReader)
        {
            for (var i = 0; i < dataReader.FieldCount; i++)
            {
                var name = dataReader.GetName(i);

                switch (name)
                {
                    case MoneyColumn:
                        Money = dataReader.GetDecimal(i);
                        break;

                    case LastNameColumn:
                        LastName = dataReader[i] is DBNull
                            ? null
                            : dataReader.GetString(i);

                        break;

                    case LastSeenColumn:
                        LastSeen = dataReader.GetInt64(i);
                        break;

                    case NpcColumn:
                        IsNpc = dataReader.GetBoolean(i);
                        break;
                }
            }
        }

        public UserdataFile(string filename, byte[] fileContent)
        {
            Name = filename;
            SetData(fileContent);
        }

        public override void SetData(byte[] content)
        {
            Data = content;

            if (content.All(b => b == 0))
                return;

            try
            {
                var text = Encoding.UTF8.GetString(content);

                if (string.IsNullOrWhiteSpace(text) || text.Length <= 0)
                    return;

                var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                var newLines = new List<string>(lines.Length);

                const string moneyPrefix = "money:";
                const string lastNamePrefix = "lastAccountName:";
                const string npcPrefix = "npc:";

                var interestingLines = new[]
                {
                    moneyPrefix,
                    lastNamePrefix,
                    "npc:",
                    "timestamps:",
                    "logout:",
                    "login:"
                };

                for (var i = 0; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    if (string.IsNullOrWhiteSpace(line) || line.Length <= 0)
                        continue;

                    var trimmed = line.TrimStart();

                    if (!interestingLines.Any(interesting =>
                            trimmed.StartsWith(interesting, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    newLines.Add(lines[i]);
                }

                var newText = string.Join("\n", newLines);
                var money = FindProperty(newLines, "money:");

                if (money != null)
                {
                    money = money.Replace("'", "").Replace("\"", "");

                    if (decimal.TryParse(money, out var moneyValue))
                        Money = moneyValue;
                }

                var lastNameProperty = FindProperty(newLines, lastNamePrefix);
                if (lastNameProperty != null)
                    LastName = lastNameProperty.Trim();

                var isNpcProperty = FindProperty(newLines, npcPrefix);
                if (isNpcProperty != null && bool.TryParse(isNpcProperty.Trim(), out var isNpcValue))
                    IsNpc = isNpcValue;


                try
                {
                    var obj = _deserializer.Deserialize(new StringReader(newText));

                    if (obj is Dictionary<object, object> dict && dict.TryGetValue("timestamps", out var timestampsObj) &&
                            timestampsObj is Dictionary<object, object> timestampsDict)

                    {
                        if (timestampsDict.TryGetValue("logout", out var logoutObj) &&
                            logoutObj is string logoutString && long.TryParse(logoutString, out var logoutValue))
                            LastSeen = logoutValue;
                        else if (timestampsDict.TryGetValue("login", out var loginObj) &&
                                 loginObj is string loginString && long.TryParse(loginString, out var loginValue))
                            LastSeen = loginValue;
                        else
                            LastSeen = 0;
                    }
                }
                catch (SyntaxErrorException ex)
                {
                    Log.Debug(ex, "Syntax error exception encountered.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An exception has occurred while parsing data.");
            }
        }

        private string FindProperty(IEnumerable<string> lines, string key)
        {
            foreach (var line in lines)
            {
                if (!line.StartsWith(key, StringComparison.OrdinalIgnoreCase))
                    continue;

                var value = line.Substring(key.Length);

                return value.Trim();
            }

            return null;
        }

        public override void SaveParameter(MySqlParameter param, string column)
        {
            switch (column)
            {
                case LastNameColumn:
                    param.Value = LastName;
                    break;

                case LastSeenColumn:
                    param.Value = LastSeen;
                    break;

                case MoneyColumn:
                    param.Value = Money.ToString("F");
                    break;

                case NpcColumn:
                    param.Value = IsNpc;
                    break;

                default:
                    base.SaveParameter(param, column);
                    break;
            }
        }

        public UserdataFile(string filename)
        {
            Name = filename;
        }
    }
}
