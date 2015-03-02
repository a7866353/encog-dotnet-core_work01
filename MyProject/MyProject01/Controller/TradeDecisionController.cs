using Encog.ML;
using Encog.ML.Data;
using Encog.ML.Data.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.Controller
{
    public enum MarketActions
    {
        Nothing = 0,
        Buy,
        Sell,
        Init,
    };

    public class TradeDecisionController
    {
        private IInputDataFormater _inputFormater;
        private IOutputDataConvertor _outputConvertor;
        public IMLRegression BestNetwork;

        public MarketActions GetAction(double[] input)
        {
            if (BestNetwork == null)
                return MarketActions.Nothing;
            BasicMLData inData = _inputFormater.Convert(input);

            IMLData output = BestNetwork.Compute(inData);
            MarketActions result = _outputConvertor.Convert(output);
            return result;
        }

    }
}
