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

namespace MyProject01
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            /*
            XORHelloWorld obj = new XORHelloWorld();
            obj.Execute();
            */

            /*
            DataLoader loader = new DataLoader("data.csv");

            foreach (RateSet rateSet in loader)
            {
                printf(rateSet.Date.ToShortDateString() + ": " + rateSet.Value.ToString() + "\r\n");
            }
            */

            XORHelloWorld test = new XORHelloWorld();
            test.Execute();
            printf("=== End!\r\n");

        }
        private void printf(string str)
        {
            System.Console.Write(str);
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
            const int dataLenth = 10;
            const int dataInv = 1;
            const int dataSetNum = 15;
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
            network.AddLayer(new BasicLayer(new ActivationSigmoid(), true, dataLenth*3));
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
            IMLTrain train = new ResilientPropagation(network, trainingSet);

            int epoch = 1;

            do
            {
                train.Iteration();
                // if(epoch%1 == 1 )
                    Console.WriteLine(@"Epoch #" + epoch + @" Error:" + train.Error);
                epoch++;
                if (epoch > 1000)
                    break;
            } while (train.Error > 0.01);

            // test the neural network
            Console.WriteLine(@"Neural Network Results:");
            int loopCnt = 1;
            foreach (IMLDataPair pair in dataSet)
            {
                IMLData res = network.Compute(pair.Input);
                Console.WriteLine(loopCnt.ToString("d2")+ @": actual=" + res[0] + 
                    @",ideal=" + pair.Ideal[0] + ",radio=" + (Math.Abs(res[0]-pair.Ideal[0])/pair.Ideal[0]*100));
                loopCnt++;
            }
        }

        #endregion
    }
    class LogFile
    {
        static string _logFileName = "log.txt";
        static private bool isStart = false;

        static private void Init()
        {
            
        }
        static public void Printf(string str)
        {

           

        }

    }
    class RateDataCreator
    {
        private const string _dataFile = "data.csv";
        private DataLoader _loader;
        private int _dateLength = 0;
        public RateDataCreator(int dateLength)
        {
            _loader = new DataLoader(_dataFile);
            _dateLength = dateLength;
        }
        public double[][] GetData(DateTime date)
        {
            double[][] dataArrContainer= new double[2][];
            dataArrContainer[0] = _loader.GetArr(date, _dateLength);
            dataArrContainer[1] = _loader.GetArr(date.AddDays(_dateLength));
            return dataArrContainer;
        }
        

    }

}
