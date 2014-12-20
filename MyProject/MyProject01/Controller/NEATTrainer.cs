using Encog.ML;
using Encog.ML.Data;
using Encog.ML.Data.Basic;
using Encog.ML.EA.Train;
using Encog.Neural.NEAT;
using Encog.Neural.Networks.Training;
using MyProject01.Agent;
using MyProject01.DAO;
using MyProject01.Util;
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
        public int DealCount { set; get; }
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
        private double[] _dataArray;
        private int _blockLength;
        public RateMarketScore(double[] testData, int blockLength)
        {
            _dataArray = testData;
            _blockLength = blockLength;
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
            RateMarketAgent agent = new RateMarketAgent(_dataArray, _blockLength);
            IMLRegression reg = (IMLRegression)network;
            RateMarketAgentData stateData = agent.Reset();
            int maxActionIndex = -1;
            MarketActions currentAction;
            while (true)
            {
                if (agent.CurrentRateValue > 0)
                {
                    // Get Action Value
                    IMLData output = reg.Compute(new BasicMLData(stateData.RateDataArray, false));

                    // Choose an action
                    maxActionIndex = 0;
                    for (int i = 1; i < output.Count; i++)
                    {
                        if (output[maxActionIndex] < output[i])
                            maxActionIndex = i;
                    }

                    // Do action
                    switch (maxActionIndex)
                    {
                        case 0:
                            currentAction = MarketActions.Nothing;
                            break;
                        case 1:
                            currentAction = MarketActions.Buy;
                            break;
                        case 2:
                            currentAction = MarketActions.Sell;
                            break;
                        default:
                            currentAction = MarketActions.Nothing;
                            break;
                    }
                    stateData = agent.TakeAction(currentAction);
                }
                if (agent.Next() == false)
                    break;
            }

            return agent.CurrentValue;
        }

    }
    class LogFormater
    {
        private double[] _valueArray;
        public enum ValueName
        {
            Step = 0,
            Score,
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
                resStr += _valueArray[i].ToString("G6") + "\t";
            return resStr;
        }

        public void Set(ValueName name, double v)
        {
            this._valueArray[(int)name] = v;
        }

    }
    class NEATTrainer
    {
        public string TestName = "DefaultTest000";


        private DataBlock _testDataBlock;
        private int _trainDataLength;
        private int _testDataLength;
        private int _dataBlockLength;
        private double[] _trainDataArray;
        private double[] _testDataArray;
        private long _epoch;


        private RateMarketTestDAO _testCaseDAO;

        public NEATController Controller;
        public long IterationCount = 10;

        public void SetDataLength(DataBlock dataBlock, int trainLength)
        {
            _testDataBlock = dataBlock;
            _trainDataLength = trainLength;
        }

        public void RunTestCase()
        {
            // Check param
            if (Controller.InputVectorLength == -1 && Controller.OutputVectorLength == -1)
            {
                throw (new Exception("Parm wrong!"));
            }

            // Init train data
            _dataBlockLength = Controller.InputVectorLength;
            _testDataLength = _testDataBlock.Length - _dataBlockLength - _trainDataLength;
            _trainDataArray = _testDataBlock.GetArray(0, _dataBlockLength + _trainDataLength);
            _testDataArray = _testDataBlock.GetArray(0, _testDataBlock.Length);


            //Start

            LogFormater log = new LogFormater();
            _testCaseDAO = RateMarketTestDAO.GetDAO<RateMarketTestDAO>(TestName, true);
            _testCaseDAO.DataBlockCount = _dataBlockLength;
            _testCaseDAO.TestDataStartIndex = _trainDataArray.Length;
            _testCaseDAO.TotalDataCount = _testDataArray.Length;
            // _testCaseDAO.TestData = _testDataArray;


            byte[] LastNetData = null;

            ICalculateScore score = new RateMarketScore(_trainDataArray, _dataBlockLength);
            // train the neural network
            TrainEA train = NEATUtil.ConstructNEATTrainer(Controller.GetPopulation(), score);

            _epoch = 1;

            LogFile.WriteLine(@"Beginning training...");
            LogFile.WriteLine(log.GetTitle());
            do
            {
                 if( (IterationCount > 0 ) && (_epoch > IterationCount) )
                 {
                     break;
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

                     log.Set(LogFormater.ValueName.Step, _epoch);
                     log.Set(LogFormater.ValueName.Score, train.BestGenome.Score);

                     LogFile.WriteLine(log.GetLog());
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
            RateMarketAgent agent = new RateMarketAgent(_testDataArray, _dataBlockLength);
            RateMarketAgentData stateData = agent.Reset();
            int maxActionIndex = -1;
            MarketActions currentAction;

            EpisodeLog epsodeLog = new EpisodeLog();
            int dealCount = 0;
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
                    IMLData output = network.Compute(new BasicMLData(stateData.RateDataArray, false));

                    // Choose an action
                    maxActionIndex = 0;
                    for (int i = 1; i < output.Count; i++)
                    {
                        if (output[maxActionIndex] < output[i])
                            maxActionIndex = i;
                    }

                    // Do action
                    switch (maxActionIndex)
                    {
                        case 0:
                            currentAction = MarketActions.Nothing;
                            break;
                        case 1:
                            currentAction = MarketActions.Buy;
                            dealCount++;
                            break;
                        case 2:
                            currentAction = MarketActions.Sell;
                            dealCount++;
                            break;
                        default:
                            currentAction = MarketActions.Nothing;
                            break;
                    }

                    dealLog = new DealLog()
                    {
                        Action = currentAction,
                        CurrentMoney = agent.CurrentValue,

                    };
                    // To large for test
                    // epsodeLog.DealLogs.Add(dealLog);
                    if (agent.index == trainedDataIndex)
                        trainedMoney = agent.CurrentValue;

                    stateData = agent.TakeAction(currentAction);
                }
                if (agent.Next() == false)
                    break;
            } // end while
            endMoney = agent.CurrentValue;

            epsodeLog.TrainedDataEarnRate = (trainedMoney / startMoney) * 100;
            epsodeLog.UnTrainedDataEarnRate = (endMoney / trainedMoney) * 100;
            epsodeLog.DealCount = dealCount;
            epsodeLog.HidenNodeCount = network.Links.Length;
            epsodeLog.ResultMoney = endMoney;
            epsodeLog.Step = _epoch;
            dao.AddEpisode(epsodeLog);

            // update dao
            dao.LastTestDataEarnRate = epsodeLog.UnTrainedDataEarnRate;
            dao.LastTrainedDataEarnRate = epsodeLog.TrainedDataEarnRate;


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
