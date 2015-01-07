using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using SocketTestClient.Sender;
using SocketTestClient.RequestObject;
using SocketTestClient.RateDataController;

namespace SocketTestClient.ConnectionContoller
{
    class ClientControl
    {
        
        SocketDeamonSender _sender;
        public ClientControl()
        {

  
        }

        public void StartListen()
        {
            IRequest rcvReq, sendReq;
            bool isDoSend;
            RateDataController rateDataCtrl = new RateDataController();
            _sender = new SocketDeamonSender();
            while (true)
            {
                if (_sender.State != DeamonState.Connected)
                {
                    Thread.Sleep(2000);
                    continue;
                }

                rcvReq = _sender.Get();
                if (rcvReq == null)
                {
                    // DealWithSend
                    isDoSend = false;

                    sendReq = rateDataCtrl.GetRequest();
                    if( sendReq != null )
                    {
                        isDoSend = true;
                        _sender.Send(sendReq);
                    }

                    if (isDoSend == false)
                        Thread.Sleep(100);
                }
                else
                {
                    if (rcvReq.GetType() == typeof(RateDataIndicateRequest))
                    {
                        rateDataCtrl.SetResult(rcvReq);
                    }
                }

            }
        }


    }
}
