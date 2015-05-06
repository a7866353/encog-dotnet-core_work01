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

        public NormalRateMarketTestCase()
        {
            TestName = "5Min_512_100_Test";
        }
        
        protected override void Init()
        {

            FirstTrainerFactory trainerFactory = new FirstTrainerFactory();
            trainerFactory.TestCaseName = TestName;

            NEATExchangeControllerFactory controllerFactory = new NEATExchangeControllerFactory();
            controllerFactory.InputLength = 32;
            trainerFactory.Controller = controllerFactory.Get();

            OldRateTrainingDataFactory dataFactory = new OldRateTrainingDataFactory();
            dataFactory.DataBlockLength = controllerFactory.InputLength;
            trainerFactory.TrainingData = dataFactory.Get();

            NormalPopulationFactory popFactory = new NormalPopulationFactory();
            popFactory.PopulationNumber = 100;
            trainerFactory.PopulationFacotry = popFactory;

            _train = trainerFactory.GetTrainer();
        }
    }
}
