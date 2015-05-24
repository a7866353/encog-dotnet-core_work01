using SocketTestClient.Sender;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SocketTestClient.ConnectionContoller;

namespace SocketTestClient.RequestObject
{
    class RateRequest : IRequest
    {
        public RequestType OrderType = RequestType.RateDataRequire;

        public string SymbolName;
        public int TimeFrame;
        public DateTime StartTime;
        public DateTime StopTime;

        public byte[] GetBytes()
        {
            DataSendBuffer sb = new DataSendBuffer();
            sb.Add((int)OrderType);
            sb.Add(SymbolName);
            sb.Add(TimeFrame);
            sb.Add(StartTime);
            sb.Add(StopTime);

            return sb.GetBytes();
        }


        public RateDataRequestController ReqCtrl;
        public void FromBytes(byte[] data, int length)
        {
            RateDataIndicateRequest idc = new RateDataIndicateRequest();
            idc.FromBytes(data, length);
            ReqCtrl.SetResult(idc);
        }
    }
}
