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
    enum OrderCommand
    {
        Nothing = 0,
        Buy,
        Sell,
    }
    class TradeOrder
    {
        private RateDataControlDAO _dataController;
        private NetworkController _networkController;
        private ITradeDesisoin _decisionCtrl;
        private DateTime _lastTradeTime;

        public TradeOrder(string rateDataControllerName, string networkControllerName)
        {
            _dataController = RateDataControlDAO.GetByName(rateDataControllerName);
            _networkController = NetworkController.Open(networkControllerName);
            _decisionCtrl = _networkController.GetDecisionController();
            _lastTradeTime = DateTime.Now;
        }

        public string SymbolName
        {
            get { return _dataController.SymbolName; }
        }

        public OrderCommand GetNextCommand()
        {

            _dataController.Update();
            if (_dataController.LastItemTime > _lastTradeTime)
            {
                _lastTradeTime = _dataController.LastItemTime;
                RateData[] rateDataArr = _dataController.Get(_lastTradeTime, _decisionCtrl.InputDataLength);
                return Calculte(rateDataArr);
            }
            return OrderCommand.Nothing;
        }
        private OrderCommand Calculte(RateData[] rateDataArr)
        {
            double[] dataArr = new double[rateDataArr.Length];
            for (int i = 0; i < dataArr.Length; i++)
                dataArr[i] = Normallize((rateDataArr[i].high + rateDataArr[i].low) / 2);
            MarketActions res = _decisionCtrl.GetAction(dataArr);

            return ChoseAction(res);
        }

        private double Normallize(double value)
        {
            return (value + _networkController.DataOffset) * _networkController.DataScale;
        }

        private OrderCommand ChoseAction(MarketActions result)
        {
            OrderCommand cmd;
             // Do action
            switch (result)
            {
                case MarketActions.Nothing:
                    cmd = OrderCommand.Nothing;
                    break;
                case MarketActions.Buy:
                    cmd = OrderCommand.Buy;
                    break;
                case MarketActions.Sell:
                    cmd = OrderCommand.Sell;
                    break;
                default:
                    cmd = OrderCommand.Nothing;
                    break;
            }
            return cmd;
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


        }
        public IRequest GetRequest()
        {
            foreach(TradeOrder order in _tradeOrderList)
            {
                OrderCommand cmd = order.GetNextCommand();
                if (cmd == OrderCommand.Nothing)
                    continue;
                SendOrderRequest req = new SendOrderRequest();
                req.SymbolName = order.SymbolName;
                if (cmd == OrderCommand.Buy)
                {
                    req.OrderCmd = SendOrderRequest.Cmd.Buy;
                    Printf("Buy");
                }
                else if (cmd == OrderCommand.Sell)
                {
                    req.OrderCmd = SendOrderRequest.Cmd.Sell;
                    Printf("Sell");
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
