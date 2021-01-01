using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EssentialsSql.Settings
{
    public class Settings
    {
        public DbOptions Database { get; set; } = null;
        public FsOptions FileSystem { get; set; } = null;
        public bool LogToConsole { get; set; } = false;
    }
}
