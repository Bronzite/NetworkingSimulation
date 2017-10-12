using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkSimulation
{
    public interface IConnection
    {
        void ReceiveData(byte[] bData);
        byte[] GetData();
    }
}
