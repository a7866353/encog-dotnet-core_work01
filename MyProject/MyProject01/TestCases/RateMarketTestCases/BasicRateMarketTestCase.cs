using MyProject01.ControllerFactorys;
using MyProject01.Controller.TrainerFactorys;
using MyProject01.TrainerFactorys;
using MyProject01.TrainingDataFactorys;
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
        /*
        protected NetworkController controller;
        protected TrainingData trainData;
        protected Trainer _train;
        */

        protected BasicControllerFactory _controllerFactory;
        protected BasicTrainerFactory _trainerFactory;
        protected BasicTrainingDataFactory _dataFactory;

        public override void RunTest()
        {
            _train = _trainerFactory.Get();
            _train.DataList.Add(trainData);

            // start trainning
            _train.TestName = TestName;
            _train.Controller = controller;
            // _train.IterationCount = 0
            _train._decisionCtrl = controller.GetDecisionController();
            _train.RunTestCase();

        }
    }
}
