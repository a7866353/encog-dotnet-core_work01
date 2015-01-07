using SocketTestClient.Sender;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketTestClient.RequestObject
{
    class TestRequest : IRequest
    {
        public RequestType OrderType = RequestType.TestRequire;
        public double doubleTest = 0.5;
        public int intTest = 1234567;
        public short shortTest = 1234;
        public byte charTest = 123;


        private SendOrderResult _result;

        public byte[] GetBytes()
        {
            // Clear Result
            _result = SendOrderResult.None;

            DataSendBuffer sb = new DataSendBuffer();
            sb.Add((int)OrderType);
            sb.Add(doubleTest);
            sb.Add(intTest);
            sb.Add(shortTest);
            sb.Add(charTest);

            return sb.GetBytes();
        }

        public void FromBytes(byte[] data, int length)
        {
            DataRcvBuffer rb = new DataRcvBuffer(data, length);
            if (rb.GetBool() == true)
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
