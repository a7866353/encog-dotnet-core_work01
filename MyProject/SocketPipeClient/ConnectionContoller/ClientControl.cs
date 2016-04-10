﻿using System;
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
#if false
            _tradeOrderList.Add(new RateTradeOrder("USDJPYpro30", "20150622__172651__151", 2));
            _tradeOrderList.Add(new RateTradeOrder("USDJPYpro30", "20150622__172656__283", 3));
            _tradeOrderList.Add(new RateTradeOrder("USDJPYpro30", "20150622__064758__749", 4));
            _tradeOrderList.Add(new RateTradeOrder("USDJPYpro30", "20150622__172651__151", 5));
            _tradeOrderList.Add(new RateTradeOrder("USDJPYpro30", "20150623__074144__028", 6));
            _tradeOrderList.Add(new RateTradeOrder("USDJPYpro30", "20150625__231317__582", 7));
            _tradeOrderList.Add(new KDJTradeOrder("USDJPYpro1", "20150716__210730__425", 8));
#endif
        }

        public void StartListen()
        {
            _sender = new SocketDeamonSender();

            RateDataController rateDataCtrl = new RateDataController(_sender);
            // _tradeOrderList.Add(new NewTradeOrder("USDJPY_30", "Controller2016/3/22 21:55:27", 1, _sender)); // 116
            // _tradeOrderList.Add(new NewTradeOrder("USDJPY_30", "Controller2016/3/22 21:56:32", 2, _sender)); // 112
            // _tradeOrderList.Add(new NewTradeOrder("USDJPY_30", "Controller2016/3/22 21:35:41", 3, _sender)); // 115
            _tradeOrderList.Add(new NewTradeOrder("USDJPY_30", "Controller2016/4/4 18:34:04", 4, _sender)); // 115
            _tradeOrderList.Add(new NewTradeOrder("USDJPY_30", "Controller2016/4/4 18:26:34", 5, _sender)); // 115
            _tradeOrderList.Add(new NewTradeOrder("USDJPY_30", "Controller2016/4/4 18:53:45", 6, _sender)); // 115
            _tradeOrderList.Add(new NewTradeOrder("USDJPY_30", "Controller2016/4/5 3:54:55", 7, _sender)); // 115
            _tradeOrderList.Add(new NewTradeOrder("USDJPY_30", "Controller2016/4/6 1:27:35", 8, _sender)); // 115
            _tradeOrderList.Add(new NewTradeOrder("USDJPY_5", "Controller2016/4/6 19:07:07", 9, _sender)); // 115


            // rateDataCtrl.AddSymbol();

            foreach(BasicTradeOrder order in _tradeOrderList)
            {
                RataDataCollectTask collector = rateDataCtrl.FindByName(order.Name);
                if (collector == null)
                    continue;

                collector.OnChange += new RataDataCollectTask.ChangeHandler(order.DoTrade);
                collector.UpdateInterval = 0.2;
            }

            rateDataCtrl.Start();
        }
    }
}
