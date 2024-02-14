using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Updater
{
    internal class VersionInfo
    {
        public VersionInfo(bool canUpdate)
        {
            CanUpdate = canUpdate;
        }

        public int Id { set; get; }
        public string? NumVersion { set; get; }
        public bool CanUpdate { get; }
        public string? Date { set; get; }
        public string? Notes { set; get; }
        public List<FileInfo>? Files { set; get; }
    }
}
