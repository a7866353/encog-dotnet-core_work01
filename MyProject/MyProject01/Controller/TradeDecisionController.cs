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
    class FWTFormater : IInputDataFormater
    {
        private double[] _buffer;
        private double[] _tempBuffer;

        public BasicMLData Convert(double[] rateDataArray)
        {
            if (_buffer == null || _buffer.Length != rateDataArray.Length)
            {
                _buffer = new double[rateDataArray.Length];
                _tempBuffer = new double[rateDataArray.Length];
            }

            DllTools.FTW_2(rateDataArray, _buffer, _tempBuffer);


            MyProject01.Util.DataNormallizer adj = new MyProject01.Util.DataNormallizer();
            _buffer[0] = _buffer[1];
            adj.Set(_buffer, 0, _buffer.Length);
            adj.DataValueAdjust(-0.01, 0.01);

            return new BasicMLData(_buffer, false);
        }
    }

    class RateDataFormater : IInputDataFormater
    {
        private double[] _buffer;
        private double[] _tempBuffer;

        public BasicMLData Convert(double[] rateDataArray)
        {
            return new BasicMLData(rateDataArray, false);
        }
    }

    class TradeStateResultConvertor : IOutputDataConvertor
    {
        public MarketActions Convert(IMLData output)
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
                case 0:
                    currentAction = MarketActions.Nothing;
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
            return currentAction;
        }
    }
    public class TradeDecisionController
    {
        private IInputDataFormater _inputFormater;
        private IOutputDataConvertor _outputConvertor;
        public IMLRegression BestNetwork;

        public TradeDecisionController()
        {
            _inputFormater = new FWTFormater();
            _outputConvertor = new TradeStateResultConvertor();
        }

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
