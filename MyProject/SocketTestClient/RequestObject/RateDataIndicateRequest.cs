using SocketTestClient.Sender;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketTestClient.RequestObject
{
    class RateInfo
    {
        public string time;
        public double open;
        public double high;
        public double low;
        public double close;
        public bool isEnd;
    }

    class RateDataIndicateRequest : IRequest
    {
        public RequestType OrderType = RequestType.RateDataIndicate;
        public RateInfo[] RateInfoArray;
        public bool EndFlag;

        public byte[] GetBytes()
        {
            throw new NotImplementedException();
        }

        public void FromBytes(byte[] data, int length)
        {
            DataRcvBuffer rb = new DataRcvBuffer(data, length);
            rb.GetInt();
            int count = rb.GetInt();
            if (count <= 0)
                return;
            EndFlag = rb.GetBool();
            RateInfoArray = new RateInfo[count];
            for (int i = 0; i < count;i++ )
            {
                RateInfoArray[i] = new RateInfo();
                RateInfoArray[i].time = rb.GetString();
                RateInfoArray[i].open = rb.GetDouble();
                RateInfoArray[i].high = rb.GetDouble();
                RateInfoArray[i].low = rb.GetDouble();
                RateInfoArray[i].close = rb.GetDouble();
                RateInfoArray[i].isEnd = rb.GetBool();

            }
        }
    }
}
