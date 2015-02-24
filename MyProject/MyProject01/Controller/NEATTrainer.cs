using Encog.ML;
using Encog.ML.Data;
using Encog.ML.Data.Basic;
using Encog.ML.EA.Train;
using Encog.Neural.NEAT;
using Encog.Neural.Networks.Training;
using MyProject01.Agent;
using MyProject01.DAO;
using MyProject01.ExchangeRateTrade;
using MyProject01.Util;
using MyProject01.Util.DataObject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.Controller
{
    class DealLog
    {
        public MarketActions Action { set; get; }
        public double CurrentMoney { set; get; }
    }
    class EpisodeLog : BasicTestEpisodeDAO
    {
        public double ResultMoney { set; get; }
        public int TrainedDealCount { set; get; }
        public int UntrainedDealCount { set; get; }
        public double TrainedDataEarnRate { set; get; }
        public double UnTrainedDataEarnRate { set; get; }
        public List<DealLog> DealLogs { set; get; }

        public long Step { set; get; }

        // ================
        // Network
        public int HidenNodeCount { set; get; }

        // ====================
        // Functions
        public EpisodeLog()
        {
            DealLogs = new List<DealLog>();
        }
    }

    public class RateMarketScore : ICalculateScore
    {
        private BasicDataBlock _dataBlock;
        private int _blockLength;
        public int StartIndex;
        public int Length;
        public void SetData(BasicDataBlock dataBlock, int blockLength)
        {
            _dataBlock = dataBlock;
            _blockLength = blockLength;
            StartIndex = 0;
            Length = _dataBlock.Length;
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
            RateMarketAgent agent = new RateMarketAgent(_dataBlock.GetNewBlock(StartIndex, Length));
            TradeController tradeCtrl = new TradeController(agent, (IMLRegression)network);
            while (true)
            {
                if (agent.CurrentRateValue > 0)
                {
                    // Get Action Value
                    tradeCtrl.DoAction();
                }
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
    class LogFormater
    {
        private double[] _valueArray;
        public enum ValueName
        {
            Step = 0,
            TrainScore,
            UnTrainScore,
        }

        public LogFormater()
        {
            _valueArray = new double[Enum.GetValues(typeof(ValueName)).Length];
        }

        public string GetTitle()
        {
            string title = "";
            string[] arr = Enum.GetNames(typeof(ValueName));
            for (int i = 0; i < arr.Length; i++)
                title += arr[i].ToString() + "\t";
            return title;
        }

        public string GetLog()
        {
            string resStr = "";
            for (int i = 0; i < _valueArray.Length; i++)
                resStr += _valueArray[i].ToString("G6") + "    \t";
            return resStr;
        }

        public void Set(ValueName name, double v)
        {
            this._valueArray[(int)name] = v;
        }

    }
    class TrainingData
    {
        private BasicDataBlock _dataBlock;
        private int _testLength;

        public BasicDataBlock DataBlock
        {
            get { return _dataBlock; }
        }
        public int TestLength
        {
            get { return _testLength; }
        }


        public TrainingData(BasicDataBlock block, int testLength)
        {
            _dataBlock = block;
            _testLength = testLength;
        }

    }
    class TrainDataList : List<TrainingData> 
    {
        private Random _rand;

        public TrainDataList()
        {
            _rand = new Random();
        }
        public TrainingData GetNext()
        {
            return this[_rand.Next(Count)];
        }
    }

    class NEATTrainer
    {
        public string TestName = "DefaultTest000";
        public TrainDataList DataList;

        private BasicDataBlock _testDataBlock;
        private BasicDataBlock _trainDataBlock;
        private int _trainDataLength;
        private int _dataBlockLength;
        private long _epoch;
        private LogFormater _log = new LogFormater();

        private RateMarketTestDAO _testCaseDAO;

        public NEATController Controller;
        public long IterationCount = 10;

        public NEATTrainer()
        {
            DataList = new TrainDataList();
        }
        private void SetDataLength(BasicDataBlock dataBlock, int trainLength)
        {
            _testDataBlock = dataBlock;
            _trainDataLength = trainLength;

            // Update test data
            _dataBlockLength = Controller.InputVectorLength;
            _trainDataBlock = _testDataBlock.GetNewBlock(0, _trainDataLength);

        }

        public void RunTestCase()
        {
            TrainingData trainData;
            // Check param
            if (Controller.InputVectorLength == -1 && Controller.OutputVectorLength == -1)
            {
                throw (new Exception("Parm wrong!"));
            }


            // Set test data
            trainData = DataList.GetNext();
            SetDataLength(trainData.DataBlock, trainData.TestLength);

            //Start
            _log = new LogFormater();
            _testCaseDAO = RateMarketTestDAO.GetDAO<RateMarketTestDAO>(TestName, true);
            _testCaseDAO.DataBlockCount = _dataBlockLength;
            _testCaseDAO.TestDataStartIndex = _trainDataBlock.Length;
            _testCaseDAO.TotalDataCount = _testDataBlock.Length;
            // _testCaseDAO.TestData = _testDataArray;


            byte[] LastNetData = null;

            RateMarketScore score = new RateMarketScore();
            score.SetData(_trainDataBlock, _dataBlockLength);

            // train the neural network
            TrainEA train = NEATUtil.ConstructNEATTrainer(Controller.GetPopulation(), score);
            
            _epoch = 1;

            LogFile.WriteLine(@"Beginning training...");
            LogFile.WriteLine(_log.GetTitle());

            
            do
            {
                if ((IterationCount > 0) && (_epoch % IterationCount == 0))
                 {
                     // set next test data
                     trainData = DataList.GetNext();
                     SetDataLength(trainData.DataBlock, trainData.TestLength);
                     score.SetData(_trainDataBlock, _dataBlockLength);
                     train.FinishTraining();
                     train = NEATUtil.ConstructNEATTrainer(Controller.GetPopulation(), score); 
                     LogFile.WriteLine("Change data!");
                 }

                if( _epoch % 100 == 1)
                {
                    score.Length = 10;
                    score.StartIndex++;
                    if (score.StartIndex > _trainDataBlock.Length - 10)
                        score.StartIndex = 0;
                }
                 try
                 {
                     train.Iteration();


                     NEATNetwork episodeNet = (NEATNetwork)train.CODEC.Decode(train.BestGenome);
                     Controller.BestNetwork = episodeNet;
                     byte[] netData = NetworkToByte(episodeNet);
                     if (ByteArrayCompare(netData, LastNetData) == false)
                     {
                         TestResult(episodeNet, _testCaseDAO);
                         LastNetData = netData;
                         Controller.Save();

                     }
                     _testCaseDAO.NetworkData = netData;
                     _testCaseDAO.Step = _epoch;
                     _testCaseDAO.Save();

                     _log.Set(LogFormater.ValueName.Step, _epoch);

                     LogFile.WriteLine(_log.GetLog());
                 }
                catch(Exception e)
                 {
                     LogFile.WriteLine("Error!");
                 }
                _epoch++;


            } while (true);
            train.FinishTraining();
            
            // test the neural network
            LogFile.WriteLine(@"Training end");
        }



        private void TestResult(NEATNetwork network, RateMarketTestDAO dao)
        {
            RateMarketAgent agent = new RateMarketAgent(_testDataBlock);
            TradeController tradeCtrl = new TradeController(agent, network);
            EpisodeLog epsodeLog = new EpisodeLog();
            int trainDealCount = 0;
            DealLog dealLog;
            int trainedDataIndex = _trainDataLength;
            double startMoney = agent.InitMoney;
            double trainedMoney = 0;
            double endMoney = 0;
            while (true)
            {
                if (agent.CurrentRateValue > 0)
                {
                    // Get Action Value
                    tradeCtrl.DoAction();

                    dealLog = new DealLog()
                    {
                        Action = tradeCtrl.LastAction,
                        CurrentMoney = agent.CurrentValue,

                    };
                    // To large for test
                    // epsodeLog.DealLogs.Add(dealLog);
                    if (agent.index == trainedDataIndex)
                    {
                        trainedMoney = agent.CurrentValue;
                        // trainDealCount = dealCount;
                        trainDealCount = agent.DealCount;
                    }

                }
                if (agent.IsEnd == true)
                    break;
            } // end while
            endMoney = agent.CurrentValue;

            epsodeLog.TrainedDataEarnRate = (trainedMoney / startMoney) * 100;
            epsodeLog.UnTrainedDataEarnRate = (endMoney / trainedMoney) * 100;
            epsodeLog.TrainedDealCount = trainDealCount;
            epsodeLog.UntrainedDealCount = agent.DealCount - trainDealCount;
            epsodeLog.HidenNodeCount = network.Links.Length;
            epsodeLog.ResultMoney = endMoney;
            epsodeLog.Step = _epoch;
            dao.AddEpisode(epsodeLog);

            // update dao
            dao.LastTestDataEarnRate = epsodeLog.UnTrainedDataEarnRate;
            dao.LastTrainedDataEarnRate = epsodeLog.TrainedDataEarnRate;

            // update log
            _log.Set(LogFormater.ValueName.TrainScore, epsodeLog.TrainedDataEarnRate);
            _log.Set(LogFormater.ValueName.UnTrainScore, epsodeLog.UnTrainedDataEarnRate);

        }

        private byte[] NetworkToByte(NEATNetwork network)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, network);

            byte[] res = stream.ToArray();
            stream.Close();
            return res;
        }

        private bool ByteArrayCompare(byte[] arr1, byte[] arr2)
        {
            if (arr1 == null || arr2 == null)
                return false;

            if (arr1.Length != arr2.Length)
                return false;
            for (int i = 0; i < arr1.Length; i++)
            {
                if (arr1[i] != arr2[i])
                    return false;
            }
            return true;
        }
    }
}
