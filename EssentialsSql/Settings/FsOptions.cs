using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SQLFS;

namespace EssentialsSql.Settings
{
    public class FsOptions : ISqlFileSystemOptions<UserdataFile>
    {
        public int Threads { get; set; } = Environment.ProcessorCount / 2;

        [JsonIgnore]
        public UserdataFile FileTemplate { get; set; } = new UserdataFile((string) null);

        [JsonIgnore]
        public IFileFactory<UserdataFile> FileFactory { get; set; } = new UserdataFileFactory();

        public string Mount { get; set; }
        public string VolumeName { get; set; }

        public long Space { get; set; }

        public long FreeSpace { get; set; }
    }
}
