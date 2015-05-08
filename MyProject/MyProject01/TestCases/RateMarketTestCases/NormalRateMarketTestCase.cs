using MyProject01.Controller.TrainerFactorys;
using MyProject01.ControllerFactorys;
using MyProject01.PopulationFactorys;
using MyProject01.TrainingDataFactorys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.TestCases.RateMarketTestCases
{
    class NormalRateMarketTestCase : BasicRateMarketTestCase
    {
        private NEATStateKeepControllerFactory controllerFactory = new NEATStateKeepControllerFactory();
        private OldRateTrainingDataFactory dataFactory = new OldRateTrainingDataFactory();

        public NormalRateMarketTestCase()
        {
            controllerFactory = new NEATStateKeepControllerFactory();
            controllerFactory.InputLength = 32;

            dataFactory = new OldRateTrainingDataFactory();
            dataFactory.DataBlockLength = controllerFactory.InputLength;

            TestName = controllerFactory.GetDesc();
            TestName += "_" + dataFactory.GetDesc();
            TestName += "_" + DateTime.Now.ToString();
        }
        
        protected override void Init()
        {
            FirstTrainerFactory trainerFactory = new FirstTrainerFactory();

            NormalPopulationFactory popFactory = new NormalPopulationFactory();
            popFactory.PopulationNumber = 100;
            trainerFactory.PopulationFacotry = popFactory;

            controllerFactory.Name = TestName;
            trainerFactory.Controller = controllerFactory.Get();

            trainerFactory.TrainingData = dataFactory.Get();
            trainerFactory.TestCaseName = TestName;

            _train = trainerFactory.GetTrainer();
        }
    }
}
