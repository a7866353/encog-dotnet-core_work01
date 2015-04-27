using MyProject01.Controller;
using MyProject01.Controller.TrainerFactorys;
using MyProject01.Util.DataObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.TestCases.RateMarketTestCases
{
    abstract class BasicRateMarketTestCase : BasicTestCase
    {
        protected NetworkController controller;
        protected TrainingData trainData;
        protected Trainer _train;

        public override void RunTest()
        {
            _train.DataList.Add(trainData);

            // start trainning
            _train.TestName = TestName;
            _train.Controller = controller;
            // _train.IterationCount = 0
            _train.DecisionCtrl = controller.GetDecisionController();
            _train.RunTestCase();

        }
    }
}
