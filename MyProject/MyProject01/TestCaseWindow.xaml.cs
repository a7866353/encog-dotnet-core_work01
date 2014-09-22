using MyProject01.Util;
using MyProject01.Util.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MyProject01.TestCases;
using Encog.ML.Data;
using Encog.ML.Data.Basic;
using Encog.Neural.NEAT;
using Encog.Neural.Networks.Training;
using Encog.ML.EA.Train;
using Encog.Util.Simple;
using Encog.Util.Banchmark;
using Encog.Util;

namespace MyProject01
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class TestCaseWindow : Window
    {
        private TestCaseObject[] TestCaseArray;
        private delegate void Func();

        public TestCaseWindow()
        {
            InitializeComponent();

            this.Closing += TestCaseWindow_Closing;

            TestCaseArray = new TestCaseObject[]
            {
                new TestCaseObject("TestRateMarketAgent", "", new TestCaseObject.TestFucntion(TestRateMarketAgent)),
                new TestCaseObject("TestAnn", "", new TestCaseObject.TestFucntion(TestANN)),
                new TestCaseObject("TestMarketAnalyz", "", new TestCaseObject.TestFucntion(TestMarketAnalyz)),
                new TestCaseObject("RateAnalyzeTest", "", new TestCaseObject.TestFucntion(RateAnalyzeTest)),
                new TestCaseObject("TestNEATNet", "", new TestCaseObject.TestFucntion(TestNEATNet)),
                new TestCaseObject("SampleCase", "", new TestCaseObject.TestFucntion(TestCase01)),
            };

            ;
            foreach( TestCaseObject obj in TestCaseArray)
            {
                Border border = new Border();
                border.BorderThickness = new Thickness(2, 5, 2, 5);
                border.BorderBrush = Brushes.Black;

                Button testButton = new Button();
                testButton.Height = 30;
                testButton.Content = obj.Name;
                testButton.Click += new RoutedEventHandler(delegate(object sender, RoutedEventArgs e)
                    {
                        MainWindow mainWin = new MainWindow(obj);
                        mainWin.Show();
                    });
                border.Child = testButton;
                MainStackPanel.Children.Add(border);
            }

        }

        void TestCaseWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            App.Current.Shutdown();
        }

      

        private void TestCase01()
        {
            MessageBox.Show("01");
        }

        private void TestMarketAnalyz()
        {
            DataLoader loader = new DataLoader();
            MarketRateAnalyzer analyzer = new MarketRateAnalyzer(loader.ToArray());
            DealPointInfomation[] info = analyzer.GetDealInfo();
        }

        private void RateAnalyzeTest()
        {
            GraphViewer win;
            this.Dispatcher.Invoke(new Func(delegate()
            {
                win = new GraphViewer();
                win.Show();
            }));

            DataLoader dataLoader = new DataLoader();
            MarketRateAnalyzer test = new MarketRateAnalyzer(dataLoader.ToArray());
            test.GetDealInfo();


        }

        private void TestANN()
        {
            // NetworkTest test;
            //  test = new FeedForwardNetworkTest();
            //  test = new ElmanNetworkTest();
            // RateAnalyzeTest();
            // test.Run();

            TestCaseFactory testCaseFactory = new TestCaseFactory();
            BasicTestCase[] testCaseArr = testCaseFactory.GetTestCases();
            Parallel.ForEach(testCaseArr, (testCase, loopState) =>
            {
                try
                {
                    testCase.RunTest();
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.ToString());
                }
            });
        }

        private void TestRateMarketAgent()
        {
            RateMarketTest test = new RateMarketTest();
            test.RunTest();
        }

        private void TestNEATNet()
        {
            double errorLimit = 0.01;

            IMLDataSet trainingSet = RandomTrainingFactory.Generate(1000, 1500,
                                         30, 3, -1, 1);

            double targetErrorLimit = 0;
            for (int i = 0; i < trainingSet[0].Ideal.Count; i++)
                targetErrorLimit += Math.Abs(trainingSet[0].Ideal[i]);
            targetErrorLimit /= trainingSet[0].Ideal.Count;
            targetErrorLimit *= errorLimit;

            NEATPopulation pop = new NEATPopulation(2, 1, 10000);
            pop.Reset();
            pop.InitialConnectionDensity = 1.0; // not required, but speeds processing.
            ICalculateScore score = new TrainingSetScore(trainingSet);
            // train the neural network
            TrainEA train = NEATUtil.ConstructNEATTrainer(pop,score);
           
            int epoch = 1;
            LogFile.WriteLine(@"Beginning training...");
            do
            {
                train.Iteration();

                LogFile.WriteLine(@"Iteration #" + Format.FormatInteger(epoch)
                         + @" Error:" + Format.FormatPercent(train.Error)
                         + @" Target Error: " + Format.FormatPercent(targetErrorLimit));
                epoch++;
            } while ((train.Error > targetErrorLimit) && !train.TrainingDone);
            train.FinishTraining();


            NEATNetwork network = (NEATNetwork)train.CODEC.Decode(train.BestGenome);

            // test the neural network
            LogFile.WriteLine(@"Neural Network Results:");
            EncogUtility.Evaluate(network, trainingSet);

        }

    }

    public class TestCaseObject
    {
        public delegate void TestFucntion();
        public string Name;
        public String Description;
        public TestFucntion TestFunction;

        public TestCaseObject(string name, string description, TestFucntion function)
        {
            this.Name = name;
            this.Description = description;
            this.TestFunction = function;
        }
    }


}
