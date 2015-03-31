using MyProject01.Test;
using MyProject01.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyProject01.Reinforcement;
using MyProject01.Util.DataObject;
using MyProject01.Controller;

namespace MyProject01.Agent
{
    class DataNormallizer
    {
        private double _targDataMax;
        private double _targDataMin;
        private double _dataMaxValue;
        private double _dataMinValue;
        private double[] _dataArr;

        private double _dataScale;
        private double _dataOffset;

        public void DataValueAdjust(double[] dataArr, double max, double min)
        {
            _dataArr = dataArr;
            _targDataMax = max;
            _targDataMin = min;

            _dataMaxValue = _dataMinValue = _dataArr[0];
            foreach (double data in _dataArr)
            {
                if (data > _dataMaxValue)
                    _dataMaxValue = data;
                else if (data < _dataMinValue)
                    _dataMinValue = data;
            }

            _dataScale = (_targDataMax - _targDataMin) / (_dataMaxValue - _dataMinValue);
            _dataOffset = (_dataMaxValue - _dataMinValue) * _targDataMax / (_targDataMax - _targDataMin) - _dataMaxValue;

            Normallize(_dataOffset, _dataScale);
        }
        public void Normallize(double offset, double scale)
        {
            _dataOffset = offset;
            _dataScale = scale;
            for (int i = 0; i < _dataArr.Length;i++ )
            {
                _dataArr[i] = DataConv(_dataArr[i], _dataOffset, _dataScale);
            }
        }
        private double DataConv(double value, double offset, double scale)
        {
            return (value + offset) * scale;
        }
    }
    class RateMarketAgentData
    {
        public double[] RateDataArray;
        public double Reward;
    }

    interface IRateMarketUser
    {
        MarketActions Determine(RateMarketAgentData inputData);
        double TotalErrorRate{ set; get; }

        void EpsodeEnd();
    }

    class Order
    {
#if true
        public double BuyOffset = 0.0;
        public double SellOffset = 0.0;
#else
        public double BuyOffset = 0.02;
        public double SellOffset = 0.01;

#endif
        private double _money;
        private OrderType _type;
        private OrderType _lastAction;
        private double _startRate;
        private double _startRatePre;
        private double _currentRate;

        private int _dealCount;

        public Order(double initMoney)
        {
            _money = initMoney;
            _type = OrderType.Nothing;
            _lastAction = OrderType.Nothing;
            _startRate = 0;
            _dealCount = 0;
        }
        public int DealCount
        {
            get { return _dealCount; }
        }
        public OrderType LastAction
        {
            get { return _lastAction; }
        }
        public double GetCurrentMoney(double currentRate)
        {
            _currentRate = currentRate;
            return _money + Benifit();
            // return _money;
        }
        public void StartOrder( OrderType type, double currentRate)
        {
            _currentRate = currentRate;
            _lastAction = OrderType.Nothing;
            if (_type == type)
                return;
            else if (_type != type && (_type != OrderType.Nothing))
                CloseOrder(currentRate);
            _dealCount++;
            _type = type;
            if (_type == OrderType.Buy)
            {
                _startRatePre = _startRate;
                _startRate = Ask();
                _lastAction = OrderType.Buy;
            }
            else
            {
                _startRatePre = _startRate;
                _startRate = Bid();
                _lastAction = OrderType.Sell;
            }
        }
        public void CloseOrder(double currentRate)
        {
            double res = Benifit();
            _money += res;
            _type = OrderType.Nothing;
 //           System.Console.WriteLine("M: " + _money.ToString());

        }

        private double Benifit()
        {
            if (_type == OrderType.Nothing)
                return 0;
            else if (_type == OrderType.Buy)
                return (Bid() / _startRate - 1) * _money;
            else // for sell order
                return (_startRate / Ask() - 1) * _money;
        }
        
        private double Bid()
        {
            return _currentRate + SellOffset;
        }
        private double Ask()
        {
            return _currentRate + BuyOffset;
        }
        public enum OrderType
        {
            Nothing,
            Buy,
            Sell,
        }

    }

    class RateMarketAgent
    {
        public int index = 0;
        public double InitMoney = 10000;

        private BasicDataBlock _dataBlock;
        private Order _order;
        private long _step;
        private RateMarketAgentData _stateData = new RateMarketAgentData();

        public long Step
        {
            get
            {
                return _step;
            }
        }

        public bool IsEnd { private set; get; }

        public double CurrentRateValue { get { return _dataBlock.GetRate(index); } }
        public double CurrentValue
        {
            get
            {
                return _order.GetCurrentMoney(_currentRate);
            }
        }
        public MarketActions LastAction
        {
            get
            {
                switch(_order.LastAction)
                {
                    case Order.OrderType.Nothing:
                        return MarketActions.Nothing;
                    case Order.OrderType.Sell:
                        return MarketActions.Sell;
                    case Order.OrderType.Buy:
                        return MarketActions.Buy;
                    default:
                        return MarketActions.Nothing;
                }
            }
        }
        public int DealCount
        {
            get { return _order.DealCount; }
        }
        public RateMarketAgent(BasicDataBlock dataBlock)
        {
            _stateData = new RateMarketAgentData();
            _dataBlock = dataBlock.Clone() ;
            _order = new Order(InitMoney);
            _stateData.RateDataArray = new double[_dataBlock.DataBlockLength];

            Reset();
        }

        public RateMarketAgentData Reset()
        {
            _step = 1;
            _order = new Order(InitMoney);
            index = 0;
            IsEnd = false;

            GetArrayValue(_stateData.RateDataArray, index);
            _stateData.Reward = 0;
            return _stateData;
        }

        public bool Next()
        {
            if (IsEnd == true)
                return false;
            if((index+1) >=  _dataBlock.Length)
            {
                IsEnd = true;
                return false;
            }

            index++;
            _step++;
            return true;
        }
        public RateMarketAgentData TakeAction(MarketActions action)
        {
            switch (action)
            {
                case MarketActions.Buy:
                    _order.StartOrder(Order.OrderType.Buy, _currentRate);
                    break;
                case MarketActions.Sell:
                    _order.StartOrder(Order.OrderType.Sell, _currentRate);
                    break;
                case MarketActions.Nothing:
                    break;
                default:
                    break;
            }
            _stateData.Reward = (CurrentValue - InitMoney) / InitMoney;
            GetArrayValue(_stateData.RateDataArray, index);
            return _stateData;
        }

        private double _currentRate
        {
            get { return _dataBlock.GetRate(index); }
        }
        
        private void GetArrayValue(double[] buffer, int offset)
        {
            _dataBlock.Copy(buffer, offset);

            /*
            // adj to same end point
            */
            // DataNormallizer n = new DataNormallizer();
            // n.DataValueAdjust(buffer, 1.0, 0.0);
                // double[] res = new double[length];
                // Array.Copy(_dataArray, index, res, 0, length);

            // DataAdj(buffer);
            // DataAdj_Offset(buffer);
                return;
        }
        private void DataAdj_Offset(double[] buffer)
        {
            // adj to same end point
            double offset = buffer[buffer.Length-1];
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = buffer[i] - offset + 0.5;
        }
        private void DataAdj(double[] buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] -= buffer[buffer.Length - 1];

            double min, max;
            min = max = buffer[0];
            foreach(double d in buffer)
            {
                if (d > max)
                    max = d;
                if (d < min)
                    min = d;
            }
            double scale = 0.5 / Math.Max(Math.Abs(min), Math.Abs(max));

            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = buffer[i] * scale + 0.5;


        }
        
    }
}
