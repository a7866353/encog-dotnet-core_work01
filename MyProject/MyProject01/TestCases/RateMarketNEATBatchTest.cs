using MyProject01.Controller;
using MyProject01.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.TestCases
{
    class RateMarketNEATBatchTest : BasicTestCase
    {
        public string TestName = "DefaultTestCase00";
        public int TestCaseCount = 4;


        private BatchParam[] _params = new BatchParam[]
        {
            new BatchParam(){ startIndex = 0, trainLength = 10, totalLength = 100, blockLength = 30},
        };

        public override void RunTest()
        {
            int blockLength = 30;
            double dataLengthPer = 0.4;
            double trainPer = 0.5;
            /*
            int totalDataCount = RateMarketNEATTest._dataLoader.Count;
            int dataLength = (int)(totalDataCount * dataLengthPer);
            int trainLength = (int)(dataLength * trainPer);
            int startIndexInc = (totalDataCount - dataLength) / TestCaseCount;
            List<RateMarketNEATTest> testList = new List<RateMarketNEATTest>();
            for (int i = 0; i < TestCaseCount; i++)
            {
                RateMarketNEATTest test = new RateMarketNEATTest();
                test.TestName = TestCaseName + "_" + i.ToString("D2");
                test.SetDataLength(startIndexInc * i, trainLength, dataLength, blockLength);
                testList.Add(test);
            }

            Parallel.ForEach(testList, currentTest => currentTest.RunTest());
            */
        }

        private void StartTest()
        {
            double testDataRate = 0.75;
            int dataBlockLength = 300;
            int populationNum = 50;


            // init controller
            string controllerName = TestName;
            NEATController controller = NEATController.Open(controllerName);
            if (controller.InputVectorLength == -1)
            {
                controller.PopulationNumeber = populationNum;
            }
            else
            {
                if (controller.InputVectorLength != dataBlockLength || controller.OutputVectorLength != 3)
                {
                    controller = NEATController.Open(controllerName, true);
                    controller.PopulationNumeber = populationNum;
                }
            }

            // init test data
            DataLoader loader = new MTDataLoader("USDJPY", DataTimeType.Time5Min);
            DataBlock testBlock = loader.CreateDataBlock(0, loader.Count);
            _train = new NEATTrainer();
            _train.SetDataLength(testBlock, (int)(testBlock.Length * testDataRate));

            // start trainning
            _train.TestName = TestName;
            _train.Controller = controller;
            _train.IterationCount = 0;
            _train.RunTestCase();

        }

        class BatchParam
        {
            public int startIndex;
            public int trainLength;
            public int totalLength;
            public int blockLength;
        }
    }
}
