using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.TestCases
{
    class RateMarketNEATBatchTest : BasicTestCase
    {
        public string TestCaseName = "DefaultTestCase00";
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
