using Encog.ML.Data;
using Encog.Neural.Networks.Training.Propagation.Resilient;
using Encog.Util.Banchmark;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyProject01.TrainingMethods
{
    class ResilientPropagationTraining : BasicTrainingMethod
    {

        public override double TrainNetwork(Encog.Neural.Networks.BasicNetwork network, Encog.ML.Data.IMLDataSet trainingSet)
        {
            IMLDataSet training = RandomTrainingFactory.Generate(1000, 500,
                                                     trainingSet.InputSize, trainingSet.IdealSize, -1, 1);

            var rprop = new ResilientPropagation(network, training);
            rprop.ThreadCount = 16;
            for (int i = 0; i < 5; i++)
            {
                rprop.Iteration();
            }

            return rprop.Error;

        }
    }
}
