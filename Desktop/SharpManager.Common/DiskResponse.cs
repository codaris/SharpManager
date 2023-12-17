using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpManager
{

    public class DiskResponse
    {
        public byte[] Data { get; }

        public bool Capture { get; }

        public DiskResponse(byte[] data, bool capture)
        {
            this.Data = data;   
            this.Capture = capture; 
        }
    }
}
