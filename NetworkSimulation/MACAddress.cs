using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkSimulation
{
    public class MACAddress
    {
        public static MACAddress GeneratedAddress(Random r)
        {
            byte[] bAddress = new byte[6];
            r.NextBytes(bAddress);
            MACAddress retval = new MACAddress(bAddress);
            return retval;
        }
        public MACAddress (string sAddress)
        {
            Address = Utility.ConvertHexStringToByteArray(sAddress);
        }

        public MACAddress(byte[] bAddress)
        {
            Address = bAddress;
        }

        public string SetAddress {
            set
            {
                Address = Utility.ConvertHexStringToByteArray(value);
            }
        }

        private byte[] mAddress;
        public byte[] Address
        {
            get
            {
                return mAddress;
            }

            set
            {
                if(value.Length== 6)
                {
                    mAddress = value;
                }
                else
                {
                    throw new Exception("MAC Address must be six bytes.");
                }
            }
        }


    }
}
