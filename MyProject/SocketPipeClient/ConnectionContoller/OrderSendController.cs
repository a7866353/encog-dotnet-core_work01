using MyProject01.Controller;
using MyProject01.DAO;
using MyProject01.Util;
using MyProject01.Util.DataObject;
using SocketTestClient.RequestObject;
using SocketTestClient.Sender;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketTestClient.ConnectionContoller
{
    abstract class BasicTradeOrder
    {
        protected RateDataControlDAO _dataController;
        protected ITradeDesisoin _decisionCtrl;
        protected DateTime _lastTradeTime;
        protected int _magicNumber;

        public BasicTradeOrder(string rateDataControllerName, string networkControllerName, int magicNumber)
        {
            _dataController = RateDataControlDAO.GetByName(rateDataControllerName);
            NetworkController _networkController = NetworkController.Open(networkControllerName);
            _decisionCtrl = _networkController.GetDecisionController();
            // For Debug at first run
            _lastTradeTime = DateTime.Now.AddDays(-10);
            _magicNumber = magicNumber;
        }
        public string SymbolName
        {
            get { return _dataController.SymbolName; }
        }
        public int MagicNumber
        {
            get { return _magicNumber; }
        }
        public MarketActions GetNextCommand()
        {
            _dataController.Update();
            if (_dataController.LastItemTime > _lastTradeTime)
            {
                _lastTradeTime = _dataController.LastItemTime;
                return GetNextCommand(_dataController, _decisionCtrl);
            }
            return MarketActions.Nothing;
        }

        abstract protected MarketActions GetNextCommand(RateDataControlDAO dataController, ITradeDesisoin decisionCtrl);
    }

    class RateTradeOrder : BasicTradeOrder
    {
        public RateTradeOrder(string rateDataControllerName, string networkControllerName, int magicNumber)
            :base(rateDataControllerName, networkControllerName, magicNumber)
        {

        }
        protected override MarketActions GetNextCommand(RateDataControlDAO dataController, ITradeDesisoin decisionCtrl)
        {
            DateTime lastTradeTime = dataController.LastItemTime;
            RateData[] rateDataArr = dataController.GetByEndTime(lastTradeTime, decisionCtrl.InputDataLength);
            return Calculte(rateDataArr);
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

    // TODO
    class KDJTradeOrder : BasicTradeOrder
    {
        public KDJTradeOrder(string rateDataControllerName, string networkControllerName, int magicNumber)
            : base(rateDataControllerName, networkControllerName, magicNumber)
        {
              
        }
        protected override MarketActions GetNextCommand(RateDataControlDAO dataController, ITradeDesisoin decisionCtrl)
        {
            DateTime lastTradeTime = dataController.LastItemTime;
            TestDataCountLoader loader = new TestDataCountLoader(_dataController.Name, DataTimeType.M5, lastTradeTime, 1000);
            KDJDataBlock dataBlock = new KDJDataBlock(loader, 0, loader.Count, decisionCtrl.InputDataLength);

            double[] buffer = new double[decisionCtrl.InputDataLength * 4];
            dataBlock.Copy(buffer, dataBlock.BlockCount - 1);

            MarketActions res = _decisionCtrl.GetAction(buffer);
            return res;
        }
    }

    class OrderSendController : IRequestController
    {
        private List<BasicTradeOrder> _tradeOrderList;

        public OrderSendController()
        {
            _tradeOrderList = new List<BasicTradeOrder>();
            _tradeOrderList.Add(new RateTradeOrder("USDJPYpro30", "20150622__172651__151", 2));
            _tradeOrderList.Add(new RateTradeOrder("USDJPYpro30", "20150622__172656__283", 3));
            _tradeOrderList.Add(new RateTradeOrder("USDJPYpro30", "20150622__064758__749", 4));
            _tradeOrderList.Add(new RateTradeOrder("USDJPYpro30", "20150622__172651__151", 5));
            _tradeOrderList.Add(new RateTradeOrder("USDJPYpro30", "20150623__074144__028", 6));
            _tradeOrderList.Add(new RateTradeOrder("USDJPYpro30", "20150625__231317__582", 7));

        }
        public IRequest GetRequest()
        {
            foreach(RateTradeOrder order in _tradeOrderList)
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
                    Printf(order.MagicNumber + ": Buy");
                }
                else if (cmd == MarketActions.Sell)
                {
                    req.OrderCmd = SendOrderRequest.Cmd.Sell;
                    Printf(order.MagicNumber + ": Sell");
                }
                else if (cmd == MarketActions.Close)
                {
                    req.OrderCmd = SendOrderRequest.Cmd.Close;
                    Printf(order.MagicNumber + ": Close");
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
