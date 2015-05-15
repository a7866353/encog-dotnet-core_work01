using MyProject01.Factorys.ControllerFactorys;
using MyProject01.Factorys.PopulationFactorys;
using MyProject01.Factorys.TrainerFactorys;
using MyProject01.Factorys.TrainingDataFactorys;
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
        private FirstTrainerFactory trainerFactory;
        private NormalPopulationFactory popFactory;

        public NormalRateMarketTestCase()
        {
            controllerFactory = new NEATStateKeepControllerFactory();
            controllerFactory.InputLength = 32;
            _descList.Add(controllerFactory);

            dataFactory = new OldRateTrainingDataFactory();
            dataFactory.DataBlockLength = controllerFactory.InputLength;
            _descList.Add(dataFactory);

            trainerFactory = new FirstTrainerFactory();
            _descList.Add(trainerFactory);

            popFactory = new NormalPopulationFactory();
            popFactory.PopulationNumber = 100;
            _descList.Add(popFactory);
        }
        
        protected override void Init()
        {

            trainerFactory.PopulationFacotry = popFactory;

            controllerFactory.Name = TestName;
            trainerFactory.Controller = controllerFactory.Get();

            trainerFactory.TrainingData = dataFactory.Get();
            trainerFactory.TestCaseName = TestName;

            _train = trainerFactory.Get();
        }
    }

    class NormalRateMarketTestCase_BigPop : BasicRateMarketTestCase
    {
        private NEATStateKeepControllerFactory controllerFactory = new NEATStateKeepControllerFactory();
        private OldRateTrainingDataFactory dataFactory = new OldRateTrainingDataFactory();
        private FirstTrainerFactory trainerFactory;
        private NormalPopulationFactory popFactory;


        public NormalRateMarketTestCase_BigPop()
        {
            controllerFactory = new NEATStateKeepControllerFactory();
            controllerFactory.InputLength = 32;
            _descList.Add(controllerFactory);

            dataFactory = new OldRateTrainingDataFactory();
            dataFactory.DataBlockLength = controllerFactory.InputLength;
            _descList.Add(dataFactory);

            trainerFactory = new FirstTrainerFactory();
            _descList.Add(trainerFactory);

            popFactory = new NormalPopulationFactory();
            popFactory.PopulationNumber = 1000;
            _descList.Add(popFactory);
        }

        protected override void Init()
        {

            trainerFactory.PopulationFacotry = popFactory;

            controllerFactory.Name = TestName;
            trainerFactory.Controller = controllerFactory.Get();

            trainerFactory.TrainingData = dataFactory.Get();
            trainerFactory.TestCaseName = TestName;

            _train = trainerFactory.Get();
        }
    }
}
