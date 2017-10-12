using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkSimulation.Devices
{
    public class Hub : IDevice
    {
        public Hub(int iPorts)
        {
            mAdapters = new EthernetAdapter[iPorts];
            for (int i = 0; i < iPorts; i++)
            {
                mAdapters[i] = new EthernetAdapter();
                mAdapters[i].PromiscuousMode = true;
            }
        }

        public int SentPackets { get; set; }
        public int ReceivedPackets { get; set; }

        public void Cycle()
        {
            for (int i = 0; i < mAdapters.Length; i++)
            {
                mAdapters[i].ParseFrames();
                while (mAdapters[i].FramesAvailable)
                {
                    EthernetFrame curFrame = mAdapters[i].GetNextFrame();
                    mRepeatFrames.Enqueue(curFrame);
                    ReceivedPackets++;
                }
            }

            while (mRepeatFrames.Count > 0)
            {
                EthernetFrame curFrame = mRepeatFrames.Dequeue();
                for (int i = 0; i < mAdapters.Length; i++)
                {
                    mAdapters[i].TransmitFrame(curFrame);
                    SentPackets++;
                }
            }
        }

        private EthernetAdapter[] mAdapters;

        public int PortCount { get { return mAdapters.Length; } }
        public EthernetAdapter GetPort(int i)
        {
           return mAdapters[i];
        }
        public void AttachCable(int iPortNumber,Medium.Connector oConnector)
        {
            if(iPortNumber >= 0 && iPortNumber < mAdapters.Length )
            {
                if(oConnector != null)
                {
                    oConnector.ConnectedObject = mAdapters[iPortNumber ];
                }
                else
                {
                    throw new Exception("Cannot plug in null Connector");

                }
            }
            else
            {
                throw new Exception("Cannot connect medium to port; no such port number.");
            }
        }

        private Queue<EthernetFrame> mRepeatFrames = new Queue<EthernetFrame>();
    }
}
