using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NetworkSimulation
{
    public class Serialization
    {
        public static byte[] Serialize(object o)
        {
            MemoryStream retval = new MemoryStream();
            
            if(o is EthernetFrame)
            {
                EthernetFrame ef = o as EthernetFrame;
                BinaryWriter bw = new BinaryWriter(retval);
                bw.Write(ef.DestinationAddress.Address);
                bw.Write(ef.SourceAddress.Address);
                bw.Write(ef.Length);
                bw.Write(ef.Payload);
                bw.Write(ef.Checksum);
            }


            retval.Position = 0;
            return retval.ToArray();
        }
    }
}
