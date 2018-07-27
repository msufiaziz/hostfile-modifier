using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostFile.Libs.Contracts.DataContracts
{
    public class Source
    {
        public string Name { get; set; }
        public string FirstDNS { get; set; }
        public string SecondDNS { get; set; }

        public override string ToString()
        {
            return $"{Name}, {FirstDNS}, {SecondDNS}";
        }
    }

    public class StorageInfo
    {
        public string Path { get; set; }
    }
}
