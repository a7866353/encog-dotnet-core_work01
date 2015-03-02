using MyProject01.Controller;
using MyProject01.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.TestCases
{
    class TestDataBlockCreator
    {
        public int DataLength = 60*24*10/5; // 2 week data
        public int DataBlockLength = 512;
        public RateDataBlock[] DataBlockArr;

        private DataLoader _loader;
        private Random _rand;

        public TestDataBlockCreator()
        {
            _rand = new Random();
        }

        public void Init()
        {
            _loader = new MTDataLoader("USDJPY", DataTimeType.Time5Min);

            int count = _loader.Count / DataLength;
            if (count == 0)
                count = 1;

            DataBlockArr = new RateDataBlock[count];
            for(int i=0;i<count-1;i++)
            {
                DataBlockArr[i] = _loader.CreateDataBlock(i * DataLength, DataLength, DataBlockLength);
            }
            // add last block
            DataBlockArr[count - 1] = _loader.CreateDataBlock(DataLength * (count - 1), _loader.Count - (DataLength * (count - 1)), DataBlockLength);
        }

        public RateDataBlock GetNextBlock()
        {
            if (_loader == null)
                return null;
            return DataBlockArr[_rand.Next(DataBlockArr.Length)];

        }

    }


    class RateMarketNEATBatchTest : BasicTestCase
    {
        public string TestName = "DefaultTestCase00";
        public int TestCaseCount = 4;

        private TestDataBlockCreator _testDataBlockContainer;
#if true
        public int IterationCountPerTest = 50;
        public double testDataRate = 0.75;
        public int dataBlockLength = 1000;
        public int populationNum = 100;
        public int TestDataLength = 5000;

#else
        public int IterationCountPerTest = 100;
        public double testDataRate = 0.75;
        public int populationNum = 1000;
        public int dataBlockLength = 60 * 24 * 10 / 5;
        public int TestDataLength = 60 * 24 * 10 / 5 * 12;
#endif
        private BatchParam[] _params = new BatchParam[]
        {
            new BatchParam(){ startIndex = 0, trainLength = 10, totalLength = 100, blockLength = 30},
        };

        public override void RunTest()
        {
            _testDataBlockContainer = new TestDataBlockCreator();
            _testDataBlockContainer.DataLength = TestDataLength;
            _testDataBlockContainer.DataBlockLength = dataBlockLength;
            _testDataBlockContainer.Init();





            // init controller
            string controllerName = TestName;
            NetworkController controller = NetworkController.Open(controllerName);
            if (controller.InputVectorLength == -1)
            {
                controller.InputVectorLength = dataBlockLength;
                controller.OutputVectorLength = 3;
                controller.PopulationNumeber = populationNum;
            }
            else
            {
                if (controller.InputVectorLength != dataBlockLength || controller.OutputVectorLength != 3)
                {
                    controller = NetworkController.Open(controllerName, true);
                    controller.InputVectorLength = dataBlockLength;
                    controller.OutputVectorLength = 3;
                    controller.PopulationNumeber = populationNum;
                }
            }

            // init test data
            NEATTrainer _train;
            _train = new NEATTrainer();
            _train.TestName = TestName;
            _train.Controller = controller;
            foreach(RateDataBlock block in _testDataBlockContainer.DataBlockArr)
            {
                _train.DataList.Add(new TrainingData(block, (int)(block.Length * testDataRate)));
            }

            _train.IterationCount = IterationCountPerTest;
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
