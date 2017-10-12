using System;
using System.Collections.Generic;
using System.Text;


//TODO: Need to add Ethernet II checking, and test ot make sure we handle small frames elegantly.
namespace NetworkSimulation
{
    public class EthernetAdapter:IConnection
    {
        public EthernetAdapter():this(MACAddress.GeneratedAddress(new Random()))
        {
        }

        public EthernetAdapter(MACAddress macAddress)
        {
            MACAddress = macAddress;
            mArrivedFrames = new Queue<EthernetFrame>();
            mOutgoingFrames = new Queue<EthernetFrame>();
            PromiscuousMode = false;
        }


        private Queue<EthernetFrame> mArrivedFrames;
        private Queue<EthernetFrame> mOutgoingFrames;
        public bool PromiscuousMode { get; set; }
        public ulong ReceivedBytes { private set; get; }
        public ulong TransmittedBytes { private set; get; }
        private byte[] mLocalBuffer;
        public MACAddress MACAddress { get; set; }

        public void DeliverFrame(EthernetFrame frame)
        {
            mArrivedFrames.Enqueue(frame);
        }
        
        public void TransmitFrame(EthernetFrame frame)
        {
            mOutgoingFrames.Enqueue(frame);
        }

        public bool FramesAvailable { get { return mArrivedFrames.Count > 0; } }
        public EthernetFrame GetNextFrame()
        {
            if (mArrivedFrames.Count > 0) return mArrivedFrames.Dequeue();
            return null;
        }
        public byte[] GetData()
        {
            if (mOutgoingFrames.Count > 0)
            {
                byte[] packet = Serialization.Serialize(mOutgoingFrames.Dequeue());
                byte[] retval = new byte[packet.Length + 7];
                for (int i = 0; i < 6; i++) retval[i] = 0x55;
                retval[6] = 0xD5;
                Array.Copy(packet, 0, retval, 7, packet.Length);

                TransmittedBytes += (ulong)retval.Length;
                return retval;
            }
            else
                return null;
        }

        public void ReceiveData(byte[] bData)
        {
            if (bData != null)
            {
                if (mLocalBuffer == null) mLocalBuffer = new byte[0];
                ReceivedBytes += (ulong)bData.Length;
                byte[] newLocalBuffer = new byte[mLocalBuffer.Length + bData.Length];
                Array.Copy(mLocalBuffer, newLocalBuffer, mLocalBuffer.Length);
                Array.Copy(bData, 0, newLocalBuffer, mLocalBuffer.Length, bData.Length);
                mLocalBuffer = newLocalBuffer;
            }
        }

        public int ParseFrames()
        {
            int retval = 0;
            bool bCouldBeMoreFrames = (mLocalBuffer != null);
            int iStart = 0;
            int iStop = 0;
            
            while(bCouldBeMoreFrames)
            {
                EthernetFrame curFrame = ParseFrame(mLocalBuffer, iStart, out iStop);
                if(curFrame!= null)
                {
                    if (curFrame.DestinationAddress.Equals(MACAddress) || PromiscuousMode)
                    {
                        mArrivedFrames.Enqueue(curFrame);
                        retval++;
                    }
                    iStart = iStop + 1;
                    
                }
                else
                {

                    bCouldBeMoreFrames = false;
                }
            }
            iStop--;
            if (mLocalBuffer != null && iStop >= 0)
            {
                byte[] newLocalBuffer = new byte[mLocalBuffer.Length - iStop];
                Array.Copy(mLocalBuffer, iStop, newLocalBuffer, 0, mLocalBuffer.Length - iStop);
                mLocalBuffer = newLocalBuffer;
            }
            return retval;
        }

        private EthernetFrame ParseFrame(byte[] bData, int iStart, out int iStop)
        {
            EthernetFrame retval = null;
            const int STATE_PREAMBLE = 0;
            const int STATE_DESTINATION = 1;
            const int STATE_SOURCE = 2;
            const int STATE_LENGTH = 3;
            const int STATE_PAYLOAD = 4;
            const int STATE_CRC = 5;
            int iState = STATE_PREAMBLE;
            byte[] bDestinationAddress = new byte[6];
            byte[] bSourceAddress = new byte[6];
            UInt16 iPayloadLength = 0;
            iStop = iStart;

            for(int i=iStart;i<bData.Length;i++)
            {
                byte curByte = bData[i];

                if (iState == STATE_CRC)
                {
                    retval.Checksum = BitConverter.ToUInt32(bData, i);
                    i = bData.Length;
                    iStop = i;
                }

                if (iState == STATE_PAYLOAD)
                {
                    int iTransferLength = retval.Length;
                    retval.Payload = new byte[iTransferLength];
                    Array.Copy(bData, i, retval.Payload, 0, iTransferLength);
                    i += iTransferLength - 1;
                    iState = STATE_CRC;
                }
                if(iState==STATE_LENGTH)
                {
                    iPayloadLength = BitConverter.ToUInt16(bData, i);
                    iPayloadLength -= 18;
                    if(bData.Length < i+ iPayloadLength + 4)
                    {
                        //We don't have the whole packet
                        return null;
                    }
                    i++;
                    retval.Length = iPayloadLength;
                    iState = STATE_PAYLOAD;
                }
                if (iState == STATE_SOURCE)
                {
                    Array.Copy(bData, i, bSourceAddress, 0, 6);
                    i += 5;
                    iState = STATE_LENGTH;
                    retval.SourceAddress = new MACAddress(bSourceAddress);
                }
                if (iState == STATE_DESTINATION)
                {
                    Array.Copy(bData, i, bDestinationAddress, 0, 6);
                    i += 5;
                    iState = STATE_SOURCE;
                    retval = new EthernetFrame();
                    retval.DestinationAddress = new MACAddress(bDestinationAddress);
                }
                if(iState == STATE_PREAMBLE)
                {
                    if(curByte == 0x55)
                    {
                        //Still in Preamble
                    }
                    else if(curByte == 0xD5)
                    {
                        iState = STATE_DESTINATION;
                        if(bData.Length < i + 63)
                        {
                            //This is too short to have a full Frame
                            return null;
                        }
                    }
                    else
                    {
                        //ERROR: Malformed Packet
                    }
                }
            }

            return retval;
        }

    }
}
