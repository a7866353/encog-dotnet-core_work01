using Encog.ML.Data;
using Encog.ML.Data.Basic;
using Encog.ML.EA.Train;
using Encog.ML.Train.Strategy;
using Encog.Neural.NEAT;
using Encog.Neural.Networks.Training;
using MyProject01.TestCases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.Controller
{
    class NEATController
    {
        private int _testDataStartIndex;
        private int _trainDataLength;
        private int _testDataLength;
        private int _dataBlockLength;

        private NEATPopulation _population;
        public NEATNetwork BestNetwork;
        
        public void OpenOrNew()
        {

        }

        public double[] Compute(double[] input)
        {
            if (BestNetwork == null)
                return null;
            IMLData output = BestNetwork.Compute(new BasicMLData(input, false));
            double[] outputArr = new double[output.Count];
            for(int i=0; i<outputArr.Length; i++)
                outputArr[i] = output[i];
            return outputArr;
        }

        public NEATPopulation Population
        {
            get
            {
                if (_population == null)
                {
                    _population = new NEATPopulation(_dataBlockLength, 3, 500);
                    _population.Reset();
                    _population.InitialConnectionDensity = 1.0; // not required, but speeds processing.
                }
                return _population;
            }
        }


        
    }
}
