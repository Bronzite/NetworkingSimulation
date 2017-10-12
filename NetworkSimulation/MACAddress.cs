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

        public override string ToString()
        {
            string s = BitConverter.ToString(mAddress); ;
            return s;
        }

        public override bool Equals(object obj)
        {
            if (obj is MACAddress)
            {
                MACAddress other = obj as MACAddress;
                for(int i =0;i<other.mAddress.Length;i++)
                {
                    if(mAddress[i] != other.mAddress[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            else
                return false;

            
        }
    }
}
