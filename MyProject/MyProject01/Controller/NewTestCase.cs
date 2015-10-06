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
    public class NewNormalScore : ICalculateScore
    {
        public BasicController _ctrl;

        public NewNormalScore(BasicController ctrl)
        {
            _ctrl = ctrl;
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
            BasicController ctrl = (BasicController)_ctrl.Clone();
            ctrl.UpdateNetwork((IMLRegression)network);
            LearnRateMarketAgent agent = new LearnRateMarketAgent(ctrl);
            
            while (true)
            {
                if (agent.IsEnd == true)
                    break;

                agent.DoAction();
                
                if (agent.IsEnd == true)
                    break;
            }
            //            System.Console.WriteLine("S: " + agent.CurrentValue);
            double score = agent.CurrentValue - agent.InitMoney;
            // System.Console.WriteLine("S: " + score);
            // return score;
            return agent.CurrentValue;
        }

    }
    class NewUpdateControllerJob : ICheckJob
    {
        private NewNetworkController _ctrl;
        private string _caseName;
        public NewUpdateControllerJob(string caseName)
        {
            _caseName = caseName;

        }


        public bool Do(TrainerContex context)
        {
            ControllerDAOV2 dao = ControllerDAOV2.GetDAOByName("Controller"+DateTime.Now);
            dao.CaseName = _caseName;
            dao.StepNum = (int)context.Epoch;
            dao.UpdateTime = DateTime.Now;


            dao.Save();
            return true;
        }
    }
    class NewUpdateTestCaseJob : ICheckJob
    {
        public string TestName;
        public string TestDescription;
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
                _testCaseDAO.TestDataStartIndex = Controller.TotalLength/2;
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


    class NewTestCase
    {
        public string TestCaseName = "NewTest";
        private BasicController ctrl;
        public void Run()
        {
            SensorGroup senGroup = new SensorGroup();
            senGroup.Add(new RateSensor(16));
            senGroup.Add(new RateSensor(32));

            BasicActor actor = new BasicActor();

            ctrl = new BasicController(senGroup, actor);


            ctrl.DataSource = new FixDataSource(new MTDataLoader("USDJPY", DataTimeType.M5));
            ctrl.Init();


            NewTrainer trainer = new NewTrainer(ctrl.NetworkInputVectorLength, 
                ctrl.NetworkOutputVectorLenth);

            trainer.CheckCtrl = CreateCheckCtrl();
            trainer.TestName = "";
            trainer.PopulationFacotry = new NormalPopulationFactory();
            trainer.ScoreCtrl = new NewNormalScore(ctrl);

            trainer.RunTestCase();

        }

        private ICheckJob CreateCheckCtrl()
        {
            TrainResultCheckSyncController mainCheckCtrl = new TrainResultCheckSyncController();
            mainCheckCtrl.Add(new CheckNetworkChangeJob());
            // TODO
            // mainCheckCtrl.Add(new NewUpdateControllerJob(Controller));

            // TrainResultCheckAsyncController subCheckCtrl = new TrainResultCheckAsyncController();
            // subCheckCtrl.Add(new UpdateTestCaseJob() 
            BasicController testCtrl = (BasicController)ctrl.Clone();
            testCtrl.DataSource = ctrl.DataSource;
            mainCheckCtrl.Add(new NewUpdateTestCaseJob()
            {
                TestName = TestCaseName,
                TestDescription = "",
                Controller = testCtrl,
            });

            // mainCheckCtrl.Add(subCheckCtrl);

            return mainCheckCtrl;

        }
    }
}
