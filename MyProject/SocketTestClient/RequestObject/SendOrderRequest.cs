using SocketTestClient.Sender;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketTestClient.RequestObject
{
    enum SendOrderResult
    {
        None,
        OK,
        NG,
    }

    class SendOrderRequest : IRequest
    {
        public RequestType OrderType = RequestType.SendOrderRequire;
        public double Price;

        private SendOrderResult _result;

        public byte[] GetBytes()
        {
            // Clear Result
            _result = SendOrderResult.None;

            DataSendBuffer sb = new DataSendBuffer();
            sb.Add((int)OrderType);
            sb.Add(Price);

            return sb.GetBytes();
        }

        public void FromBytes(byte[] data, int length)
        {
            DataRcvBuffer rb = new DataRcvBuffer(data, length);
            if( rb.GetBool() == true )
            {
                _result = SendOrderResult.OK;
            }
            else
            {
                _result = SendOrderResult.NG;
            }
        }

        public SendOrderResult Result
        {
            get { return _result; }
        }
    }
}
