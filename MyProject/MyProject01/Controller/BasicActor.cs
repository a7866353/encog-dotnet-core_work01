using Encog.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.Controller
{
    public interface IActor
    {
        MarketActions GetAction(IMLData output);
        IActor Clone();
        int DataLength { get; }
    }
    [Serializable]
    class BasicActor : IActor
    {
        private MarketActions _lastAction = MarketActions.Nothing;
        public MarketActions GetAction(IMLData output)
        {
            MarketActions currentAction;
            // Choose an action
            int maxActionIndex = 0;
            for (int i = 1; i < output.Count; i++)
            {
                if (output[maxActionIndex] < output[i])
                    maxActionIndex = i;
            }

            // Do action
            switch (maxActionIndex)
            {
                    /*
                case 0:
                    if (_lastAction == MarketActions.Buy)
                        currentAction = MarketActions.Sell;
                    else if (_lastAction == MarketActions.Sell)
                        currentAction = MarketActions.Buy;
                    else
                        currentAction = MarketActions.Nothing;
                    break;
                     */
                case 0:
                    currentAction = MarketActions.Close;
                    break;
                case 1:
                    currentAction = MarketActions.Buy;
                    break;
                case 2:
                    currentAction = MarketActions.Sell;
                    break;
                default:
                    currentAction = MarketActions.Nothing;
                    break;
            }

            _lastAction = currentAction;
            return currentAction;
        }

        public int DataLength
        {
            get
            {
                return 3;
            }
        }


        public IActor Clone()
        {
            return new BasicActor();
        }
    }
}
