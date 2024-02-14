using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Updater
{
    internal struct FileInfo
    {
        public FileInfo(int id, string name, string directory)
        {
            Id=id;
            Name=name;
            Directory=directory;
        }

        public int Id { get; }
        public string Name { get; }
        public string Directory { get; }
    }
}
