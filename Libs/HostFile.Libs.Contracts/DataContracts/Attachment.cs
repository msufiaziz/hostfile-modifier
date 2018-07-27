using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostFile.Libs.Contracts.DataContracts
{
    public class Attachment
    {
        public string Id { get; set; }

        public string Filename { get; set; }

        public byte[] Content { get; set; }

        public override string ToString()
        {
            int contentLength = Content != null ? Content.Length : 0;
            return $"Id:{Id}, Filename: {Filename}, Content-Length: {contentLength}";
        }
    }
}
