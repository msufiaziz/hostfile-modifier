using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HostFile.Libs.Contracts.DataContracts
{
    public class HostItem
    {
        public IPAddress IPAddress { get; set; }

        public string HostName { get; set; }

        public string Status { get; set; }

        /// <summary>
        /// Returns a string that represents the object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string retVal = $"{IPAddress}\t\t{HostName}";
            if (!string.IsNullOrEmpty(Status))
            {
                retVal += $"\t\t#{Status}";
            }

            return retVal;
        }
    }
}
