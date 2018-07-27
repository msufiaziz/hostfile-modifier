using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostFile.Libs.Contracts.DataContracts
{
    public class DnsObject
    {
        public string FirstAddress { get; }

        public string SecondAddress { get; }

        public DnsObject(string firstAddress, string secondAddress)
        {
            FirstAddress = firstAddress;
            SecondAddress = secondAddress;
        }
    }
}
