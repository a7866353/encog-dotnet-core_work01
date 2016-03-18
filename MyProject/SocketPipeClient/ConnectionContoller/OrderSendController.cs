using MyProject01.Controller;
using MyProject01.DAO;
using MyProject01.DataSources;
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
        protected DateTime _lastTradeTime;
        protected int _magicNumber;

        public BasicTradeOrder(string rateDataControllerName, string networkControllerName, int magicNumber)
        {
            _dataController = RateDataControlDAO.GetByName(rateDataControllerName);
            // For Debug at first run
            _lastTradeTime = DateTime.Now.AddDays(-10);
            _magicNumber = magicNumber;
        }
        public string SymbolName
        {
            get { return _dataController.SymbolName; }
        }
        public string Name
        {
            get { return _dataController.Name; }
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
                return GetNextCommand(_dataController, _lastTradeTime);
            }
            return MarketActions.Nothing;
        }

        abstract protected MarketActions GetNextCommand(RateDataControlDAO dataController, DateTime itemTime);
    }

    class RateTradeOrder : BasicTradeOrder
    {
        protected ITradeDesisoin _decisionCtrl;
        public RateTradeOrder(string rateDataControllerName, string networkControllerName, int magicNumber)
            :base(rateDataControllerName, networkControllerName, magicNumber)
        {
            NetworkController _networkController = NetworkController.Open(networkControllerName);
            _decisionCtrl = _networkController.GetDecisionController();

        }
        protected override MarketActions GetNextCommand(RateDataControlDAO dataController, DateTime itemTime)
        {
            DateTime lastTradeTime = dataController.LastItemTime;
            RateData[] rateDataArr = dataController.GetByEndTime(lastTradeTime, _decisionCtrl.InputDataLength);
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
        protected ITradeDesisoin _decisionCtrl;
        public KDJTradeOrder(string rateDataControllerName, string networkControllerName, int magicNumber)
            : base(rateDataControllerName, networkControllerName, magicNumber)
        {
            NetworkController _networkController = NetworkController.Open(networkControllerName);
            _decisionCtrl = _networkController.GetDecisionController();
             
        }
        protected override MarketActions GetNextCommand(RateDataControlDAO dataController, DateTime itemTime)
        {
            DateTime lastTradeTime = dataController.LastItemTime;
            TestDataDateRangeLoader loader = new TestDataDateRangeLoader(dataController.CollectiongName, DataTimeType.M5, lastTradeTime.AddDays(-10), lastTradeTime, 1024);
            loader.Load();
            KDJDataBlock dataBlock = new KDJDataBlock(loader, 0, loader.Count, _decisionCtrl.InputDataLength);

            double[] buffer = new double[_decisionCtrl.InputDataLength * 4];
            dataBlock.Copy(buffer, dataBlock.BlockCount - 1);

            MarketActions res = _decisionCtrl.GetAction(buffer);
            return res;
        }
    }
    class NewTradeOrder : BasicTradeOrder
    {
        protected ITradeDesisoin _decisionCtrl;
        private IController _ctrl;
        public NewTradeOrder(string rateDataControllerName, string networkControllerName, int magicNumber)
            : base(rateDataControllerName, networkControllerName, magicNumber)
        {
            ControllerDAOV2 dao = ControllerDAOV2.GetDAOByName(networkControllerName);
            _ctrl = dao.GetController();
        }
        protected override MarketActions GetNextCommand(RateDataControlDAO dataController, DateTime itemTime)
        {
            DateTime lastTradeTime = dataController.LastItemTime;
            BasicTestDataLoader loader =
                new TestDataDateRangeLoader("USDJPY", DataTimeType.M30, lastTradeTime.AddMinutes(-1*dataController.TimeFrame*1000), lastTradeTime, 2000);
            loader.Load();

            _ctrl.DataSourceCtrl = new DataSourceCtrl(loader);
            _ctrl.CurrentPosition = _ctrl.TotalLength;

            return _ctrl.GetAction();
        }

    }
    class OrderSendController : IRequestController
    {
        private List<BasicTradeOrder> _tradeOrderList;
        private RateDataController _rateDataCtrl;

        public OrderSendController(RateDataController RateDataCtrl)
        {
            _tradeOrderList = new List<BasicTradeOrder>();
            _tradeOrderList.Add(new RateTradeOrder("USDJPYpro30", "20150622__172651__151", 2));
            _tradeOrderList.Add(new RateTradeOrder("USDJPYpro30", "20150622__172656__283", 3));
            _tradeOrderList.Add(new RateTradeOrder("USDJPYpro30", "20150622__064758__749", 4));
            _tradeOrderList.Add(new RateTradeOrder("USDJPYpro30", "20150622__172651__151", 5));
            _tradeOrderList.Add(new RateTradeOrder("USDJPYpro30", "20150623__074144__028", 6));
            _tradeOrderList.Add(new RateTradeOrder("USDJPYpro30", "20150625__231317__582", 7));
            _tradeOrderList.Add(new KDJTradeOrder("USDJPYpro1", "20150716__210730__425", 8));

            _rateDataCtrl = RateDataCtrl;
            foreach (BasicTradeOrder trader in _tradeOrderList)
            {
                _rateDataCtrl.FindByName(trader.SymbolName).OnChange += 
                _rateDataCtrl.AddWatchSymbol(trader.Name, 0.5);
            }

        }
        public IRequest GetRequest()
        {
            foreach (BasicTradeOrder order in _tradeOrderList)
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
