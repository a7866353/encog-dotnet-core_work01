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
            OutputTextBox.Background = Brushes.Black;

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
            // init log
            logger = new LogWriter("log.txt");
            resultLog = new LogWriter("Results.txt");

            //
            const int dataLenth = 10;
            const int dataInv = 1;
            const int dataSetNum = 19;
            const int trainNum = 10;
            double[][] input = new double[dataSetNum][];
            double[][] output = new double[dataSetNum][];
            RateDataCreator dataCreator = new RateDataCreator(dataLenth);
            DateTime startDate = DateTime.Parse("2013/4/20");
            DateTime currDate = startDate;
            for (int i = 0; i < dataSetNum; i++)
            {
                double[][] res = dataCreator.GetData(currDate);
                input[i] = res[0];
                output[i] = res[1];

                currDate = currDate.AddDays(dataInv);
            }

            

            // create a neural network, without using a factory
            var network = new BasicNetwork();
            network.AddLayer(new BasicLayer(null, true, dataLenth));
            network.AddLayer(new BasicLayer(new ActivationSigmoid(), true, (int)(dataLenth*10000)));
            network.AddLayer(new BasicLayer(new ActivationSigmoid(), true, 1));
            network.Structure.FinalizeStructure();
            network.Reset();

            // create training data
            IMLDataSet dataSet = new BasicMLDataSet(input, output);
            double[][] trainInput = new double[trainNum][];
            double[][] trainOutput = new double[trainNum][];
            Array.Copy(input, trainInput, trainNum);
            Array.Copy(output, trainOutput, trainNum);
            IMLDataSet trainingSet = new BasicMLDataSet(trainInput, trainOutput);

            // train the neural network
            var train = new ResilientPropagation(network, trainingSet);
            train.ThreadCount = 16;

            int epoch = 8;

            do
            {
                train.Iteration();
                // if(epoch%1 == 1 )
                    LogPrintf(@"Epoch #" + epoch + @" Error:" + train.Error);
                epoch++;
                if (epoch > 1000)
                    break;
            } while (train.Error > 0.005);

            // test the neural network
            ResultPrintf(@"Neural Network Results:");
            int loopCnt = 1;
            foreach (IMLDataPair pair in dataSet)
            {
                IMLData res = network.Compute(pair.Input);
                ResultPrintf(loopCnt.ToString("d2") + @": actual=" + res[0] + 
                    @",ideal=" + pair.Ideal[0] + ",radio=" + (Math.Abs(res[0]-pair.Ideal[0])/pair.Ideal[0]*100));
                loopCnt++;
            }

            LogPrintf("Test end!");
            LogPrintf("");
        }

        #endregion
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
        static string _logFileName = "log.txt";
        static private bool isStart = false;
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
    class RateDataCreator
    {
        private const string _dataFile = "data.csv";
        private DataLoader _loader;
        private int _dateLength = 0;
        private int index = 0;
        public RateDataCreator()
        {
            _loader = new DataLoader(_dataFile);
            Reset();
        }
        public void Reset()
        {
            index = 0;
        }
        public
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
