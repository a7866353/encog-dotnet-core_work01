using Encog.ML;
using Encog.ML.Data;
using Encog.ML.Data.Basic;
using Encog.ML.EA.Train;
using Encog.ML.Train.Strategy;
using Encog.Neural.NEAT;
using Encog.Neural.Networks.Training;
using Encog.Util;
using Encog.Util.Banchmark;
using Encog.Util.Simple;
using MyProject01.Agent;
using MyProject01.DAO;
using MyProject01.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyProject01.DAO;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using MyProject01.Controller;

namespace MyProject01.TestCases
{

    class RateMarketNEATTest : BasicTestCase
    {
        public string TestName = "DefaultTest000";

        NEATTrainer _train;
        public override void RunTest()
        {
#if true
            double testDataRate = 0.75;
            int dataBlockLength = 300;
            int populationNum = 50;
#else
            double testDataRate = 0.75;
            int dataBlockLength = 60 * 24 * 10 / 5;
            int populationNum = 1000;

#endif

            // init controller
            string controllerName = TestName;
            NEATController controller = NEATController.Open(controllerName);
            if (controller.InputVectorLength == -1)
            {
                controller.PopulationNumeber = populationNum;
                controller.InputVectorLength = dataBlockLength;
                controller.OutputVectorLength = 3;
            }
            else
            {
                if (controller.InputVectorLength != dataBlockLength || controller.OutputVectorLength != 3)
                {
                    controller = NEATController.Open(controllerName, true);
                    controller.PopulationNumeber = populationNum;
                    controller.InputVectorLength = dataBlockLength;
                    controller.OutputVectorLength = 3;
                }
            }

            // init test data
            DataLoader loader = new MTDataLoader("USDJPY", DataTimeType.Time5Min);
            if(controller.DataScale == 0)
            {
                controller.DataScale = loader.Scale;
                controller.DataOffset = loader.Offset;
            }
            else
            {
                loader.Normallize(controller.DataOffset, controller.DataScale);
            }
            DataBlock testBlock = loader.CreateDataBlock(0, loader.Count);
            _train = new NEATTrainer();
            _train.DataList.Add(new TrainingData(testBlock, (int)(testBlock.Length * testDataRate) ));

            // start trainning
            _train.TestName = TestName;
            _train.Controller = controller;
            _train.IterationCount = 0;
            _train.RunTestCase();
        }

    }
}
