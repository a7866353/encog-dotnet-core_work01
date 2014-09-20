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
        public double[] rateDataArray;
    }

    interface IRateMarketUser
    {
        MarketActions Determine(RateMarketAgentData inputData);
    }

    class RateMarketAgent
    {
        private const string _dataFile = "data.csv";

        private double money = 10000;
        private int dataLength = 30;

        private DataLoader dataLoader;
        private int index = 0;
        private double mountInHand;

        
        

        public void Run()
        {
            long testStep = 1;
            dataLoader = new DataLoader(_dataFile);
            index = dataLength - 1;
            mountInHand = 0;

            MarketActions action;
            IRateMarketUser user = new QLearn();
            RateMarketAgentData inputData = new RateMarketAgentData();
            while(true)
            {
                if (index >= dataLoader.Count)
                    break;// end

                // Calcute
                inputData.rateDataArray = dataLoader.GetArr(index, dataLength);
                action = user.Determine(inputData);

                switch(action)
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

                LogFile.WriteLine("[" + testStep.ToString("D6") + "]" + "Current Value: " + CurrentValue().ToString());
                index++;
                testStep++;
            }

        }


        private void Buy()
        {
            double rate = dataLoader[index].Value;
            if (money <= 0)
                return;

            mountInHand += money * rate;
            money = 0;

        }
        private void Sell()
        {
            double rate = dataLoader[index].Value;
            if (mountInHand <= 0)
                return;
            money += mountInHand / rate;
            mountInHand = 0;
        }
        
        private double CurrentValue()
        {
            double rate = dataLoader[index].Value;
            double res = money;
            if (mountInHand != 0)
                res += mountInHand / rate;

            return res;
        }
        
    }
}
