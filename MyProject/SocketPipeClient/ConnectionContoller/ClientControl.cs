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
        List<BasicTradeOrder> _tradeOrderList = new List<BasicTradeOrder>();
        public ClientControl()
        {
            _tradeOrderList = new List<BasicTradeOrder>();
            _tradeOrderList.Add(new RateTradeOrder("USDJPYpro30", "20150622__172651__151", 2));
            _tradeOrderList.Add(new RateTradeOrder("USDJPYpro30", "20150622__172656__283", 3));
            _tradeOrderList.Add(new RateTradeOrder("USDJPYpro30", "20150622__064758__749", 4));
            _tradeOrderList.Add(new RateTradeOrder("USDJPYpro30", "20150622__172651__151", 5));
            _tradeOrderList.Add(new RateTradeOrder("USDJPYpro30", "20150623__074144__028", 6));
            _tradeOrderList.Add(new RateTradeOrder("USDJPYpro30", "20150625__231317__582", 7));
            _tradeOrderList.Add(new KDJTradeOrder("USDJPYpro1", "20150716__210730__425", 8));

        }

        public void StartListen()
        {
            _sender = new SocketDeamonSender();

            RateDataController rateDataCtrl = new RateDataController(_sender);


            // rateDataCtrl.AddSymbol();

            foreach(BasicTradeOrder order in _tradeOrderList)
            {
                RataDataCollectTask collector = rateDataCtrl.FindByName(order.SymbolName);
                if (collector == null)
                    continue;

                collector.OnChange += new RataDataCollectTask.ChangeHandler(order.DoTrade);
                collector.UpdateInterval = 0.2;
            }

            rateDataCtrl.Start();
        }
    }
}
