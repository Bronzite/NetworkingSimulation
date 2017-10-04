using System;
using System.Collections.Generic;
using System.Text;

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
        }


        private Queue<EthernetFrame> mArrivedFrames;
        private byte[] mLocalBuffer;
        public MACAddress MACAddress { get; set; }

        public void DeliverFrame(EthernetFrame frame)
        {
            mArrivedFrames.Enqueue(frame);
        }
        
        public void ReceiveData(byte[] bData)
        {
            byte[] newLocalBuffer = new byte[mLocalBuffer.Length + bData.Length];
            Array.Copy(mLocalBuffer, newLocalBuffer, mLocalBuffer.Length);
            Array.Copy(bData, 0, newLocalBuffer, mLocalBuffer.Length, bData.Length);
        }

        public int ParseFrames()
        {
            int retval = 0;
            bool bCouldBeMoreFrames = true;
            int iStart = 0;
            int iStop = 0;
            while(bCouldBeMoreFrames)
            {
                EthernetFrame curFrame = ParseFrame(mLocalBuffer, iStart, out iStop);
                if(curFrame!= null)
                {
                    mArrivedFrames.Enqueue(curFrame);
                    iStart = iStop + 1;
                    retval++;
                }
                else
                {

                    bCouldBeMoreFrames = false;
                }
            }
            byte[] newLocalBuffer = new byte[mLocalBuffer.Length - iStop];
            Array.Copy(mLocalBuffer, iStop, newLocalBuffer, 0, mLocalBuffer.Length - iStop);
            mLocalBuffer = newLocalBuffer;
            return retval;
        }

        private EthernetFrame ParseFrame(byte[] bData, int iStart, out int iStop)
        {
            EthernetFrame retval = new EthernetFrame();
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
                    iStop = 0;
                }

                if (iState == STATE_PAYLOAD)
                {
                    retval.Payload = new byte[retval.Length];
                    Array.Copy(bData, i, retval.Payload, 0, retval.Length);
                    i += retval.Length - 1;
                    iState = STATE_CRC;
                }
                if(iState==STATE_LENGTH)
                {
                    iPayloadLength = BitConverter.ToUInt16(bData, i);
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
