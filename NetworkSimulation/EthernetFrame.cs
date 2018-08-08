using System;

namespace NetworkSimulation
{
    public class EthernetFrame
    {
        public MACAddress SourceAddress { get; set; }
        public MACAddress DestinationAddress { get; set; }
        public UInt16 Length { get; set; }

        private byte[] mPayload;
        public byte[] Payload {
            get { return mPayload; }
            set
            {
                mPayload = value;
                Length = (ushort)(Payload.Length + 18);
                if(Length < 64)
                {
                    //Payload is too short.  Throw exception.
                    throw new Exception("Payload Lenght too short.");
                }
            }
        }

        public UInt32 Checksum { get; set; }
        public bool CheckCRC()
        {
            byte[] bData = Serialization.Serialize(this);
            UInt32 result = ComputeCRC(bData);
            if (result == Checksum) return true;
            return false;
        }

        public void SetChecksum() { Checksum = ComputeCRC(Serialization.Serialize(this)); }
        private UInt32 ComputeCRC (byte[] bData)
        {
            UInt32 accumulator = 0xffffffff;
            //UInt32 magic = 0xEDB88320;
            for (int i = 0; i < bData.Length; i++) //loop through each byte
            {
                byte lookup = (byte)((accumulator & 0xFF) ^ bData[i]);
                //byte lookup = (byte)((accumulator ^ bData[i]) & 0xFF);
                accumulator = (UInt32)((accumulator >> 8) ^ CRCLookup(lookup));
            }
            return accumulator ^ 0xFFFFFFFF;
        }

        public static UInt32 CRCLookup(byte input)
        {
            UInt32 retval = input;
            for (uint j = 8; j > 0; --j)
            {
                if ((retval & 0x00000001) == 1)
                {
                    retval = (uint)((retval >> 1) ^ 0xedb88320);
                }
                else
                {
                    retval = retval >> 1;
                }
            }
            return retval;
        }

    }
}
