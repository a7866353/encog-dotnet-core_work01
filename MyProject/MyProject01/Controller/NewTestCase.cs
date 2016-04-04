using Encog.ML;
using Encog.Neural.NEAT;
using Encog.Neural.Networks.Training;
using MyProject01.Agent;
using MyProject01.Controller.Jobs;
using MyProject01.DAO;
using MyProject01.Factorys.PopulationFactorys;
using MyProject01.Util;
using MyProject01.Util.DllTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.Controller
{
    class NewTestDataPacket
    {
        static public BasicTestDataLoader GetOneWeek()
        {
            DateTime StartDateTime = new DateTime(2013, 11, 1);
            DateTime EndDateTime = new DateTime(2013, 11, 7);
            BasicTestDataLoader loader =
                new TestDataDateRangeLoader("USDJPY", DataTimeType.M30, StartDateTime, EndDateTime, 50000);
            // loader.Load();
            return loader;
        }
        static public BasicTestDataLoader GetRecentM5_1Month()
        {
            DateTime StartDateTime = new DateTime(2016, 2, 13);
            DateTime EndDateTime = new DateTime(2016, 3, 13);
            BasicTestDataLoader loader =
                new TestDataDateRangeLoader("USDJPY_1", DataTimeType.M5, StartDateTime, EndDateTime, 50000) { NeedTimeFrameConver = true };
            // loader.Load();
            return loader;
        }
        static public BasicTestDataLoader GetRecentM5_3Month()
        {
            DateTime StartDateTime = new DateTime(2015, 12, 13);
            DateTime EndDateTime = new DateTime(2016, 3, 13); 
            BasicTestDataLoader loader =
                new TestDataDateRangeLoader("USDJPY_5", DataTimeType.M5, StartDateTime, EndDateTime, 50000) { NeedTimeFrameConver = false };
            // loader.Load();
            return loader;
        }
        static public BasicTestDataLoader GetM5_1Year()
        {
            DateTime StartDateTime = new DateTime(2013, 10, 31);
            DateTime EndDateTime = new DateTime(2014, 10, 31);
            BasicTestDataLoader loader =
                new TestDataDateRangeLoader("USDJPY", DataTimeType.M5, StartDateTime, EndDateTime, 50000);
            // loader.Load();
            return loader;
        }
        static public BasicTestDataLoader GetM30OneMonth()
        {
            DateTime StartDateTime = new DateTime(2013, 11, 1);
            DateTime EndDateTime = new DateTime(2013, 12, 31);
            BasicTestDataLoader loader =
                new TestDataDateRangeLoader("USDJPY", DataTimeType.M30, StartDateTime, EndDateTime, 50000);
            // loader.Load();
            return loader;
        }
        static public BasicTestDataLoader GetM30OneYear()
        {
            DateTime StartDateTime = new DateTime(2013, 10, 31);
            DateTime EndDateTime = new DateTime(2014, 10, 31);
            BasicTestDataLoader loader =
                new TestDataDateRangeLoader("USDJPY", DataTimeType.M30, StartDateTime, EndDateTime, 50000);
            // loader.Load();
            return loader;
        }
        static public BasicTestDataLoader GetD1OneMounth()
        {
            DateTime StartDateTime = new DateTime(2013, 11, 1);
            DateTime EndDateTime = new DateTime(2013, 12, 31);
            BasicTestDataLoader loader =
                new TestDataDateRangeLoader("USDJPY", DataTimeType.D1, StartDateTime, EndDateTime, 50000);
            // loader.Load();
            return loader;
        }
        static public BasicTestDataLoader Get1DayOneYear()
        {
            DateTime StartDateTime = new DateTime(2011, 10, 31);
            DateTime EndDateTime = new DateTime(2014, 10, 31);
            BasicTestDataLoader loader =
                new TestDataDateRangeLoader("USDJPY", DataTimeType.D1, StartDateTime, EndDateTime, 50000);
            // loader.Load();
            return loader;
        }
        static public BasicTestDataLoader Get1Day10Year()
        {
            DateTime StartDateTime = new DateTime(2004, 10, 31);
            DateTime EndDateTime = new DateTime(2014, 10, 31);
            BasicTestDataLoader loader =
                new TestDataDateRangeLoader("USDJPY", DataTimeType.D1, StartDateTime, EndDateTime, 50000);
            // loader.Load();
            return loader;
        }

        static public BasicTestDataLoader GetRecnetM30OneMonth()
        {
            DateTime StartDateTime = new DateTime(2016, 2, 13);
            DateTime EndDateTime = new DateTime(2016, 3, 13);
            BasicTestDataLoader loader =
                new TestDataDateRangeLoader("USDJPY_1", DataTimeType.M30, StartDateTime, EndDateTime, 50000);
            // loader.Load();
            return loader;
        }
        static public BasicTestDataLoader GetRecnetM30_3Month()
        {
            DateTime StartDateTime = new DateTime(2015, 12, 13);
            DateTime EndDateTime = new DateTime(2016, 3, 13);
            BasicTestDataLoader loader =
                new TestDataDateRangeLoader("USDJPY_1", DataTimeType.M30, StartDateTime, EndDateTime, 50000);
            // loader.Load();
            return loader;
        }
    }
    public class NewNormalScore : ICalculateScore
    {
        public ControllerFactory _ctrlFactory;
        public double _testRate = 1.0;
        public int StartPosition = 50000;

        public NewNormalScore(ControllerFactory ctrlFactory, double testRate = 1.0)
        {
            _ctrlFactory = ctrlFactory;
            _testRate = testRate;
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
            IController ctrl = _ctrlFactory.Get();
            ctrl.UpdateNetwork((IMLRegression)network);
            LearnRateMarketAgent agent = new LearnRateMarketAgent(ctrl);
            int testCount = ctrl.TotalLength - StartPosition;
            testCount = (int)(testCount * _testRate);
            agent.SetRange(StartPosition, StartPosition + testCount);

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
            if (context.IsChanged == false)
                return true;

            // ControllerDAOV2 dao = ControllerDAOV2.GetDAOByName("Controller"+DateTime.Now);
            ControllerDAOV2 dao = new ControllerDAOV2();
            dao.Name = "Controller" + DateTime.Now;
            dao.CaseName = _caseName;
            dao.StepNum = (int)context.Epoch;
            dao.UpdateTime = DateTime.Now;
            _packer.NeuroNetwork = context.BestNetwork;
            dao.ControllerData = _packer.GetData();
            dao.Save();
            context.ControllerName = dao.Name;

            return true;
        }
    }
    class NewUpdateTestCaseJob : ICheckJob
    {
        public string TestName;
        public string TestDescription;
        public IController Controller;
        public int StartPosition = 50000;
        public int TrainDataLength;
        public int TestDataLength;

        private RateMarketTestDAO _testCaseDAO;

        private int _trainDataLength;
        private long _epoch;
        private LogFormater _log;

        private TrainerContex _context;
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

            _context = context;
            if (_testCaseDAO == null)
            {
                _testCaseDAO = RateMarketTestDAO.GetDAO<RateMarketTestDAO>(TestName, true);
                _testCaseDAO.TestDescription = TestDescription;
                // TODO
                _testCaseDAO.TotalDataCount = TrainDataLength + TestDataLength;
                _testCaseDAO.TestDataStartIndex = TrainDataLength;
                _testCaseDAO.Time = _context.StartDate;

            }

            _epoch = context.Epoch;
            _trainDataLength = StartPosition + _testCaseDAO.TestDataStartIndex;
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
            agent.SetRange(StartPosition, Controller.TotalLength);

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
            epsodeLog.ControllerName = _context.ControllerName;
            epsodeLog.Time = _context.CurrentDate;
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
        private BasicControllerWithCache _testCtrl;
        private BasicTestDataLoader _loader;
        private double _testRate = 0.7;
        private int _startPosition = 50000;

        private int _trainDataLength;
        private int _testDataLength;
        public void Run()
        {
            _loader = GetDataLoader();
            _loader.Load();
            
            _testCtrl = new BasicControllerWithCache(GetSensor(), GetActor());
            _testCtrl.DataSourceCtrl = new DataSources.DataSourceCtrl(_loader);

            int totalDataLength = _testCtrl.TotalLength - _startPosition;
            _trainDataLength = (int)(totalDataLength * _testRate);
            _testDataLength = totalDataLength - _trainDataLength;

            _testCtrl.Normilize(0, 1.0);

            BasicControllerWithCache trainCtrl = (BasicControllerWithCache)_testCtrl.Clone();
            trainCtrl.DataSourceCtrl = new DataSources.DataSourceCtrl(_loader); // TODO
            _ctrlFac = new ControllerFactory(trainCtrl);

            NewTrainer trainer = new NewTrainer(_testCtrl.NetworkInputVectorLength,
                _testCtrl.NetworkOutputVectorLenth);

            trainer.CheckCtrl = CreateCheckCtrl();
            trainer.TestName = "";
            trainer.PopulationFacotry = new NormalPopulationFactory();
            trainer.ScoreCtrl = new NewNormalScore(_ctrlFac, _testRate);
           
            trainer.RunTestCase();
        }

        private ICheckJob CreateCheckCtrl()
        {
            TrainResultCheckSyncController mainCheckCtrl = new TrainResultCheckSyncController();
            mainCheckCtrl.Add(new CheckNetworkChangeJob());
            mainCheckCtrl.Add(new NewUpdateControllerJob(TestCaseName, _testCtrl.GetPacker()));

            // TrainResultCheckAsyncController subCheckCtrl = new TrainResultCheckAsyncController();
            // subCheckCtrl.Add(new UpdateTestCaseJob() 
            IController testCtrl = _ctrlFac.Get();
            testCtrl.DataSourceCtrl = new DataSources.DataSourceCtrl(_loader);
            mainCheckCtrl.Add(new NewUpdateTestCaseJob()
            {
                TestName = TestCaseName,
                TestDescription = "",
                Controller = testCtrl,
                TrainDataLength = _trainDataLength,
                TestDataLength = _testDataLength,
                StartPosition = _startPosition,
            });

            // mainCheckCtrl.Add(subCheckCtrl);

            return mainCheckCtrl;

        }

        abstract protected ISensor GetSensor();
        abstract protected IActor GetActor();
        abstract protected BasicTestDataLoader GetDataLoader();
        abstract public string TestCaseName
        {
            get;
        }
    }
    /*
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

        protected override BasicTestDataLoader GetDataLoader()
        {
            return  new MTDataLoader("USDJPY", DataTimeType.M5);
        }

        public override string TestCaseName
        {
            get { return "NewTest" + DateTime.Now; }
        }
    }
     */


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

        protected override BasicTestDataLoader GetDataLoader()
        {
            return NewTestDataPacket.GetM30OneYear();
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

        protected override BasicTestDataLoader GetDataLoader()
        {
            return NewTestDataPacket.GetM30OneMonth();
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

        protected override BasicTestDataLoader GetDataLoader()
        {
            return NewTestDataPacket.GetM30OneMonth();
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

        protected override BasicTestDataLoader GetDataLoader()
        {
            return NewTestDataPacket.GetM30OneMonth();
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
            SensorGroup senGroup = new SensorGroup()
            {
                // new RateSensor(64),
                new RateFWTSensor(256),
                new KDJ_KSensor(64),
                new KDJ_DSensor(64),
                new KDJ_JSensor(64),
                new KDJ_KDCrossSensor(64),
                new KDJ_DJCrossSensor(64),
                new KDJ_KJCrossSensor(64),
                new KDJ_CrossSensor(64)
            };
            return senGroup;
        }

        protected override IActor GetActor()
        {
            BasicActor actor = new BasicActor();
            return actor;
        }

        protected override BasicTestDataLoader GetDataLoader()
        {
            return NewTestDataPacket.GetM30OneMonth();
        }

        public override string TestCaseName
        {
            get { return "NewTestCase_All_30Min_Year" + DateTime.Now; }
        }
    }
    class NewTestCase_All_Switch_5Min_Short : BasicNewTestCase
    {
        protected override ISensor GetSensor()
        {
            SensorGroup senGroup = new SensorGroup()
            {
                // new RateSensor(64),
                // new RateFWTSensor(256),
                new KDJ_KSensor(8),
                new KDJ_DSensor(8),
                new KDJ_JSensor(8),
                new KDJ_KDCrossSensor(8),
                new KDJ_DJCrossSensor(8),
                new KDJ_KJCrossSensor(8),
                new KDJ_CrossSensor(8)
            };
            return senGroup;
        }

        protected override IActor GetActor()
        {
            IActor actor = new StateSwitchActor();
            return actor;
        }

        protected override BasicTestDataLoader GetDataLoader()
        {
            return NewTestDataPacket.GetM30OneMonth();
        }

        public override string TestCaseName
        {
            get { return "NewTestCase_All_Switch_5Min_Short" + DateTime.Now; }
        }
    }
    class NewTestCase_All_Switch_1Day_Short : BasicNewTestCase
    {
        protected override ISensor GetSensor()
        {
            SensorGroup senGroup = new SensorGroup()
            {
                // new RateSensor(64),
                // new RateFWTSensor(256),
                new KDJ_KSensor(8),
                new KDJ_DSensor(8),
                new KDJ_JSensor(8),
                new KDJ_KDCrossSensor(8),
                new KDJ_DJCrossSensor(8),
                new KDJ_KJCrossSensor(8),
                new KDJ_CrossSensor(8)
            };
            return senGroup;
        }

        protected override IActor GetActor()
        {
            IActor actor = new StateSwitchActor();
            return actor;
        }

        protected override BasicTestDataLoader GetDataLoader()
        {
            return NewTestDataPacket.GetD1OneMounth();
        }

        public override string TestCaseName
        {
            get { return "NewTestCase_All_Switch_1Day_Short" + DateTime.Now; }
        }
    }
    class NewTestCase_All_SwitchClose_5Min_Short : BasicNewTestCase
    {
        protected override ISensor GetSensor()
        {
            SensorGroup senGroup = new SensorGroup()
            {
                // new RateSensor(64),
                new RateFWTSensor(256),
                new KDJ_KSensor(64),
                new KDJ_DSensor(64),
                new KDJ_JSensor(64),
                new KDJ_KDCrossSensor(64),
                new KDJ_DJCrossSensor(64),
                new KDJ_KJCrossSensor(64),
                new KDJ_CrossSensor(64)
            };
            return senGroup;
        }

        protected override IActor GetActor()
        {
            IActor actor = new StateSwitchWithCloseActor();
            return actor;
        }

        protected override BasicTestDataLoader GetDataLoader()
        {
            return NewTestDataPacket.GetM30OneMonth();
        }

        public override string TestCaseName
        {
            get { return "NewTestCase_All_SwitchClose_5Min_Short" + DateTime.Now; }
        }
    }
    class NewTestCase_All_1Day_Long : BasicNewTestCase
    {
        protected override ISensor GetSensor()
        {
            SensorGroup senGroup = new SensorGroup();
            senGroup.Add(new RateFWTSensor(128));
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

        protected override BasicTestDataLoader GetDataLoader()
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
            senGroup.Add(new RateFWTSensor(128));
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

        protected override BasicTestDataLoader GetDataLoader()
        {
            return NewTestDataPacket.Get1DayOneYear();
        }

        public override string TestCaseName
        {
            get { return "NewTestCase_All_1Day_Short" + DateTime.Now; }
        }
    }


    class NewTestCaseContainer : BasicNewTestCase
    {
        private ISensor _sensor;
        private IActor _actor;
        private BasicTestDataLoader _loader;
        private string _name;

        public NewTestCaseContainer(ISensor sensor, IActor actor, BasicTestDataLoader loader, string name)
        {
            _sensor = sensor;
            _actor = actor;
            _loader = loader;
            _name = name;

        }

        protected override ISensor GetSensor()
        {
            return _sensor;
        }

        protected override IActor GetActor()
        {
            return _actor;
        }

        protected override BasicTestDataLoader GetDataLoader()
        {
            return _loader;
        }

        public override string TestCaseName
        {
            get { return _name + "\t" + DateTime.Now; }
        }
    }



    class NewTestContainer
    {
        public ISensor Sensor;
        public IActor Actor;
        public BasicTestDataLoader Loader;
        public string Name;
    }


    class NewTestCollecor
    {
        static NewTestContainer[] _testArr;
        static NewTestCollecor()
        {
            _testArr = new NewTestContainer[]
            {
                new NewTestContainer(){ Name="FWT2-1024-4-Recent-M5-1Month", Sensor = new RateFWT2Sensor(1024){ DataCollectLength = 4}, 
                    Actor = new BasicActor(),   Loader=NewTestDataPacket.GetRecentM5_1Month()},
                new NewTestContainer(){ Name="FWT2-4096-4-Recent-M5-1Month", Sensor = new RateFWT2Sensor(4096){ DataCollectLength = 4}, 
                    Actor = new BasicActor(),   Loader=NewTestDataPacket.GetRecentM5_1Month()},
                new NewTestContainer(){ Name="FWT2-16384-4-Recent-M5-1Month", Sensor = new RateFWT2Sensor(16384){ DataCollectLength = 4}, 
                    Actor = new BasicActor(),   Loader=NewTestDataPacket.GetRecentM5_1Month()},
                new NewTestContainer(){ Name="FWT2-32768-4-Recent-M5-1Month", Sensor = new RateFWT2Sensor(32768){ DataCollectLength = 4}, 
                    Actor = new BasicActor(),   Loader=NewTestDataPacket.GetRecentM5_1Month()},

                new NewTestContainer(){ Name="FWT2-4096-4-Recent-M30-1Month", Sensor = new RateFWT2Sensor(4096){ DataCollectLength = 4}, 
                    Actor = new BasicActor(),   Loader=NewTestDataPacket.GetRecnetM30OneMonth()},

                new NewTestContainer(){ Name="FWT2-1024-4-Recent-M30-1Month", Sensor = new RateFWT2Sensor(1024){ DataCollectLength = 4}, 
                    Actor = new BasicActor(),   Loader=NewTestDataPacket.GetRecnetM30OneMonth()},


                new NewTestContainer(){ Name="Haar-64-4-Recent-M30-1Month", 
                    Sensor = new RateWaveletSensor(64, new HaarWavelet(),4), 
                    Actor = new BasicActor(),   Loader=NewTestDataPacket.GetRecnetM30OneMonth() 
                },
                new NewTestContainer(){ Name="Daubechies8-64-4-Recent-M30-1Month", 
                    Sensor = new RateWaveletSensor(64, new Daubechies8Wavelet(),4), 
                    Actor = new BasicActor(),   Loader=NewTestDataPacket.GetRecnetM30OneMonth() 
                },

                new NewTestContainer(){ Name="Daubechies4-64-4-Recent-M30-1Month", 
                    Sensor = new RateWaveletSensor(64, new Daubechies4Wavelet(),4), 
                    Actor = new BasicActor(),   Loader=NewTestDataPacket.GetRecnetM30OneMonth() 
                },

                new NewTestContainer(){ Name="Legendre6-64-4-Recent-M30-1Month", 
                    Sensor = new RateWaveletSensor(64, new Legendre6Wavelet(),4), 
                    Actor = new BasicActor(),   Loader=NewTestDataPacket.GetRecnetM30OneMonth() 
                },


                new NewTestContainer(){ Name="Haar-1024-4-Recent-M5-1Month", 
                    Sensor = new RateWaveletSensor(1024, new HaarWavelet(),4), 
                    Actor = new BasicActor(),   Loader=NewTestDataPacket.GetRecentM5_1Month() 
                },
                new NewTestContainer(){ Name="Daubechies8-1024-4-Recent-M5-1Month", 
                    Sensor = new RateWaveletSensor(1024, new Daubechies8Wavelet(),4), 
                    Actor = new BasicActor(),   Loader=NewTestDataPacket.GetRecentM5_1Month() 
                },

                new NewTestContainer(){ Name="Daubechies4-1024-4-Recent-M5-1Month", 
                    Sensor = new RateWaveletSensor(1024, new Daubechies4Wavelet(),4), 
                    Actor = new BasicActor(),   Loader=NewTestDataPacket.GetRecentM5_1Month() 
                },

                new NewTestContainer(){ Name="Legendre6-1024-4-Recent-M5-1Month", 
                    Sensor = new RateWaveletSensor(1024, new Legendre6Wavelet(),4), 
                    Actor = new BasicActor(),   Loader=NewTestDataPacket.GetRecentM5_1Month() 
                },

                new NewTestContainer(){ Name="Legendre6-1024-16-Recent-M5-1Month", 
                    Sensor = new RateWaveletSensor(1024, new Legendre6Wavelet(),16), 
                    Actor = new BasicActor(),   Loader=NewTestDataPacket.GetRecentM5_1Month() 
                },



                new NewTestContainer(){ Name="CrossTest2-16,9-Recent_M30_1Month", Sensor = SensorUtility.GetKDJCrossSensor(16, new int[] {9}, new CrossPartten02()),
                    Actor = new BasicActor(),   Loader=NewTestDataPacket.GetRecnetM30OneMonth()},
                new NewTestContainer(){ Name="CrossTest2-16,9,89-Recent_M30_1Month", Sensor = SensorUtility.GetKDJCrossSensor(16, new int[] {9, 89}, new CrossPartten02()),     
                    Actor = new BasicActor(),   Loader=NewTestDataPacket.GetRecnetM30OneMonth()},
//--------------------------


                new NewTestContainer(){ Name="CrossTest3-4,9-Recent_M30_1Month", Sensor = SensorUtility.GetKDJCrossSensor(4, new int[] {9}, new CrossPartten03()),
                    Actor = new BasicActor(),   Loader=NewTestDataPacket.GetRecnetM30OneMonth()},
                new NewTestContainer(){ Name="CrossTest3-4,9,89-Recent_M30_1Month", Sensor = SensorUtility.GetKDJCrossSensor(4, new int[] {9, 89}, new CrossPartten03()),     
                    Actor = new BasicActor(),   Loader=NewTestDataPacket.GetRecnetM30OneMonth()},

                new NewTestContainer(){ Name="CrossTest3-4,9-Recent_M5_1Month", Sensor = SensorUtility.GetKDJCrossSensor(4, new int[] {9}, new CrossPartten03()),
                    Actor = new BasicActor(),   Loader=NewTestDataPacket.GetRecentM5_1Month()},
                new NewTestContainer(){ Name="CrossTest3-4,9,89-Recent_M5_1Month", Sensor = SensorUtility.GetKDJCrossSensor(4, new int[] {9, 89}, new CrossPartten03()),     
                    Actor = new BasicActor(),   Loader=NewTestDataPacket.GetRecentM5_1Month()},
//--------------------------
                new NewTestContainer(){ Name="CrossTest-4,59-Recent_M5_1Month", Sensor = SensorUtility.GetKDJCrossSensor(4, new int[] {59}, new CrossPartten01()),     
                    Actor = new BasicActor(),   Loader=NewTestDataPacket.GetRecentM5_1Month()},

                new NewTestContainer(){ Name="CrossTest-4,9,89-Recent_M5_1Month", Sensor = SensorUtility.GetKDJCrossSensor(4, new int[] {9, 89}, new CrossPartten01()),     
                    Actor = new BasicActor(),   Loader=NewTestDataPacket.GetRecentM5_1Month()},
                new NewTestContainer(){ Name="CrossTest-4,9,89-Recent_M30_1Month", Sensor = SensorUtility.GetKDJCrossSensor(4, new int[] {9, 89}, new CrossPartten01()),     
                    Actor = new BasicActor(),   Loader=NewTestDataPacket.GetRecnetM30OneMonth()},

                new NewTestContainer(){ Name="CrossTest5Line_Recent_M5_1Month", Sensor = SensorUtility.GetKDJCrossSensor(4, new int[] {9, 299}, new CrossPartten01()),     Actor = new BasicActor(),   Loader=NewTestDataPacket.GetRecentM5_1Month()},
                new NewTestContainer(){ Name="CrossTest5Line_Recent_M5_3Month", Sensor = SensorUtility.GetKDJCrossSensor(4, new int[] {9, 299}, new CrossPartten01()),     Actor = new BasicActor(),   Loader=NewTestDataPacket.GetRecentM5_3Month()},

                new NewTestContainer(){ Name="CrossTest_Recent_M5_1Month", Sensor = SensorUtility.GetKDJCrossSensor(4, new int[] {9}, new CrossPartten01()),     Actor = new BasicActor(),   Loader=NewTestDataPacket.GetRecentM5_1Month()},
                new NewTestContainer(){ Name="CrossTest_Recent_M30_Month", Sensor = SensorUtility.GetKDJCrossSensor(4, new int[] {9}, new CrossPartten01()),     Actor = new BasicActor(),   Loader=NewTestDataPacket.GetRecnetM30OneMonth()},
                new NewTestContainer(){ Name="CrossTest_Recent_M30_3Month", Sensor = SensorUtility.GetKDJCrossSensor(4, new int[] {9}, new CrossPartten01()),     Actor = new BasicActor(),   Loader=NewTestDataPacket.GetRecnetM30_3Month()},

                
                new NewTestContainer(){ Name="CrossTest_M5_Year",   Sensor = SensorUtility.GetKDJCrossSensor(4, new int[] {9}, new CrossPartten01()),     Actor = new BasicActor(),   Loader=NewTestDataPacket.GetM5_1Year()},
                new NewTestContainer(){ Name="CrossTest_M30_Month", Sensor = SensorUtility.GetKDJCrossSensor(4, new int[] {9}, new CrossPartten01()),     Actor = new BasicActor(),   Loader=NewTestDataPacket.GetM30OneMonth()},
                new NewTestContainer(){ Name="CrossTest_M30_Year",  Sensor = SensorUtility.GetKDJCrossSensor(4, new int[] {9}, new CrossPartten01()),     Actor = new BasicActor(),   Loader=NewTestDataPacket.GetM30OneYear()},
                new NewTestContainer(){ Name="CrossTest_D1_Year",   Sensor = SensorUtility.GetKDJCrossSensor(4, new int[] {9}, new CrossPartten01()),     Actor = new BasicActor(),   Loader=NewTestDataPacket.Get1DayOneYear()},

            };
        }
        static public BasicNewTestCase[] GetTest()
        {
            BasicNewTestCase[] testCaseArr = new BasicNewTestCase[_testArr.Length];
            for(int i=0;i<testCaseArr.Length;i++)
            {
                NewTestContainer con = _testArr[i];
                testCaseArr[i] = new NewTestCaseContainer(con.Sensor, con.Actor, con.Loader, con.Name);
            }

            return testCaseArr;
        }
    }

}
