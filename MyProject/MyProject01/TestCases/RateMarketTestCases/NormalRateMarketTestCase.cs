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
        protected override void Init()
        {
            FirstTrainerFactory trainerFactory = new FirstTrainerFactory();
            trainerFactory.TestCaseName = "NEAT_D512_5Min";

            NEATExchangeControllerFactory controllerFactory = new NEATExchangeControllerFactory();
            controllerFactory.InputLength = 512;
            trainerFactory.Controller = controllerFactory.Get();

            OldRateTrainingDataFactory dataFactory = new OldRateTrainingDataFactory();
            dataFactory.DataBlockLength = controllerFactory.InputLength;
            trainerFactory.TrainingData = dataFactory.Get();

            NormalPopulationFactory popFactory = new NormalPopulationFactory();
            popFactory.PopulationNumber = 100;
            trainerFactory.PopulationFacotry = popFactory;


        }
    }
}
