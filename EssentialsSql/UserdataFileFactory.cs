using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLFS;

namespace EssentialsSql
{
    public class UserdataFileFactory : IFileFactory<UserdataFile>
    {
        public UserdataFile Create(string filename, byte[] fileContent) => 
            new UserdataFile(filename, fileContent);

        public UserdataFile Create(DbDataReader reader) => 
            new UserdataFile(reader);

        public UserdataFile Create(string filename)
        {
            var now = DateTime.Now;

            return new UserdataFile(filename)
            {
                CreationTime = now,
                AccessTime = now,
                LastModifyTime = now
            };
        }
            
    }
}
