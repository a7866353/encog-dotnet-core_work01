using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyProject01.Agent;
using MyProject01.Networks;
using MyProject01.TrainingMethods;
using MyProject01.TestParameters;
using MyProject01.Test;
using MyProject01.Util;


namespace MyProject01.Reinforcement
{
    class QLearn : IRateMarketUser
    {
        public double _totalError;
        public int _totalCount;
        private RateMarketAgentData _previousState;
        private double[] _previousOutput;
        private int _previousOutputSelete;
        public MyNet network;
        private Random rand;

        private int _greedRate = 95;
        private double _discountRate = 0.80;
        private double _scaleRate = 1000;
        private double _learnRate = 0.7;
        // Paramters
        private int _inputLength;
        private int _outputLength;

        private double[] _inputDataArray;
        private double[] _outputDataArray;

        public QLearn(MyNet network)
        {
            rand = new Random();
            _previousState = null;
            _previousOutput = null;
            _previousOutputSelete = -1;

            this._inputLength = network.parm.InputSize; ;
            _inputDataArray = new double[_inputLength];

            _outputLength = 3;

            _totalError = 0;
            _totalCount = 0;

            this.network = network;
        }

        MarketActions IRateMarketUser.Determine(RateMarketAgentData state)
        {
            int actionIndex;
            int greedActionIndex;

            // Calcute Q Value
            double[] actionQValueArr = network.Compute(state.RateDataArray);

            // Find greed action
            double maxValue = actionQValueArr[0];
            greedActionIndex = 0;
            for (int i = 1; i < actionQValueArr.Length; i++)
            {
                if (actionQValueArr[i] <= maxValue)
                    continue;
                maxValue = actionQValueArr[i];
                greedActionIndex = i;
            }

            // Choose a action.
            if (rand.Next(100) < _greedRate)
            {
                // Greed action
                actionIndex = greedActionIndex;
            }
            else
            {
                // Explorer action
                actionIndex = rand.Next(actionQValueArr.Length);
            }

            // Learn
            if( _previousState != null)
            {
                double error;
                _previousOutput[_previousOutputSelete] = ((1 - _learnRate) * _previousOutput[_previousOutputSelete]) + _learnRate * (state.Reward + _discountRate * actionQValueArr[greedActionIndex]);
                error = network.Training(_previousState.RateDataArray, _previousOutput);
                // LogFile.WriteLine(error.ToString("G"));
                _totalError += error;
                _totalCount++;
            }

            _previousState = state;
            _previousOutput = actionQValueArr;
            _previousOutputSelete = actionIndex;

            switch(actionIndex)
            {
                case 0:
                    return MarketActions.Buy;
                case 1:
                    return MarketActions.Nothing;
                case 2:
                    return MarketActions.Sell;
                default: 
                    return MarketActions.Nothing;
            }
        }

        double IRateMarketUser.TotalErrorRate
        {
            get
            {
                double errorSum = 0;
                foreach (double d in _previousOutput)
                    errorSum += Math.Abs(d);
                errorSum /= _previousOutput.Length;
                return _totalError / errorSum / _totalCount;
            }
            set
            {
                _totalError = 0; _totalCount = 0;
            }
        }

        public void EpsodeEnd()
        {
            network.SaveNetwork();
            _totalError = 0; _totalCount = 0;
        }
    }
}
