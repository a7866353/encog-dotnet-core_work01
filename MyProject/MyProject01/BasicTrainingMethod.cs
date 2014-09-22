using Encog.ML.Data;
using Encog.Neural.Networks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyProject01
{
    abstract class BasicTrainingMethod
    {
        public double errorLimit = 0.1;
        public int maxTryCount = 1000;
        public abstract double TrainNetwork( BasicNetwork network, IMLDataSet trainingSet );
    }
}
