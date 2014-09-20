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
        private RateMarketAgentData _previousState;
        private double[] _previousOutput;
        private int _previousOutputSelete;
        private MyNet network;
        private Random rand;

        private int _greedRate = 95;
        private double _discountRate = 0.80;
        private double _scaleRate = 1000;
        // Paramters
        private int _inputLength;
        private int _outputLength;

        private double[] _inputDataArray;
        private double[] _outputDataArray;

        public QLearn(int inputLengh)
        {
            rand = new Random();
            _previousState = null;
            _previousOutput = null;
            _previousOutputSelete = -1;

            this._inputLength = inputLengh;
            _inputDataArray = new double[_inputLength];

            _outputLength = 3;


            // Init network
            NetworkTestParameter parm = new NetworkTestParameter("QLearn", 0.1, 2, 10000);
            // network = new MyNet(new FeedForwardNet(), new ResilientPropagationTraining(), parm);
            network = new MyNet(new FeedForwardNet(), new BackpropagationTraining(), parm);
            network.Init(_inputLength, _outputLength);
        }

        public MarketActions Determine(RateMarketAgentData state)
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
                _previousOutput[_previousOutputSelete] = state.Reward + _discountRate * actionQValueArr[greedActionIndex];
                error = network.Training(_previousState.RateDataArray, _previousOutput);
                LogFile.WriteLine(error.ToString("G"));
                if (error > 0.1)
                    error = error;
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
    }
}
