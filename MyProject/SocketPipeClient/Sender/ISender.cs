using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketTestClient.Sender
{
    interface ISender
    {
        int Send(IRequest req);
    }

    interface IRequest
    {
        byte[] GetBytes();
        void FromBytes(byte[] data, int length);
    }
}
