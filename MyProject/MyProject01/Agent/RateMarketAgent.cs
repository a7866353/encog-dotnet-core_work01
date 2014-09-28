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
        Nothing,
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

        static private DataLoader _dataLoader;
        static private const string _dataFile = "data.csv";
        static RateMarketAgent()
        {
            _dataLoader = new DataLoader(_dataFile);

        }



        public double InitMoney = 10000;
        private double money;
        private int _dataLength = 30;
        private int _testLength = 100;

        private int index = 0;
        private double mountInHand;
        private IRateMarketUser user;
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

        public RateMarketAgent(IRateMarketUser user)
        {
            this.user = user;
            _testLength = Math.Min(_testLength, _dataLoader.Count);
            _stateData = new RateMarketAgentData();
            Reset();
        }

        public RateMarketAgentData Reset()
        {
            _step = 0;
            money = InitMoney;
            index = _dataLength - 1;
            mountInHand = 0;
            money = InitMoney;
            user.TotalErrorRate = 0;
            IsEnd = false;

            
            _stateData.RateDataArray = _dataLoader.GetArr(index - _dataLength + 1, _dataLength);
            _stateData.Reward = 0;
            return _stateData;
        }

        public bool Next()
        {
            if (IsEnd == true)
                return false;
            if((index+1) > _testLength - 1)
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
            _stateData.Reward = (CurrentValue() - InitMoney) / InitMoney;
            _stateData.RateDataArray = _dataLoader.GetArr(index - _dataLength + 1, _dataLength);
            return _stateData;
        }


        private void Buy()
        {
            double rate = _dataLoader[index].Value;
            if (money <= 0)
                return;

            mountInHand += money * rate;
            money = 0;

        }
        private void Sell()
        {
            double rate = _dataLoader[index].Value;
            if (mountInHand <= 0)
                return;
            money += mountInHand / rate;
            mountInHand = 0;
        }
        
        public double CurrentValue()
        {
            double rate = _dataLoader[index].Value;
            double res = money;
            if (mountInHand != 0)
                res += mountInHand / rate;

            return res;
        }
        
    }
}
