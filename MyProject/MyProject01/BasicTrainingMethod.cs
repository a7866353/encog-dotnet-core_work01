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
        public abstract double TrainNetwork( BasicNetwork network, IMLDataSet trainingSet );
    }
}
