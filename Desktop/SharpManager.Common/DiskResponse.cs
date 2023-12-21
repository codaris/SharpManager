using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpManager
{

    /// <summary>
    /// Response from the disk drive class 
    /// </summary>
    public class DiskResponse
    {
        /// <summary>
        /// Gets the raw disk response data.
        /// </summary>
        public byte[] Data { get; }

        /// <summary>
        /// Gets the capture flag.  This indicates that the arduino should immediately retreive another disk command from the pocket computer
        /// </summary>
        public bool Capture { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiskResponse"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="capture">if set to <c>true</c> [capture].</param>
        public DiskResponse(byte[] data, bool capture)
        {
            this.Data = data;   
            this.Capture = capture; 
        }
    }
}
