using System;

namespace NetworkSimulation
{
    public class EthernetFrame
    {
        public MACAddress SourceAddress { get; set; }
        public MACAddress DestinationAddress { get; set; }
        public UInt16 Length { get; set; }

        public byte[] Payload { get; set; }

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
