using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Encog.Neural.Networks;
using Encog.Neural.Networks.Layers;
using Encog.ML.Data.Basic;
using Encog.Engine.Network.Activation;
using Encog.ML.Data;
using Encog.Neural.Networks.Training.Propagation.Resilient;
using Encog.Neural.Networks.Training.Propagation.Back;
using Encog.ML.Train;
using System.Diagnostics;
using System.Threading;
using MyProject01.Util;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Encog.Util.Simple;

namespace MyProject01
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private delegate void func();
        private Thread workThread;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
            this.Closing += MainWindow_Closing;


            OutputTextBox.Text = "";
            OutputTextBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            
            OutputTextBox.Foreground = Brushes.Green;
            OutputTextBox.FontSize = 12;
            OutputTextBox.Background = Brushes.Black;

            // Set lowest priority
            System.Diagnostics.Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Idle;

        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null == workThread)
                return;
            workThread.Abort();

        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LogFile.FuncList.Add(new LogFile.WriteLineFunction(WriteText));
            workThread = new Thread(new ThreadStart(MainWorkFunction));
            workThread.Priority = ThreadPriority.BelowNormal;
            workThread.Start();
        }
        private void CloseWindows()
        {
            this.Dispatcher.BeginInvoke(new func(delegate
            {
                this.Close();
            }));
        }
        private void WriteText(string str)
        {
            this.Dispatcher.BeginInvoke(new func(delegate
            {
                OutputTextBox.AppendText(str + "\r\n");
                OutputTextBox.ScrollToEnd();
            }));
        }
        

        private void MainWorkFunction()
        {
            TestANN();
            // CloseWindows();

            // XORHelloWorld test = new XORHelloWorld();
            // test.Execute();
        }

        private void TestANN()
        {
            MyTest test = new MyTest();
            string testName = "SimpleFeedFoward+BP";
            int num = 1;
            NetworkParm[] parmArr = new NetworkParm[]
            {
                new NetworkParm(testName+num++.ToString("D2"),0.03,  0.01,   500),
                new NetworkParm(testName+num++.ToString("D2"),0.03,  0.1,    500),
                new NetworkParm(testName+num++.ToString("D2"),0.03,  1,      500),
                new NetworkParm(testName+num++.ToString("D2"),0.03,  10,     500),
                new NetworkParm(testName+num++.ToString("D2"),0.03,  100,    500),
                // new NetworkParm(testName+num++.ToString("D2"),0.03,  1000,   500),
                // new NetworkParm("Test04",0.03,  10000,  100),
                new NetworkParm(testName+num++.ToString("D2"),0.01,  0.01,   500),
                new NetworkParm(testName+num++.ToString("D2"),0.01,  0.1,    500),
                new NetworkParm(testName+num++.ToString("D2"),0.01,  1,      500),
                new NetworkParm(testName+num++.ToString("D2"),0.01,  10,     500),
                // new NetworkParm(testName+num++.ToString("D2"),0.01,  100,    500),
                // new NetworkParm(testName+num++.ToString("D2"),0.01,  1000,   500),
                // new NetworkParm("Test08",0.01,  10000,  100),
                new NetworkParm(testName+num++.ToString("D2"),0.005, 0.01,   1000),
                new NetworkParm(testName+num++.ToString("D2"),0.005, 0.1,    1000),
                new NetworkParm(testName+num++.ToString("D2"),0.005, 1,      1000),
                new NetworkParm(testName+num++.ToString("D2"),0.005, 10,     1000),
                 new NetworkParm(testName+num++.ToString("D2"),0.0001, 100,     1000),
               // new NetworkParm(testName+num++.ToString("D2"),0.005, 100,    500),
                // new NetworkParm(testName+num++.ToString("D2"),0.005, 1000,   500),
                // new NetworkParm("Test12",0.005, 10000,  100),
            };
            foreach (NetworkParm parm in parmArr)
            {
                test.Execute(parm);
            }
        }
    }

    public class XORHelloWorld
    {
        /// <summary>
        /// Input for the XOR function.
        /// </summary>
        public static double[][] XORInput = {
            new[] {0.0, 0.0},
            new[] {1.0, 0.0},
            new[] {0.0, 1.0},
            new[] {1.0, 1.0}
        };

        /// <summary>
        /// Ideal output for the XOR function.
        /// </summary>
        public static double[][] XORIdeal = {
            new[] {0.0},
            new[] {1.0},
            new[] {1.0},
            new[] {0.0}
        };

         #region IExample Members

        /// <summary>
        /// Program entry point.
        /// </summary>
        /// <param name="app">Holds arguments and other info.</param>
        public void Execute()
        {
            // create a neural network, without using a factory
            var network = new BasicNetwork();
            network.AddLayer(new BasicLayer(null, true, 2));
            network.AddLayer(new BasicLayer(new ActivationSigmoid(), true, 3*10000));
            network.AddLayer(new BasicLayer(new ActivationSigmoid(), false, 1));
            network.Structure.FinalizeStructure();
            network.Reset();

            // create training data
            IMLDataSet trainingSet = new BasicMLDataSet(XORInput, XORIdeal);

            // train the neural network
            IMLTrain train = new ResilientPropagation(network, trainingSet);

            int epoch = 1;

            do
            {
                train.Iteration();
                Console.WriteLine(@"Epoch #" + epoch + @" Error:" + train.Error);
                epoch++;
            } while (train.Error > 0.00001);

            // test the neural network
            Console.WriteLine(@"Neural Network Results:");
            foreach (IMLDataPair pair in trainingSet)
            {
                IMLData output = network.Compute(pair.Input);
                Console.WriteLine(pair.Input[0] + @"," + pair.Input[1]
                                  + @", actual=" + output[0] + @",ideal=" + pair.Ideal[0]);
            }
        }

        #endregion
    }

    public class MyTest
    {
        private LogWriter logger;
        private LogWriter resultLog;
        private string networkFileName = "network.bin";

        public MyTest()
        {
            // init log
            logger = new LogWriter("log.txt");
            resultLog = new LogWriter("Results.txt");

        }


        /// <summary>
        /// Program entry point.
        /// </summary>
        /// <param name="app">Holds arguments and other info.</param>
        public void Execute(NetworkParm parm)
        {
            logger.SetFileName(parm.name + "_log.txt");
            logger.Reset();
            resultLog.SetFileName(parm.name + "_result.txt");
            resultLog.Reset();

            ResultPrintf(@"------------------------");
            LogPrintf("Test Start!" + parm.name);

            RateDataCreator dataCreator = new RateDataCreator();
            TestData data = dataCreator.GetTestData();

            if (false)
            {
                LogPrintf("[TestData]");
                string dataStr = "";
                for (int i = 0; i < data.TrainInputs.Length; i++)
                {
                    dataStr = i.ToString("D4") + ":";
                    for (int j = 0; j < data.InputSize; j++)
                    {
                        dataStr += data.TrainInputs[i][j].ToString("0.000");
                        dataStr += ", ";
                    }
                    dataStr += "| ";

                    for (int j = 0; j < data.OutputSize; j++)
                    {
                        dataStr += data.TrainIdeaOutputs[i][j].ToString("0.000");
                        dataStr += ", ";
                    }
                    LogPrintf(dataStr);
                }
            }

            BasicNetwork network = null;
            if( true )
            {
                // Create a new trained netwrok
                // network = CreateNetwork(data,parm);
                network = CreateNetwork02(data, parm);
            }
            else
            {
                // Create network from file.
                network = LoadNetwrokFromFile(parm.name);
            }

            if (null == network)
                throw (new Exception("Empty Network."));

            // test the neural network
            //   test train data
            foreach(MyTestData dataObj in data.TrainList)
            {
                IMLData res = network.Compute(new BasicMLData(dataObj.Input));
                double[] realArr = new double[data.OutputSize];
                res.CopyTo(realArr, 0, data.OutputSize);
                dataObj.Real = realArr;
            }

            //   test test data
            foreach (MyTestData dataObj in data.TestList)
            {
                IMLData res = network.Compute(new BasicMLData(dataObj.Input));
                double[] realArr = new double[data.OutputSize];
                res.CopyTo(realArr, 0, data.OutputSize);
                dataObj.Real = realArr;
            }


            // Output results.
            int loopCnt = 1;
            ResultPrintf(@"------------------------");
            ResultPrintf(@"Neural Network Results:");
            ResultPrintf(data.ToStringResults());
            LogPrintf("Test end!");
            LogPrintf("");
        }

        public BasicNetwork CreateNetwork(TestData data, NetworkParm parm)
        {
            LogPrintf("[NetworkCreate]"+parm.ToString());
            // create a neural network, without using a factory
            var network = new BasicNetwork();
            network.AddLayer(new BasicLayer(null, true, data.InputSize));
            network.AddLayer(new BasicLayer(new ActivationSigmoid(), true, (int)(data.InputSize * parm.hidenLayerRaio)));
            network.AddLayer(new BasicLayer(new ActivationSigmoid(), true, data.OutputSize));
            network.Structure.FinalizeStructure();
            network.Reset();

            // create training data
            IMLDataSet trainingSet = new BasicMLDataSet(data.TrainInputs, data.TrainIdeaOutputs);

            // train the neural network
            var train = new ResilientPropagation(network, trainingSet);
            train.ThreadCount = 8;

            int epoch = 1;
            SlidingFilter filter = new SlidingFilter(5);
            do
            {
                train.Iteration();
                // if(epoch%1 == 1 )
                LogPrintf(@"Epoch #" + epoch + @" Error:" + train.Error);
                // Save network each train.
                SaveNetworkToFile(network,parm.name);
                epoch++;
                if (epoch > parm.retryCnt)
                    break;
                if(filter.Add(train.Error) < parm.errorlimit)
                    break;
            } while (true);
            train.FinishTraining();
            SaveNetworkToFile(network, parm.name);

            return network;
        }
        public BasicNetwork CreateNetwork02(TestData data, NetworkParm parm)
        {
            LogPrintf("[NetworkCreate]" + parm.ToString());
            ResultPrintf("[NetworkCreate]" + parm.ToString());
            // create a network
            BasicNetwork network = EncogUtility.SimpleFeedForward(
                data.InputSize,
                (int)(data.InputSize * parm.hidenLayerRaio),
                (int)(data.OutputSize * 10),
                data.OutputSize,
                true);
            // create training data
            IMLDataSet trainingSet = new BasicMLDataSet(data.TrainInputs, data.TrainIdeaOutputs);

            // train the neural network
            // var train = new Backpropagation(network, trainingSet);
            var train = new ResilientPropagation(network, trainingSet);
            train.ThreadCount = 8;

            int epoch = 1;
            SlidingFilter filter = new SlidingFilter(5);
            do
            {
                train.Iteration();
                // if(epoch%1 == 1 )
                LogPrintf(@"Epoch #" + epoch + @" Error:" + train.Error);
                // Save network each train.
                SaveNetworkToFile(network, parm.name);
                epoch++;
                if (epoch > parm.retryCnt)
                    break;
                if (filter.Add(train.Error) < parm.errorlimit)
                    break;
            } while (true);
            train.FinishTraining();
            SaveNetworkToFile(network, parm.name);

            return network;
        }

        // Save network class to file.
        private void SaveNetworkToFile(BasicNetwork network, string filename)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(filename+".net", FileMode.Create);
            formatter.Serialize(stream, network);
            stream.Flush();
            stream.Close();
        }
        private BasicNetwork LoadNetwrokFromFile(string filename)
        {
            if (File.Exists(networkFileName) == false)
                return null;

            BasicNetwork network;
            BinaryFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(filename+".net", FileMode.Open);
            network = (BasicNetwork)formatter.Deserialize(stream);
            stream.Close();

            return network;
        }
        private void LogPrintf(string str)
        {
            logger.WriteLine(str);
            LogFile.WriteLine(str);
        }
        private void ResultPrintf(string str)
        {
            resultLog.WriteLine(str);
        }

    }
    class SlidingFilter
    {
        private double[] _dataArr;
        private int _length;
        private int _index;
        private bool _isInit;
        public SlidingFilter(int cnt)
        {
            _length = cnt;
            _index = 0;
            _dataArr = new double[cnt];
            _isInit = false;
        }
        public double Add(double value)
        {
            if (false == _isInit)
            {
                for (int i = 0; i < _length; i++)
                    _dataArr[i] = value;
                _isInit = true;
            }
            else
            {
                _dataArr[_index] = value;
            }
            if (++_index >= _length)
                _index = 0;

            return _dataArr.Sum() / _length;
        }
    }
    class LogFile
    {
        public delegate void WriteLineFunction(string str);
        public static List<WriteLineFunction> FuncList;
        static LogFile()
        {
            FuncList = new List<WriteLineFunction>();
        }

        public static void WriteLine(string str)
        {
            foreach (WriteLineFunction func in FuncList)
            {
                func(str);
            }
        }
    }
    public class NetworkParm
    {
        public string name;
        public double errorlimit = 0.001;
        public double hidenLayerRaio = 10000;
        public int retryCnt;

        public NetworkParm(string name, double error, double hidenLayerRaio, int retryCnt)
        {
            this.name = name;
            this.errorlimit = error;
            this.hidenLayerRaio = hidenLayerRaio;
            this.retryCnt = retryCnt;
        }

        public override string ToString()
        {
            return "Err:" + errorlimit.ToString("G2") + " NeroRaio:" + hidenLayerRaio.ToString();
        }
    }
    public class TestData
    {
        public MyTestDataList TrainList;
        public MyTestDataList TestList;
        public TestData(MyTestDataList trainList, MyTestDataList testList)
        {
            TrainList = trainList;
            TestList = testList;
        }

        public int InputSize
        {
            get
            {
                return TrainList.InputSize;
            }
        }
        public int OutputSize
        {
            get
            {
                return TrainList.OutputSize;
            }
        }

        public double[][] TrainInputs
        {
            get
            {
                return TrainList.Inputs;
            }
        }
        public double[][] TrainIdeaOutputs
        {
            get
            {
                return TrainList.Ideals;
            }
        }
        public double[][] TrainRealOutputs
        {
            get
            {
                return TrainList.Reals;
            }
        }

        public double[][] TestInputs
        {
            get
            {
                return TestList.Inputs;
            }
        }
        public double[][] TestIdeaOutputs
        {
            get
            {
                return TestList.Ideals;
            }
        }
        public double[][] TestRealOutputs
        {
            get
            {
                return TestList.Reals;
            }
        }

        public string ToStringResults()
        {
            string str = "";
            str += "Tain data:\r\n";
            str += TrainList.ResultToString();
            str += "Test data:\r\n";
            str += TestList.ResultToString();
            return str;
        }
    }

    class RateDataCreator
    {
        private const string _dataFile = "data.csv";
        private DataLoader _loader;

        private const int inputCount = 14;
        private const int outputCount = 1;
        private const int testSetCount = 20;
        private const int dataInv = 1;

        public RateDataCreator()
        {
            _loader = new DataLoader(_dataFile);
        }
        public TestData GetTestData()
        {

            // check data is enough
            int inputCount = RateDataCreator.inputCount;
            int outputCount = RateDataCreator.outputCount;
            int dataInv = RateDataCreator.dataInv;
            int testSetCount = RateDataCreator.testSetCount;
            int setCount = (_loader.Count - inputCount - outputCount + dataInv)/dataInv;
            int trainSetCount = setCount - testSetCount;
            
            if( trainSetCount < 0 )
                return null;

            MyTestDataList trainList = new MyTestDataList();
            MyTestDataList testList = new MyTestDataList();

            double[][] inputs = new double[trainSetCount][];
            double[][] outputs = new double[trainSetCount][];
            double[][] testInputs = new double[testSetCount][];
            double[][] testOutputs = new double[testSetCount][];
            int index = 0;
            MyTestData dataObj;
            // Create train data sets.
            for(int i=0;i<trainSetCount;i++)
            {
                dataObj = new MyTestData();
                dataObj.Input = _loader.GetArr(index, inputCount);
                dataObj.Ideal = _loader.GetArr(index + inputCount, outputCount);
                index += dataInv;
                trainList.Add(dataObj);
            }

            // Create test data sets.
            for(int i=0;i<testSetCount;i++)
            {
                dataObj = new MyTestData();
                dataObj.Input = _loader.GetArr(index, inputCount);
                dataObj.Ideal = _loader.GetArr(index + inputCount, outputCount);
                index += dataInv;
                testList.Add(dataObj);
            }

            TestData data = new TestData(trainList, testList);
            return data;
        }
        public RateSet GetData(int index)
        {
            return _loader[index].Clone();
        }
        public double[] GetData(int startIndex, int length)
        {
            return _loader.GetArr(startIndex, length);
        }
        

    }

}
