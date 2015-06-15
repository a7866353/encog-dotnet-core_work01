using MyProject01.Controller;
using MyProject01.DAO;
using SocketTestClient.RateDataController;
using SocketTestClient.RequestObject;
using SocketTestClient.Sender;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketTestClient.ConnectionContoller
{
    class TradeOrder
    {
        private RateDataControlDAO _dataController;
        private ITradeDesisoin _decisionCtrl;
        private DateTime _lastTradeTime;
        public int MagicNumber;

        public TradeOrder(string rateDataControllerName, string networkControllerName, int magicNumber)
        {
            _dataController = RateDataControlDAO.GetByName(rateDataControllerName);
            NetworkController _networkController = NetworkController.Open(networkControllerName);
            _decisionCtrl = _networkController.GetDecisionController();
			// For Debug at first run
            _lastTradeTime = DateTime.Now.AddDays(-10);
            MagicNumber = magicNumber;
        }

        public string SymbolName
        {
            get { return _dataController.SymbolName; }
        }

        public MarketActions GetNextCommand()
        {
            _dataController.Update();
            if (_dataController.LastItemTime > _lastTradeTime)
            {
                _lastTradeTime = _dataController.LastItemTime;
                RateData[] rateDataArr = _dataController.GetByEndTime(_lastTradeTime, _decisionCtrl.InputDataLength);
                return Calculte(rateDataArr);
            }
            return MarketActions.Nothing;
        }
        private MarketActions Calculte(RateData[] rateDataArr)
        {
            double[] dataArr = new double[rateDataArr.Length];
            for (int i = 0; i < dataArr.Length; i++)
                dataArr[i] = (rateDataArr[i].high + rateDataArr[i].low) / 2;
            MarketActions res = _decisionCtrl.GetAction(dataArr);

            return res;
        }

    }
    class OrderSendController : IRequestController
    {
        private List<TradeOrder> _tradeOrderList;

        public OrderSendController()
        {
            _tradeOrderList = new List<TradeOrder>();
            // _tradeOrderList.Add(new TradeOrder("test01", "Long_5MinTest02"));
            // _tradeOrderList.Add(new TradeOrder("test01", "Long_01"));
            // _tradeOrderList.Add(new TradeOrder("test01", "Short_01"));
            // _tradeOrderList.Add(new TradeOrder("USDJPYpro30", "20150614__083013__256", 1));
            _tradeOrderList.Add(new TradeOrder("USDJPYpro5", "20150614__083013__256", 1));
            _tradeOrderList.Add(new TradeOrder("USDJPYpro30", "20150615__005834__666", 2));
            _tradeOrderList.Add(new TradeOrder("USDJPYpro30", "20150615__005834__654", 3));


        }
        public IRequest GetRequest()
        {
            foreach(TradeOrder order in _tradeOrderList)
            {
                MarketActions cmd = order.GetNextCommand();
                if (cmd == MarketActions.Nothing)
                    continue;
                SendOrderRequest req = new SendOrderRequest();
                req.SymbolName = order.SymbolName;
                req.MagicNumber = order.MagicNumber;
                if (cmd == MarketActions.Buy)
                {
                    req.OrderCmd = SendOrderRequest.Cmd.Buy;
                    Printf("Buy");
                }
                else if (cmd == MarketActions.Sell)
                {
                    req.OrderCmd = SendOrderRequest.Cmd.Sell;
                    Printf("Sell");
                }
                else if (cmd == MarketActions.Close)
                {
                    req.OrderCmd = SendOrderRequest.Cmd.Close;
                    Printf("Close");
                }
                else
                    throw (new Exception("Parm error!"));

                return req;
            }
            return null;
        }
        public void SetResult(IRequest req)
        {
            // Nothing to do.
        }

        private void Printf(string str)
        {
            System.Console.WriteLine("OrderSendController: " + str);
        }
    }
}
