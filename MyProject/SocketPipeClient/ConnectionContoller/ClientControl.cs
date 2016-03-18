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
        SocketDeamonSender _sender;
        public ClientControl()
        {

        }

        public void StartListen()
        {
            _sender = new SocketDeamonSender();

            RateDataController rateDataCtrl = new RateDataController(_sender);


            // rateDataCtrl.AddSymbol();

            rateDataCtrl.Start();
        }
    }
}
