using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using SocketTestClient.Sender;
using SocketTestClient.RequestObject;

namespace SocketTestClient.ConnectionContoller
{
    class ClientControl
    {
        private List<IRequestController> _ctrlList;
        
        SocketDeamonSender _sender;
        public ClientControl()
        {

        }


        public void Add(IRequestController ctrl)
        {
            _ctrlList.Add(ctrl);
        }

        public void StartListen()
        {
            IRequest rcvReq, sendReq;
            bool isDoSend;
            bool isNeedInit = true;


            _sender = new SocketDeamonSender();
            while (true)
            {
                if (_sender.State == DeamonState.Disconnected)
                {
                    Thread.Sleep(500);
                    isNeedInit = true;
                    continue;
                }

                if (isNeedInit == true)
                {
                    Init();
                    isNeedInit = false;
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

        private void Init()
        {
            _ctrlList = new List<IRequestController>();
            OrderSendController orderCtrl = new OrderSendController();
            _ctrlList.Add(orderCtrl);
            RateDataController rateDataCtrl = new RateDataController();
            _ctrlList.Add(rateDataCtrl);
        }


    }
}
