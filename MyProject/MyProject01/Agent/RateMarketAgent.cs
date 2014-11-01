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
        public double[] DataArray;
        public int CaseLength = 30;
        public double InitMoney = 10000;
        public int index = 0;


        private double money;
        private double mountInHand;
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

        public double CurrentRateValue { get { return DataArray[index]; } }

        public RateMarketAgent()
        {
            _stateData = new RateMarketAgentData();
            Reset();
        }

        public RateMarketAgentData Reset()
        {
            _step = 0;
            money = InitMoney;
            index = CaseLength - 1;
            mountInHand = 0;
            money = InitMoney;
            IsEnd = false;


            _stateData.RateDataArray = GetArray(index - CaseLength + 1, CaseLength);
            _stateData.Reward = 0;
            return _stateData;
        }

        public bool Next()
        {
            if (IsEnd == true)
                return false;
            if ((index + 1) > DataArray.Length - 1)
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
            _stateData.RateDataArray = GetArray(index - CaseLength + 1, CaseLength);
            return _stateData;
        }

        public double CurrentValue()
        {
            double rate = DataArray[index];
            double res = money;
            if (mountInHand != 0)
                res += mountInHand / rate;

            return res;
        }


        private void Buy()
        {
            double rate = DataArray[index];
            if (money <= 0)
                return;

            mountInHand += money * rate;
            money = 0;

        }
        private void Sell()
        {
            double rate = DataArray[index];
            if (mountInHand <= 0)
                return;
            money += mountInHand / rate;
            mountInHand = 0;
        }
        
        private double[] GetArray(int index, int length)
        {
            length = Math.Min(length, DataArray.Length - index);
            double[] res = new double[length];
            Array.Copy(DataArray, index, res, 0, length);
            return res;
        }

        
    }
}
