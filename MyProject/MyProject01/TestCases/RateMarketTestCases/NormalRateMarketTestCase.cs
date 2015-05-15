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
        public int PopulationNumber
        {
            set { popFactory.PopulationNumber = value; }
        }

        private NEATStateKeepControllerFactory controllerFactory = new NEATStateKeepControllerFactory();
        private OldRate1DayTrainingDataFactory dataFactory = new OldRate1DayTrainingDataFactory();
        private FirstTrainerFactory trainerFactory;
        private NormalPopulationFactory popFactory;

        public NormalRateMarketTestCase()
        {
            controllerFactory = new NEATStateKeepControllerFactory();
            controllerFactory.InputLength = 32;
            _descList.Add(controllerFactory);

            dataFactory = new OldRate1DayTrainingDataFactory();
            dataFactory.DataBlockLength = controllerFactory.InputLength;
            _descList.Add(dataFactory);

            trainerFactory = new FirstTrainerFactory();
            _descList.Add(trainerFactory);

            popFactory = new NormalPopulationFactory();
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
    class Normal5MinRateMarketTestCase : BasicRateMarketTestCase
    {
        public int PopulationNumber
        {
            set { popFactory.PopulationNumber = value; }
        }

        private NEATStateKeepControllerFactory controllerFactory = new NEATStateKeepControllerFactory();
        private BasicTrainingDataFactory dataFactory = new OldRate1DayTrainingDataFactory();
        private FirstTrainerFactory trainerFactory;
        private NormalPopulationFactory popFactory;


        public Normal5MinRateMarketTestCase()
        {
            controllerFactory = new NEATStateKeepControllerFactory();
            controllerFactory.InputLength = 32;
            _descList.Add(controllerFactory);

            dataFactory = new OldRate5MinTrainingDataFactory();
            dataFactory.DataBlockLength = controllerFactory.InputLength;
            _descList.Add(dataFactory);

            trainerFactory = new FirstTrainerFactory();
            _descList.Add(trainerFactory);

            popFactory = new NormalPopulationFactory();
            _descList.Add(popFactory);
        }

        protected override void Init()
        {

            trainerFactory.PopulationFacotry = popFactory;
            trainerFactory.TestDescription = TestDescription;
            controllerFactory.Name = TestName;
            trainerFactory.Controller = controllerFactory.Get();

            trainerFactory.TrainingData = dataFactory.Get();
            trainerFactory.TestCaseName = TestName;

            _train = trainerFactory.Get();
        }
    }
}
