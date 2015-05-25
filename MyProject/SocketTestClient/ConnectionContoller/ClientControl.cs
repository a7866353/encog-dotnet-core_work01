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
        private List<IRequestController> _ctrlList;
        
        SocketDeamonSender _sender;
        public ClientControl()
        {
            _ctrlList = new List<IRequestController>();
            OrderSendController orderCtrl = new OrderSendController();
            _ctrlList.Add(orderCtrl);
            RateDataRequestController rateDataCtrl = new RateDataRequestController();
            _ctrlList.Add(rateDataCtrl);
        }

        public void Add(IRequestController ctrl)
        {
            _ctrlList.Add(ctrl);
        }

        public void StartListen()
        {
            IRequest rcvReq, sendReq;
            bool isDoSend;


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
                    foreach (IRequestController ctrl in _ctrlList)
                    {
                        sendReq = ctrl.GetRequest();
                        if (sendReq != null)
                        {
                            isDoSend = true;
                            _sender.Send(sendReq);
                            break;
                        }
                    }
                    
                    
                    if (isDoSend == false)
                        Thread.Sleep(100);
                }
                else
                {
                    // Todo Nothing
                }

            }
        }


    }
}
