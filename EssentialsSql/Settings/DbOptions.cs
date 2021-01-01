using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLFS;

namespace EssentialsSql.Settings
{
    public class DbOptions : IDatabaseOptions
    {
        public string Hostname { get; set; } = "localhost";
        public int Port { get; set; } = 3306;
        public string Database { get; set; } = "sqlfs";
        public string Table { get; set; } = "files";
        public string Username { get; set; } = "root";
        public string Password { get; set; } = "";
    }
}
