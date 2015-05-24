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
            List<IRequestController> ctrlList = new List<IRequestController>();
            OrderSendController orderCtrl = new OrderSendController();
            ctrlList.Add(orderCtrl);
            RateDataRequestController rateDataCtrl = new RateDataRequestController();
            ctrlList.Add(rateDataCtrl);

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
                    if (rateDataCtrl.IsFinish == true)
                    {
                        foreach (IRequestController ctrl in ctrlList)
                        {
                            sendReq = ctrl.GetRequest();
                            if (sendReq != null)
                            {
                                isDoSend = true;
                                _sender.Send(sendReq);
                                break;
                            }
                        }
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
