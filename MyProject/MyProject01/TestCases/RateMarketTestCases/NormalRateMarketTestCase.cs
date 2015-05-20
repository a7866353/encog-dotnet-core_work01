using MyProject01.Factorys.ControllerFactorys;
using MyProject01.Factorys.PopulationFactorys;
using MyProject01.Factorys.TrainerFactorys;
using MyProject01.Factorys.TrainingDataFactorys;
using MyProject01.Util;
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

        private NEATFWTStateKeepControllerFactory controllerFactory = new NEATFWTStateKeepControllerFactory();
        private OldRate1DayTrainingDataFactory dataFactory = new OldRate1DayTrainingDataFactory();
        private FirstTrainerFactory trainerFactory;
        private NormalPopulationFactory popFactory;

        public NormalRateMarketTestCase()
        {
            controllerFactory = new NEATFWTStateKeepControllerFactory();
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
            trainerFactory.TestDescription = TestDescription;

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

        private NEATFWTStateKeepControllerFactory controllerFactory = new NEATFWTStateKeepControllerFactory();
        private BasicTrainingDataFactory dataFactory = new OldRate1DayTrainingDataFactory();
        private FirstTrainerFactory trainerFactory;
        private NormalPopulationFactory popFactory;


        public Normal5MinRateMarketTestCase()
        {
            controllerFactory = new NEATFWTStateKeepControllerFactory();
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

    class RawRate1DayTestCase : BasicRateMarketTestCase
    {
        public int DataBlockLength
        {
            set { controllerFactory.InputLength = value; }
        }
        public int PopulationNumber
        {
            set { popFactory.PopulationNumber = value; }
        }

        private NEATRateStateKeepControllerFactory controllerFactory;
        private OldRate1DayTrainingDataFactory dataFactory;
        private FirstTrainerFactory trainerFactory;
        private NormalPopulationFactory popFactory;

        public RawRate1DayTestCase()
        {
            controllerFactory = new NEATRateStateKeepControllerFactory();
            dataFactory = new OldRate1DayTrainingDataFactory();
            trainerFactory = new FirstTrainerFactory();
            popFactory = new NormalPopulationFactory();

            controllerFactory.InputLength = 32;

            _descList.Add(controllerFactory);
            _descList.Add(dataFactory);
            _descList.Add(trainerFactory);
            _descList.Add(popFactory);
        }

        protected override void Init()
        {
            dataFactory.DataBlockLength = controllerFactory.InputLength;

            trainerFactory.PopulationFacotry = popFactory;
            trainerFactory.TestDescription = TestDescription;

            trainerFactory.TrainingData = dataFactory.Get();
            trainerFactory.TestCaseName = TestName;

            BlockDataNormalizer norm = new BlockDataNormalizer();
            norm.Normalize(trainerFactory.TrainingData.TrainDataBlock);
            controllerFactory.Name = TestName;
            controllerFactory.Offset = norm.Offset;
            controllerFactory.Scale = norm.Scacle;
            trainerFactory.Controller = controllerFactory.Get();

            _train = trainerFactory.Get();
        }
    }
    class RawRate5MinTestCase : BasicRateMarketTestCase
    {
        public int DataBlockLength
        {
            set { controllerFactory.InputLength = value; }
        }
        public int PopulationNumber
        {
            set { popFactory.PopulationNumber = value; }
        }

        private NEATRateStateKeepControllerFactory controllerFactory;
        private OldRate5MinTrainingDataFactory dataFactory;
        private FirstTrainerFactory trainerFactory;
        private NormalPopulationFactory popFactory;

        public RawRate5MinTestCase()
        {
            controllerFactory = new NEATRateStateKeepControllerFactory();
            dataFactory = new OldRate5MinTrainingDataFactory();
            trainerFactory = new FirstTrainerFactory();
            popFactory = new NormalPopulationFactory();

            controllerFactory.InputLength = 32;

            _descList.Add(controllerFactory);
            _descList.Add(dataFactory);
            _descList.Add(trainerFactory);
            _descList.Add(popFactory);
        }

        protected override void Init()
        {
            dataFactory.DataBlockLength = controllerFactory.InputLength;

            trainerFactory.PopulationFacotry = popFactory;
            trainerFactory.TestDescription = TestDescription;

            trainerFactory.TrainingData = dataFactory.Get();
            trainerFactory.TestCaseName = TestName;

            BlockDataNormalizer norm = new BlockDataNormalizer();
            norm.Normalize(trainerFactory.TrainingData.TrainDataBlock);
            controllerFactory.Name = TestName;
            controllerFactory.Offset = norm.Offset;
            controllerFactory.Scale = norm.Scacle;
            trainerFactory.Controller = controllerFactory.Get();

            _train = trainerFactory.Get();
        }
    }

    class FwtNorm1DayTestCase : BasicRateMarketTestCase
    {
        public int DataBlockLength
        {
            set { controllerFactory.InputLength = value; }
        }
        public int PopulationNumber
        {
            set { popFactory.PopulationNumber = value; }
        }

        private NEATFWTNromStateKeepControllerFactory controllerFactory;
        private OldRate1DayTrainingDataFactory dataFactory;
        private FirstTrainerFactory trainerFactory;
        private NormalPopulationFactory popFactory;

        public FwtNorm1DayTestCase()
        {
            controllerFactory = new NEATFWTNromStateKeepControllerFactory();
            controllerFactory.InputLength = 32;

            dataFactory = new OldRate1DayTrainingDataFactory();

            trainerFactory = new FirstTrainerFactory();

            popFactory = new NormalPopulationFactory();


            _descList.Add(controllerFactory);
            _descList.Add(dataFactory);
            _descList.Add(trainerFactory);
            _descList.Add(popFactory);
        }

        protected override void Init()
        {
            dataFactory.DataBlockLength = controllerFactory.InputLength;
            
            trainerFactory.PopulationFacotry = popFactory;
            trainerFactory.TestDescription = TestDescription;

            trainerFactory.TrainingData = dataFactory.Get();
            trainerFactory.TestCaseName = TestName;

            controllerFactory.NormalyzeData = trainerFactory.TrainingData.TrainDataBlock;
            controllerFactory.Name = TestName;

            trainerFactory.Controller = controllerFactory.Get();

            _train = trainerFactory.Get();
        }
    }

    class FwtNorm5MinTestCase : BasicRateMarketTestCase
    {
        public int DataBlockLength
        {
            set {controllerFactory.InputLength = value;}
        }
        public int PopulationNumber
        {
            set { popFactory.PopulationNumber = value; }
        }

        private NEATFWTNromStateKeepControllerFactory controllerFactory;
        private OldRate5MinTrainingDataFactory dataFactory;
        private FirstTrainerFactory trainerFactory;
        private NormalPopulationFactory popFactory;

        public FwtNorm5MinTestCase()
        {
            controllerFactory = new NEATFWTNromStateKeepControllerFactory();
            dataFactory = new OldRate5MinTrainingDataFactory();
            trainerFactory = new FirstTrainerFactory();
            popFactory = new NormalPopulationFactory();


            _descList.Add(controllerFactory);
            _descList.Add(dataFactory);
            _descList.Add(trainerFactory);
            _descList.Add(popFactory);
        }

        protected override void Init()
        {

            dataFactory.DataBlockLength = controllerFactory.InputLength;

            trainerFactory.PopulationFacotry = popFactory;
            trainerFactory.TestDescription = TestDescription;

            trainerFactory.TrainingData = dataFactory.Get();
            trainerFactory.TestCaseName = TestName;

            controllerFactory.NormalyzeData = trainerFactory.TrainingData.TrainDataBlock;
            controllerFactory.Name = TestName;

            trainerFactory.Controller = controllerFactory.Get();

            _train = trainerFactory.Get();
        }
    }


}
