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
    }

    class RateMarketAgent
    {
        private const string _dataFile = "data.csv";

        private double initMoney = 10000;
        private double scaleRate = 10000;
        private double money;
        private int dataLength = 30;

        private DataLoader dataLoader;
        private int index = 0;
        private double mountInHand;
        private IRateMarketUser user;

        public RateMarketAgent(MyNet network)
        {
            money = initMoney;
            this.user = new QLearn(network);
            dataLoader = new DataLoader(_dataFile);
        }

        public void Run()
        {
            long testStep = 1;

            MarketActions action;
            RateMarketAgentData inputData = new RateMarketAgentData();
            while (true)
            {
                index = dataLength - 1;
                mountInHand = 0;
                money = initMoney;
                user.TotalErrorRate = 0;
                while (true)
                {
                    if (dataLoader[index].Value > 0)
                    {
                        // Calcute
                        inputData.RateDataArray = dataLoader.GetArr(index - dataLength+1, dataLength);
                        inputData.Reward = (CurrentValue() - initMoney) / initMoney / scaleRate;
                        action = user.Determine(inputData);

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

                        // LogFile.WriteLine("[" + index.ToString("D6") + "]" + "Current Value: " + CurrentValue().ToString());
                    }
                    if (index >= dataLoader.Count-1)
                        break;// end
                    index++;
                }
                LogFile.WriteLine("[" + testStep.ToString("D6") + "]" + "Current Value: " + CurrentValue().ToString() + "\tErrorRate:" + user.TotalErrorRate.ToString());
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
