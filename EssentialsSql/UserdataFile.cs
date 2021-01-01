using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using SQLFS;
using SQLFS.Database;
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

        [DatabaseColumn(MoneyColumn, "DECIMAL NOT NULL DEFAULT 0", MySqlDbType.Decimal, IndexName = "money_index")]
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
            var text = Encoding.UTF8.GetString(content);

            if (text.Length <= 0)
                return;

            var obj = _deserializer.Deserialize(new StringReader(text));


            if (obj is Dictionary<object, object> dict)
            {
                if (dict.TryGetValue("money", out var moneyObj) &&
                        moneyObj is string moneyString && decimal.TryParse(moneyString, out var moneyValue))
                    Money = moneyValue;

                if (dict.TryGetValue("lastAccountName", out var lastNameObj) && lastNameObj is string lastName)
                    LastName = lastName;

                if (dict.TryGetValue("npc", out var npcObj) && npcObj is string npcString &&
                    bool.TryParse(npcString, out var boolValue))
                    IsNpc = boolValue;

                if (dict.TryGetValue("timestamps", out var timestampsObj) &&
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
