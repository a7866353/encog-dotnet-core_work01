using MyProject01.Test;
using MyProject01.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyProject01.Reinforcement;

namespace MyProject01.Agent
{
    enum MarketActions
    {
        Nothing = 0,
        Buy,
        Sell,
    };
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

    class RateMarketAgent
    {
        public int index = 0;
        public double InitMoney = 10000;

        private double[] _dataArray;
        private int _dataBlockLength;
        private double _money;
        private double _mountInHand;
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

        public double CurrentRateValue { get { return _dataArray[index]; } }
        public double CurrentValue
        {
            get
            {
                double rate = _dataArray[index];
                double res = _money;
                if (_mountInHand != 0)
                    res += _mountInHand / rate;

                return res;
            }
        }

        public RateMarketAgent(double[] dataArray, int dataBlockLength)
        {
            _stateData = new RateMarketAgentData();
            this._dataArray = dataArray;
            this._dataBlockLength = dataBlockLength;
            Reset();
        }

        public RateMarketAgentData Reset()
        {
            _step = 0;
            _money = InitMoney;
            index = _dataBlockLength - 1;
            _mountInHand = 0;
            _money = InitMoney;
            IsEnd = false;

            
            _stateData.RateDataArray = GetArrayValue(index - _dataBlockLength + 1, _dataBlockLength);
            _stateData.Reward = 0;
            return _stateData;
        }

        public bool Next()
        {
            if (IsEnd == true)
                return false;
            if((index+1) > _dataArray.Length - 1)
            {
                IsEnd = true;
                return false;
            }

            index++;
            return true;
        }
        public RateMarketAgentData TakeAction(MarketActions action)
        {
            switch (action)
            {
                case MarketActions.Buy:
                    Buy();
                    break;
                case MarketActions.Sell:
                    Sell();
                    break;
                case MarketActions.Nothing:
                    break;
                default:
                    break;
            }
            _stateData.Reward = (CurrentValue - InitMoney) / InitMoney;
            _stateData.RateDataArray = GetArrayValue(index - _dataBlockLength + 1, _dataBlockLength);
            return _stateData;
        }


        private void Buy()
        {
            double rate = _dataArray[index];
            if (_money <= 0)
                return;

            _mountInHand += _money * rate;
            _money = 0;

        }
        private void Sell()
        {
            double rate = _dataArray[index];
            if (_mountInHand <= 0)
                return;
            _money += _mountInHand / rate;
            _mountInHand = 0;
        }
        
        private double[] GetArrayValue(int index, int length)
        {
            length = Math.Min(length, _dataArray.Length - index);
            double[] res = new double[length];

            Array.Copy(_dataArray, index, res, 0, length);
            return res;
        }
        
    }
}
