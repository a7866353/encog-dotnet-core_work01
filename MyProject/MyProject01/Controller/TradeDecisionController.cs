using Encog.ML;
using Encog.ML.Data;
using Encog.ML.Data.Basic;
using MyProject01.Util.DllTools;
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
    public enum InputDataFormaterType
    {
        None = 0,
        FWT = 1,
        Rate = 2,
    }
    class InputDataFormaterFactor
    {
        static public IInputDataFormater Create(InputDataFormaterType type, int inputDataLength)
        {
            IInputDataFormater result;
            switch (type)
            {
                case InputDataFormaterType.FWT:
                    result = new FWTFormater(inputDataLength);
                    break;
                case InputDataFormaterType.Rate:
                    result = new RateDataFormater(inputDataLength);
                    break;
                default:
                    throw (new Exception("Input parm error!"));
            }

            return result;
        }
    }
    class FWTFormater : IInputDataFormater
    {
        private int _inputDataLength;
        public double[] Buffer;
        private double[] _tempBuffer;

        public FWTFormater(int inputDataLength)
        {
            _inputDataLength = inputDataLength;
            Buffer = new double[_inputDataLength];
            _tempBuffer = new double[_inputDataLength];
        }
        public BasicMLData Convert(double[] rateDataArray)
        {
            if (Buffer.Length != rateDataArray.Length)
            {
                throw (new Exception("Input Param Error!"));
            }

            DllTools.FTW_2(rateDataArray, Buffer, _tempBuffer);


            MyProject01.Util.DataNormallizer adj = new MyProject01.Util.DataNormallizer();
            Buffer[0] = Buffer[1];
            adj.Set(Buffer, 0, Buffer.Length);
            adj.DataValueAdjust(-0.01, 0.01);

            return new BasicMLData(Buffer, false);
        }


        public int NetworkInputLength
        {
            get { return _inputDataLength; }
        }
    }

    class RateDataFormater : IInputDataFormater
    {
        private int _inputDataLength;
        public RateDataFormater(int inputDataLength)
        {
            _inputDataLength = inputDataLength;
        }
        public BasicMLData Convert(double[] rateDataArray)
        {
            return new BasicMLData(rateDataArray, false);
        }
        public int NetworkInputLength
        {
            get { return _inputDataLength; }
        }
    }

    public enum OutputDataConvertorType
    {
        None = 0,
        StateKeep = 1,
        Switch = 2,
    }
    class OutputDataConvertorFactory
    {
        static public IOutputDataConvertor Create(OutputDataConvertorType type)
        {
            IOutputDataConvertor result;
            switch (type)
            {
                case OutputDataConvertorType.Switch:
                    result = new TradeStateResultConvertor();
                    break;
                default:
                    throw (new Exception("Input parm error!"));
            }

            return result;
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


        public int NetworkOutputLength
        {
            get { return 3; }
        }
    }
    public class TradeDecisionController
    {
        public IInputDataFormater _inputFormater;
        public IOutputDataConvertor _outputConvertor;
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
