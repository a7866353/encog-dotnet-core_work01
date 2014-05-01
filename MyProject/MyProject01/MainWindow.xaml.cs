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
using Encog.ML.Train;
using System.Diagnostics;
using System.Threading;
using MyProject01.Util;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

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
        }

        private void TestANN()
        {
            XORHelloWorld test = new XORHelloWorld();
            test.Execute();
        }
    }
    

    public class XORHelloWorld
    {
        private LogWriter logger;
        private LogWriter resultLog;
        private const string networkFileName = "network.bin";

        public XORHelloWorld()
        {
            // init log
            logger = new LogWriter("log.txt");
            resultLog = new LogWriter("Results.txt");

        }


        /// <summary>
        /// Program entry point.
        /// </summary>
        /// <param name="app">Holds arguments and other info.</param>
        public void Execute()
        {
            ResultPrintf(@"------------------------");
            LogPrintf("Test Start!");

            RateDataCreator dataCreator = new RateDataCreator();
            TestData data = dataCreator.GetTestData();

            BasicNetwork network = null;
            if( false )
            {
                // Create a new trained netwrok
                network = CreateNetwork(data);
            }
            else
            {
                // Create network from file.
                network = LoadNetwrokFromFile();
            }

            if (null == network)
                throw (new Exception("Empty Network."));

            // test the neural network
            //   test train data
            data.trainRealOutputs = new double[data.trainInputs.Length][];
            for(int i=0;i<data.trainInputs.Length;i++)
            {
                IMLData res = network.Compute(new BasicMLData(data.trainInputs[i]));
                double[] realArr = new double[data.outputCount];
                res.CopyTo(realArr, 0, data.outputCount);
                data.trainRealOutputs[i] = realArr;
            }

            //   test test data
            double[][] ideals = new double[data.trainInputs.Length][];
            data.testRealOutputs = new double[data.testInputs.Length][];
            for(int i=0;i<data.testRealOutputs.Length;i++)
            {
                IMLData res = network.Compute(new BasicMLData(data.trainInputs[i]));
                double[] realArr = new double[data.outputCount];
                res.CopyTo(realArr, 0, data.outputCount);
                data.testRealOutputs[i] = realArr;
            }


            // Output results.
            int loopCnt = 1;
            ResultPrintf(@"------------------------");
            ResultPrintf(@"Neural Network Results:");
            ResultPrintf("Tain data:");
            for (int i = 0; i < data.trainInputs.Length; i++)
            {
                string str = "";
                str += loopCnt.ToString("d2");
                str += ": =";
                for (int j = 0; j < data.outputCount; j++)
                {
                    str += ((data.trainRealOutputs[i][j] - data.trainIdeaOutputs[i][j]) / data.trainIdeaOutputs[i][j] * 100).ToString("000.0000");
                    str += ",\t";
                }
                ResultPrintf(str);
                loopCnt++;
               
            }
            loopCnt = 1;
            ResultPrintf("Test data:");
            for (int i = 0; i < data.testInputs.Length; i++)
            {
                string str = "";
                str += loopCnt.ToString("d2");
                str += ": =";
                for (int j = 0; j < data.outputCount; j++)
                {
                    str += ((data.testRealOutputs[i][j] - data.testIdeaOutputs[i][j]) / data.testIdeaOutputs[i][j] * 100).ToString("000.0000");
                    str += ",\t";
                }
                ResultPrintf(str);
                loopCnt++;

            }

            LogPrintf("Test end!");
            LogPrintf("");
        }

        public TestData CreateTestData()
        {
            RateDataCreator dataCreator = new RateDataCreator();
            TestData data = dataCreator.GetTestData();
            return data;
        }

        public BasicNetwork CreateNetwork(TestData data)
        {
            // create a neural network, without using a factory
            var network = new BasicNetwork();
            network.AddLayer(new BasicLayer(null, true, data.inputCount));
            network.AddLayer(new BasicLayer(new ActivationSigmoid(), true, (int)(data.inputCount * 2)));
            network.AddLayer(new BasicLayer(new ActivationSigmoid(), true, data.outputCount));
            network.Structure.FinalizeStructure();
            network.Reset();

            // create training data
            IMLDataSet trainingSet = new BasicMLDataSet(data.trainInputs, data.trainIdeaOutputs);

            // train the neural network
            var train = new ResilientPropagation(network, trainingSet);
            train.ThreadCount = 8;

            int epoch = 1;

            do
            {
                train.Iteration();
                // if(epoch%1 == 1 )
                LogPrintf(@"Epoch #" + epoch + @" Error:" + train.Error);
                // Save network each train.
                SaveNetworkToFile(network);
                epoch++;
                if (epoch > 1000)
                    break;
            } while (train.Error > 0.03);

            return network;
        }

        // Save network class to file.
        private void SaveNetworkToFile(BasicNetwork network)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(networkFileName, FileMode.Create);
            formatter.Serialize(stream, network);
            stream.Flush();
            stream.Close();
        }
        private BasicNetwork LoadNetwrokFromFile()
        {
            if (File.Exists(networkFileName) == false)
                return null;

            BasicNetwork network;
            BinaryFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(networkFileName, FileMode.Open);
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
    public class TestData
    {
        public bool isSet = false;
        public int inputCount;
        public int outputCount;

        public double[][] trainInputs;
        public double[][] trainIdeaOutputs;
        public double[][] trainRealOutputs;

        public double[][] testInputs;
        public double[][] testIdeaOutputs;
        public double[][] testRealOutputs;

        
    }
    class RateDataCreator
    {
        private const string _dataFile = "data.csv";
        private DataLoader _loader;

        private const int inputCount = 14;
        private const int outputCount = 4;
        private const int testSetCount = 5;
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

            double[][] inputs = new double[trainSetCount][];
            double[][] outputs = new double[trainSetCount][];
            double[][] testInputs = new double[testSetCount][];
            double[][] testOutputs = new double[testSetCount][];
            int index = 0;

            // Create train data sets.
            for(int i=0;i<trainSetCount;i++)
            {
                inputs[i] = _loader.GetArr(index, inputCount);
                outputs[i] = _loader.GetArr(index + inputCount, outputCount);
                index += dataInv;
            }

            // Create test data sets.
            for(int i=0;i<testSetCount;i++)
            {
                testInputs[i] = _loader.GetArr(index, inputCount);
                testOutputs[i] = _loader.GetArr(index + inputCount, outputCount);
                index += dataInv;
            }

            TestData data = new TestData();
            data.isSet = false;
            data.inputCount = inputCount;
            data.outputCount = outputCount;
            data.trainInputs = inputs;
            data.trainIdeaOutputs = outputs;
            data.testInputs = testInputs;
            data.testIdeaOutputs = testOutputs;

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
