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
        private NEATController _networkController;
        private DateTime _lastTradeTime;

        public TradeOrder(string rateDataControllerName, string networkControllerName)
        {
            _dataController = RateDataControlDAO.GetByName(rateDataControllerName);
            _networkController = NEATController.Open(networkControllerName, false, false);
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
                RateData[] rateDataArr = _dataController.Get(_lastTradeTime, _networkController.InputVectorLength);
                return Calculte(rateDataArr);
            }
            return OrderCommand.Nothing;
        }
        private OrderCommand Calculte(RateData[] rateDataArr)
        {
            double[] dataArr = new double[rateDataArr.Length];
            for (int i = 0; i < dataArr.Length; i++)
                dataArr[i] = Normallize((rateDataArr[i].high + rateDataArr[i].low) / 2);
            double[] res = _networkController.Compute(dataArr);

            return ChoseAction(res);
        }

        private double Normallize(double value)
        {
            return (value + _networkController.DataOffset) * _networkController.DataScale;
        }

        private OrderCommand ChoseAction(double[] result)
        {
            OrderCommand cmd;
            int maxActionIndex = 0;
            for (int i = 1; i < result.Length; i++)
            {
                if (result[maxActionIndex] < result[i])
                    maxActionIndex = i;
            }

            // Do action
            switch (maxActionIndex)
            {
                case 0:
                    cmd = OrderCommand.Nothing;
                    break;
                case 1:
                    cmd = OrderCommand.Buy;
                    break;
                case 2:
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
            _tradeOrderList.Add(new TradeOrder("test02", "Long_01"));

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
                    req.OrderCmd = SendOrderRequest.Cmd.Buy;
                else if (cmd == OrderCommand.Sell)
                    req.OrderCmd = SendOrderRequest.Cmd.Sell;
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
    }
}
