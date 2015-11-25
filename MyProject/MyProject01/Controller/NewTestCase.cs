using Encog.ML;
using Encog.Neural.NEAT;
using Encog.Neural.Networks.Training;
using MyProject01.Agent;
using MyProject01.Controller.Jobs;
using MyProject01.DAO;
using MyProject01.Factorys.PopulationFactorys;
using MyProject01.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.Controller
{
    class NewTestDataPacket
    {
        static public DataLoader GetOneWeek()
        {
            DateTime StartDateTime = new DateTime(2013, 11, 1);
            DateTime EndDateTime = new DateTime(2013, 11, 7);
            BasicTestDataLoader loader =
                new TestDataDateRangeLoader("USDJPY", DataTimeType.M5, StartDateTime, EndDateTime, 0);
            loader.Load();
            return loader;
        }
        static public DataLoader GetOneMonth()
        {
            DateTime StartDateTime = new DateTime(2013, 11, 1);
            DateTime EndDateTime = new DateTime(2013, 11, 30);
            BasicTestDataLoader loader =
                new TestDataDateRangeLoader("USDJPY", DataTimeType.M5, StartDateTime, EndDateTime, 0);
            loader.Load();
            return loader;
        }
        static public DataLoader GetOneYear()
        {
            DateTime StartDateTime = new DateTime(2013, 10, 31);
            DateTime EndDateTime = new DateTime(2014, 10, 31);
            BasicTestDataLoader loader =
                new TestDataDateRangeLoader("USDJPY", DataTimeType.M5, StartDateTime, EndDateTime, 0);
            loader.Load();
            return loader;
        }
        static public DataLoader Get1DayOneYear()
        {
            DateTime StartDateTime = new DateTime(2011, 10, 31);
            DateTime EndDateTime = new DateTime(2014, 10, 31);
            BasicTestDataLoader loader =
                new TestDataDateRangeLoader("USDJPY", DataTimeType.D1, StartDateTime, EndDateTime, 0);
            loader.Load();
            return loader;
        }
        static public DataLoader Get1Day10Year()
        {
            DateTime StartDateTime = new DateTime(2004, 10, 31);
            DateTime EndDateTime = new DateTime(2014, 10, 31);
            BasicTestDataLoader loader =
                new TestDataDateRangeLoader("USDJPY", DataTimeType.D1, StartDateTime, EndDateTime, 0);
            loader.Load();
            return loader;
        }
    }
    public class NewNormalScore : ICalculateScore
    {
        public ControllerFactory _ctrlFactory;

        public NewNormalScore(ControllerFactory ctrlFactory)
        {
            _ctrlFactory = ctrlFactory;
        }

        public bool ShouldMinimize
        {
            get { return false; }
        }
        public bool RequireSingleThreaded
        {
            get { return false; }
        }

        public double CalculateScore(IMLMethod network)
        {
            BasicController ctrl = (BasicController)_ctrlFactory.Get();
            ctrl.UpdateNetwork((IMLRegression)network);
            LearnRateMarketAgent agent = new LearnRateMarketAgent(ctrl);
            
            while (true)
            {
                if (agent.IsEnd == true)
                    break;

                agent.DoAction();
                agent.Next();
                if (agent.IsEnd == true)
                    break;
            }
            //            System.Console.WriteLine("S: " + agent.CurrentValue);
            double score = agent.CurrentValue - agent.InitMoney;
            // System.Console.WriteLine("S: " + score);
            // return score;
            _ctrlFactory.Free(ctrl);

            return agent.CurrentValue;
        }

    }
    class NewUpdateControllerJob : ICheckJob
    {
        private NewNetworkController _ctrl;
        private ControllerPacker _packer;
        private string _caseName;
        public NewUpdateControllerJob(string caseName, ControllerPacker packer)
        {
            _caseName = caseName;
            _packer = packer;
        }


        public bool Do(TrainerContex context)
        {
            // ControllerDAOV2 dao = ControllerDAOV2.GetDAOByName("Controller"+DateTime.Now);
            ControllerDAOV2 dao = new ControllerDAOV2();
            dao.Name = "Controller" + DateTime.Now;
            dao.CaseName = _caseName;
            dao.StepNum = (int)context.Epoch;
            dao.UpdateTime = DateTime.Now;
            _packer.NeuroNetwork = context.BestNetwork;
            dao.ControllerData = _packer.GetData();

            dao.Save();
            return true;
        }
    }
    class NewUpdateTestCaseJob : ICheckJob
    {
        public string TestName;
        public string TestDescription;
        public double TestRate;
        public BasicController Controller;

        private RateMarketTestDAO _testCaseDAO;

        private int _trainDataLength;
        private long _epoch;
        private LogFormater _log;

        public NewUpdateTestCaseJob()
        {
            _log = new LogFormater();
            LogFile.WriteLine(_log.GetTitle());
            _testCaseDAO = null;
        }
        public bool Do(TrainerContex context)
        {
            if (context.IsChanged == false)
                return true;

            if (_testCaseDAO == null)
            {
                _testCaseDAO = RateMarketTestDAO.GetDAO<RateMarketTestDAO>(TestName, true);
                _testCaseDAO.TestDescription = TestDescription;
                // _testCaseDAO.TestDataStartIndex = context._trainDataBlock.BlockCount;
                // TODO
                _testCaseDAO.TestDataStartIndex = (int)(Controller.TotalLength * TestRate);
                _testCaseDAO.TotalDataCount = Controller.TotalLength;

            }

            _epoch = context.Epoch;
            _trainDataLength = _testCaseDAO.TestDataStartIndex;
            TestResult(context.BestNetwork, _testCaseDAO);

            _testCaseDAO.NetworkData = null;
            _testCaseDAO.Step = _epoch;
            _testCaseDAO.Save();

            return true;

        }
        private void TestResult(NEATNetwork network, RateMarketTestDAO dao)
        {
            Controller.UpdateNetwork(network);
            LearnRateMarketAgent agent = new LearnRateMarketAgent(Controller);
            agent.Reset();

            RateMarketTestEpisodeDAO epsodeLog = (RateMarketTestEpisodeDAO)dao.CreateEpisode();
            DealLogList logList = new DealLogList();
            int trainDealCount = 0;
            int trainedDataIndex = _trainDataLength;
            double startMoney = agent.InitMoney;
            double trainedMoney = 0;
            double endMoney = 0;
            while (true)
            {
                if (agent.IsEnd == true)
                    break;

                // Get Action Value
                agent.DoAction();

                // Add log
                logList.Add(agent.LastAction, agent.CurrentValue, agent.CurrentRateValue);

                // To large for test
                // epsodeLog.DealLogs.Add(dealLog);
                if (agent.CurrentIndex == trainedDataIndex)
                {
                    trainedMoney = agent.CurrentValue;
                    // trainDealCount = dealCount;
                    trainDealCount = agent.DealCount;
                }
                agent.Next();

            } // end while

            if (agent.CurrentIndex < trainedDataIndex)
            {
                trainedMoney = agent.CurrentValue;
                // trainDealCount = dealCount;
                trainDealCount = agent.DealCount;
                trainedDataIndex = (int)agent.CurrentIndex;
            }

            endMoney = agent.CurrentValue;

            epsodeLog.TrainedDataEarnRate = (trainedMoney / startMoney) * 100;
            epsodeLog.UnTrainedDataEarnRate = (endMoney / trainedMoney) * 100;
            epsodeLog.TrainedDealCount = trainDealCount;
            epsodeLog.UntrainedDealCount = agent.DealCount - trainDealCount;
            epsodeLog.HidenNodeCount = network.Links.Length;
            epsodeLog.ResultMoney = endMoney;
            epsodeLog.Step = _epoch;
            epsodeLog.Save();
            epsodeLog.SaveDealLogs(logList);

            // update dao
            dao.LastTestDataEarnRate = epsodeLog.UnTrainedDataEarnRate;
            dao.LastTrainedDataEarnRate = epsodeLog.TrainedDataEarnRate;

            // update log
            _log.Set(LogFormater.ValueName.TrainScore, epsodeLog.TrainedDataEarnRate);
            _log.Set(LogFormater.ValueName.UnTrainScore, epsodeLog.UnTrainedDataEarnRate);
            _log.Set(LogFormater.ValueName.Step, _epoch);
            LogFile.WriteLine(_log.GetLog());



        }

    }

    abstract class BasicNewTestCase
    {
        private ControllerFactory _ctrlFac;
        private BasicController _testCtrl;
        private DataLoader _loader;
        private double _testRate = 0.7;
        public void Run()
        {
            _loader = GetDataLoader();

            _testCtrl = new BasicController(GetSensor(), GetActor());
            _testCtrl.DataSourceCtrl = new DataSources.DataSourceCtrl(_loader);
            _testCtrl.Init();
            _testCtrl.Normilize(0, 1.0);

            BasicController trainCtrl = (BasicController)_testCtrl.Clone();
            trainCtrl.DataSourceCtrl = new DataSources.DataSourceCtrl(_loader, _testRate); // TODO
            _ctrlFac = new ControllerFactory(trainCtrl);

            NewTrainer trainer = new NewTrainer(_testCtrl.NetworkInputVectorLength,
                _testCtrl.NetworkOutputVectorLenth);

            trainer.CheckCtrl = CreateCheckCtrl();
            trainer.TestName = "";
            trainer.PopulationFacotry = new NormalPopulationFactory();
            trainer.ScoreCtrl = new NewNormalScore(_ctrlFac);

            trainer.RunTestCase();
        }

        private ICheckJob CreateCheckCtrl()
        {
            TrainResultCheckSyncController mainCheckCtrl = new TrainResultCheckSyncController();
            mainCheckCtrl.Add(new CheckNetworkChangeJob());
            mainCheckCtrl.Add(new NewUpdateControllerJob(TestCaseName, _testCtrl.GetPacker()));

            // TrainResultCheckAsyncController subCheckCtrl = new TrainResultCheckAsyncController();
            // subCheckCtrl.Add(new UpdateTestCaseJob() 
            BasicController testCtrl = (BasicController)_ctrlFac.Get();
            testCtrl.DataSourceCtrl = new DataSources.DataSourceCtrl(_loader);
            mainCheckCtrl.Add(new NewUpdateTestCaseJob()
            {
                TestName = TestCaseName,
                TestDescription = "",
                Controller = testCtrl,
                TestRate = _testRate,
            });

            // mainCheckCtrl.Add(subCheckCtrl);

            return mainCheckCtrl;

        }

        abstract protected ISensor GetSensor();
        abstract protected IActor GetActor();
        abstract protected DataLoader GetDataLoader();
        abstract public string TestCaseName
        {
            get;
        }
    }

    class NewTestCase : BasicNewTestCase
    {
        protected override ISensor GetSensor()
        {
            SensorGroup senGroup = new SensorGroup();
            senGroup.Add(new RateSensor(16));
            senGroup.Add(new RateSensor(32));
            senGroup.Add(new KDJ_KSensor(32));
            senGroup.Add(new KDJ_DSensor(32));
            senGroup.Add(new KDJ_JSensor(32));
            senGroup.Add(new KDJ_CrossSensor(32));

            return senGroup;
        }

        protected override IActor GetActor()
        {
            BasicActor actor = new BasicActor();
            return actor;
        }

        protected override DataLoader GetDataLoader()
        {
            return  new MTDataLoader("USDJPY", DataTimeType.M5);
        }

        public override string TestCaseName
        {
            get { return "NewTest" + DateTime.Now; }
        }
    }
    class NewTestCase2 : BasicNewTestCase
    {
        protected override ISensor GetSensor()
        {
            SensorGroup senGroup = new SensorGroup();
            senGroup.Add(new RateSensor(64));
            senGroup.Add(new KDJ_KSensor(64));
            senGroup.Add(new KDJ_DSensor(64));
            senGroup.Add(new KDJ_JSensor(64));
            senGroup.Add(new KDJ_CrossSensor(64));

            return senGroup;
        }

        protected override IActor GetActor()
        {
            BasicActor actor = new BasicActor();
            return actor;
        }

        protected override DataLoader GetDataLoader()
        {
            return NewTestDataPacket.GetOneYear();
        }

        public override string TestCaseName
        {
            get { return "NewTest2" + DateTime.Now; }
        }
    }
    class NewTestCase2Short : BasicNewTestCase
    {
        protected override ISensor GetSensor()
        {
            SensorGroup senGroup = new SensorGroup();
            senGroup.Add(new RateSensor(64));
            senGroup.Add(new KDJ_KSensor(64));
            senGroup.Add(new KDJ_DSensor(64));
            senGroup.Add(new KDJ_JSensor(64));
            senGroup.Add(new KDJ_CrossSensor(64));

            return senGroup;
        }

        protected override IActor GetActor()
        {
            BasicActor actor = new BasicActor();
            return actor;
        }

        protected override DataLoader GetDataLoader()
        {
            return NewTestDataPacket.GetOneMonth();
        }

        public override string TestCaseName
        {
            get { return "NewTest2Short" + DateTime.Now; }
        }
    }
    class NewTestCase_FWT : BasicNewTestCase
    {
        protected override ISensor GetSensor()
        {
            SensorGroup senGroup = new SensorGroup();
            // senGroup.Add(new RateSensor(64));
            senGroup.Add(new RateFWTSensor(512));
            /*
            senGroup.Add(new KDJ_KSensor(64));
            senGroup.Add(new KDJ_DSensor(64));
            senGroup.Add(new KDJ_JSensor(64));
            senGroup.Add(new KDJ_CrossSensor(64));
            */

            return senGroup;
        }

        protected override IActor GetActor()
        {
            BasicActor actor = new BasicActor();
            return actor;
        }

        protected override DataLoader GetDataLoader()
        {
            return NewTestDataPacket.GetOneMonth();
        }

        public override string TestCaseName
        {
            get { return "NewTestFWT" + DateTime.Now; }
        }
    }

    class NewTestCase_All : BasicNewTestCase
    {
        protected override ISensor GetSensor()
        {
            SensorGroup senGroup = new SensorGroup();
            senGroup.Add(new RateSensor(64));
            senGroup.Add(new RateFWTSensor(128));
            senGroup.Add(new KDJ_KSensor(64));
            senGroup.Add(new KDJ_DSensor(64));
            senGroup.Add(new KDJ_JSensor(64));
            senGroup.Add(new KDJ_CrossSensor(128));

            return senGroup;
        }

        protected override IActor GetActor()
        {
            BasicActor actor = new BasicActor();
            return actor;
        }

        protected override DataLoader GetDataLoader()
        {
            return NewTestDataPacket.GetOneMonth();
        }

        public override string TestCaseName
        {
            get { return "NewTestAll" + DateTime.Now; }
        }
    }
    class NewTestCase_All_5Min_Short : BasicNewTestCase
    {
        protected override ISensor GetSensor()
        {
            SensorGroup senGroup = new SensorGroup();
            // senGroup.Add(new RateSensor(64));
            senGroup.Add(new RateFWTSensor(256));
            senGroup.Add(new KDJ_KSensor(64));
            senGroup.Add(new KDJ_DSensor(64));
            senGroup.Add(new KDJ_JSensor(64));
            senGroup.Add(new KDJ_CrossSensor(64));

            return senGroup;
        }

        protected override IActor GetActor()
        {
            BasicActor actor = new BasicActor();
            return actor;
        }

        protected override DataLoader GetDataLoader()
        {
            return NewTestDataPacket.GetOneMonth();
        }

        public override string TestCaseName
        {
            get { return "NewTestCase_All_5Min_Short" + DateTime.Now; }
        }
    }
    class NewTestCase_All_1Day_Long : BasicNewTestCase
    {
        protected override ISensor GetSensor()
        {
            SensorGroup senGroup = new SensorGroup();
            senGroup.Add(new RateSensor(64));
            senGroup.Add(new RateFWTSensor(128));
            senGroup.Add(new KDJ_KSensor(64));
            senGroup.Add(new KDJ_DSensor(64));
            senGroup.Add(new KDJ_JSensor(64));
            senGroup.Add(new KDJ_CrossSensor(128));

            return senGroup;
        }

        protected override IActor GetActor()
        {
            BasicActor actor = new BasicActor();
            return actor;
        }

        protected override DataLoader GetDataLoader()
        {
            return NewTestDataPacket.Get1Day10Year();
        }

        public override string TestCaseName
        {
            get { return "NewTestCase_All_1Day_Long" + DateTime.Now; }
        }
    }
    class NewTestCase_All_1Day_Short : BasicNewTestCase
    {
        protected override ISensor GetSensor()
        {
            SensorGroup senGroup = new SensorGroup();
            senGroup.Add(new RateSensor(64));
            senGroup.Add(new RateFWTSensor(128));
            senGroup.Add(new KDJ_KSensor(64));
            senGroup.Add(new KDJ_DSensor(64));
            senGroup.Add(new KDJ_JSensor(64));
            senGroup.Add(new KDJ_CrossSensor(128));

            return senGroup;
        }

        protected override IActor GetActor()
        {
            BasicActor actor = new BasicActor();
            return actor;
        }

        protected override DataLoader GetDataLoader()
        {
            return NewTestDataPacket.Get1DayOneYear();
        }

        public override string TestCaseName
        {
            get { return "NewTestCase_All_1Day_Short" + DateTime.Now; }
        }
    }

}
