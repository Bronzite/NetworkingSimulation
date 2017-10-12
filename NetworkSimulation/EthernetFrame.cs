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
            return 0;
        }

    }
}
