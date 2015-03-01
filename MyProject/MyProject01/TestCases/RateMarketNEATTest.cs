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
using MyProject01.Util.DataObject;

namespace MyProject01.TestCases
{

    class RateMarketNEATTest : BasicTestCase
    {
        public string TestName = "DefaultTest000";
        public double TestDataRate = 0.75;
        public int DataBlockLength = 300;
        public  int PopulationNum = 50;
        public int DataSoreceType = 0;
        public string RateDataControllerName = "test01";
        public bool IsFWT = false;

        NEATTrainer _train;
        public override void RunTest()
        {
            // init controller
            string controllerName = TestName;
            TradeDecisionController controller = TradeDecisionController.Open(controllerName);
            if (controller.InputVectorLength == -1)
            {
                controller.PopulationNumeber = PopulationNum;
                controller.InputVectorLength = DataBlockLength;
                controller.OutputVectorLength = 3;
            }
            else
            {
                if (controller.InputVectorLength != DataBlockLength || controller.OutputVectorLength != 3)
                {
                    controller = TradeDecisionController.Open(controllerName, true);
                    controller.PopulationNumeber = PopulationNum;
                    controller.InputVectorLength = DataBlockLength;
                    controller.OutputVectorLength = 3;
                }
            }
            // init test data
            DataLoader loader;
            if (DataSoreceType == 0)
            {
                loader = new MTDataLoader("USDJPY", DataTimeType.Time5Min);
                // loader.Fillter(new DateTime(2013, 1, 1), DateTime.Now);
            }
            else
            {
                loader = new MTData2Loader(RateDataControllerName);
            }
            if(controller.DataScale == 0)
            {
                controller.DataScale = loader.Scale;
                controller.DataOffset = loader.Offset;
            }
            else
            {
                loader.Normallize(controller.DataOffset, controller.DataScale);
            }
            BasicDataBlock testBlock;
            if( IsFWT == false )
            {
                // testBlock = loader.CreateDataBlock(0, loader.Count, DataBlockLength);
                testBlock = new RateDataBlock(loader, 0, loader.Count, DataBlockLength);
            }
            else
            {
                testBlock = new FWTDataBlock(loader, 0, loader.Count, DataBlockLength);

            }
            _train = new NEATTrainer();
            _train.DataList.Add(new TrainingData(testBlock, (int)(testBlock.Length * TestDataRate) ));

            // start trainning
            _train.TestName = TestName;
            _train.Controller = controller;
            _train.IterationCount = 0;
            _train.RunTestCase();
        }

    }
}
