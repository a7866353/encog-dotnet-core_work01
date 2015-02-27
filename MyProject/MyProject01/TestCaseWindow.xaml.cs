﻿using MyProject01.Util;
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
using MyProject01.TrainingMethods;
using MyProject01.Networks;
using MyProject01.Test;
using Encog.Neural.Networks;
using MyProject01.DAO;
using MyProject01.Win;
using MyProject01.Util.DllTools;

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
                new TestCaseObject("FWT_Cuda_Test", "", new TestCaseObject.TestFucntion(FWT_Cuda_Test)),
                new TestCaseObject("FWT_5min_Long", "", new TestCaseObject.TestFucntion(FWT_5min_Long)),
                new TestCaseObject("FWT_5min_Simple", "", new TestCaseObject.TestFucntion(FWT_5min_Simple)),
                new TestCaseObject("TestFWT", "", new TestCaseObject.TestFucntion(TestFWT)),
                new TestCaseObject("Long5Min_Simple", "", new TestCaseObject.TestFucntion(Long5Min_Simple)),
                new TestCaseObject("Test_5Min_Short", "", new TestCaseObject.TestFucntion(Test_5Min_Short)),
                new TestCaseObject("Test_1Min_Short", "", new TestCaseObject.TestFucntion(Test_1Min_Short)),
                new TestCaseObject("Test_1Min_Long", "", new TestCaseObject.TestFucntion(Test_1Min_Long)),
                new TestCaseObject("TestRateMarketNEAT_Long", "", new TestCaseObject.TestFucntion(TestRateMarketNEAT_Long)),
                new TestCaseObject("TestRateMarketNEAT_Short", "", new TestCaseObject.TestFucntion(TestRateMarketNEAT_Short)),
                new TestCaseObject("TestRateMarketNEAT", "", new TestCaseObject.TestFucntion(TestRateMarketNEAT)),
                new TestCaseObject("TestDataBaseViewer", "", new TestCaseObject.TestFucntion(TestDataBaseViewer)),
                new TestCaseObject("TestDAO", "", new TestCaseObject.TestFucntion(TestDAO)),
                new TestCaseObject("TestRateMarketNEATBatch", "", new TestCaseObject.TestFucntion(TestRateMarketNEATBatch)),
                new TestCaseObject("TestRateMarketAgent", "", new TestCaseObject.TestFucntion(TestRateMarketAgent)),
                new TestCaseObject("TestAnn", "", new TestCaseObject.TestFucntion(TestANN)),
                new TestCaseObject("TestMarketAnalyz", "", new TestCaseObject.TestFucntion(TestMarketAnalyz)),
                new TestCaseObject("RateAnalyzeTest", "", new TestCaseObject.TestFucntion(RateAnalyzeTest)),
                new TestCaseObject("TestNEATNet", "", new TestCaseObject.TestFucntion(TestNEATNet)),
                new TestCaseObject("TestBPTrain", "", new TestCaseObject.TestFucntion(TestBPTrain)),
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
                        mainWin.Title = GetTestName();
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

        private string GetTestName()
        {
            string name = null;
            this.Dispatcher.Invoke(new Func(delegate()
            {
                name = TestNameTextBox.Text;
            }));
            
            if (string.IsNullOrWhiteSpace(name) == true)
                return "DefaultTest000";
            else
                return name;
        }

        private void TestCase01()
        {
            MessageBox.Show("01");
        }

        private void TestMarketAnalyz()
        {
            DataLoader loader = new FenghuangDataLoader();
            MarketRateAnalyzer analyzer = new MarketRateAnalyzer(loader.ToArray());
            DealPointInfomation[] info = analyzer.GetDealInfo();
        }

        private void RateAnalyzeTest()
        {
            GraphViewer win;
            this.Dispatcher.BeginInvoke(new Func(delegate()
            {
                win = new GraphViewer();
                win.Show();
                DataLoader dataLoader = new FenghuangDataLoader();
                MarketRateAnalyzer test = new MarketRateAnalyzer(dataLoader.ToArray());
                test.GetDealInfo();
            }));



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
            RateMarketQLearnTest test = new RateMarketQLearnTest();
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

            NEATPopulation pop = new NEATPopulation(30, 3, 100);
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

        private void TestBPTrain()
        {
            NetworkTestParameter parm = new NetworkTestParameter("QLearn", 0.5, 10, 10);
            parm.InputSize = 30;
            parm.OutputSize = 3;
            FeedForwardNet net = new FeedForwardNet();
            BasicNetwork network = net.GetNet(parm);
            BackpropagationTraining method = new BackpropagationTraining();
            method.ErrorChangeLimit = 0.0000001;
            method.ErrorChangeTryMaxCount = 10;
            IMLDataSet trainingSet = RandomTrainingFactory.Generate(1000, 1500,
                                         parm.InputSize, parm.OutputSize, -1, 1);

            LogFile.WriteLine(@"Beginning training...");
            method.TrainNetwork(network, trainingSet);
            LogFile.WriteLine(@"Neural Network Results:");

        }
        private void TestRateMarketNEAT()
        {
            RateMarketNEATTest test = new RateMarketNEATTest();
            test.TestName = GetTestName();
            test.TestDataRate = 0.8;
            test.PopulationNum = 1000;
            test.DataBlockLength = 2880;
            test.RunTest();
        }

        private void TestRateMarketNEAT_Short()
        {
            string prefix = "Short_";
            RateMarketNEATTest test = new RateMarketNEATTest();
            test.TestName = prefix + GetTestName();
            test.TestDataRate = 0.5;
            test.PopulationNum = 500;
            test.DataBlockLength = 12*4;
            test.DataSoreceType = 1;
            test.RateDataControllerName = "test01";
            test.RunTest();
        }
        private void TestRateMarketNEAT_Long()
        {
            string prefix = "Long_";
            RateMarketNEATTest test = new RateMarketNEATTest();
            test.TestName = prefix + GetTestName();
            test.TestDataRate = 0.9;
            test.PopulationNum = 500;
            test.DataBlockLength = 300;
            test.DataSoreceType = 1;
            test.RunTest();
        }
        private void Test_1Min_Long()
        {
            string prefix = "1Min_Long_";
            RateMarketNEATTest test = new RateMarketNEATTest();
            test.TestName = prefix + GetTestName();
            test.TestDataRate = 0.5;
            test.PopulationNum = 1000;
            test.DataBlockLength = 60 * 24;
            test.RateDataControllerName = "test02";
            test.DataSoreceType = 1;
            test.RunTest();
        }
        private void Test_1Min_Short()
        {
            string prefix = "1Min_Short_";
            RateMarketNEATTest test = new RateMarketNEATTest();
            test.TestName = prefix + GetTestName();
            test.TestDataRate = 0.5;
            test.PopulationNum = 1000;
            test.DataBlockLength = 60 * 3;
            test.RateDataControllerName = "test02";
            test.DataSoreceType = 1;
            test.RunTest();
        }
        private void Test_5Min_Short()
        {
            string prefix = "5Min_Short_";
            RateMarketNEATTest test = new RateMarketNEATTest();
            test.TestName = prefix + GetTestName();
            test.TestDataRate = 0.5;
            test.PopulationNum = 1000;
            test.DataBlockLength = 12 * 3;
            test.RateDataControllerName = "test01";
            test.DataSoreceType = 1;
            test.RunTest();
        }
        private void Long5Min_Simple()
        {
            string prefix = "Long5Min_Simple_";
            RateMarketNEATTest test = new RateMarketNEATTest();
            test.TestName = prefix + GetTestName();
            test.TestDataRate = 0.7;
            test.PopulationNum = 500;
            test.DataBlockLength = 12 * 2;
            test.RateDataControllerName = "test01";
            test.DataSoreceType = 0;
            test.RunTest();
        }
        private void TestRateMarketNEATBatch()
        {
            RateMarketNEATBatchTest test = new RateMarketNEATBatchTest();
            test.TestName = GetTestName();
            test.RunTest();
        }

        private void TestDAO()
        {
            string testName = "QLearn01";
            if (RateMarketTestDAO.IsExist(testName) == true)
                RateMarketTestDAO.Remove(testName);
            RateMarketTestDAO dao = RateMarketTestDAO.GetDAO<RateMarketTestDAO>(testName);


            NetworkTestParameter parm = new NetworkTestParameter("QLearn", 0.5, 2, 10);
            // network = new MyNet(new FeedForwardNet(), new ResilientPropagationTraining(), parm);
            parm.MaxTryCount = 1000;
            MyNet network = new MyNet(new FeedForwardNet(), new BackpropagationTraining(), parm);
            network.Init(30, 3);

            dao.NetworkData = network.NetworkToByte();
            // TODO
            // dao.NetworkParamter = network.parm;

            dao.Save();

            RateMarketTestEpisodeDAO episodeDAO;

            for (int i = 0; i < 100; i++)
            {
                episodeDAO = new RateMarketTestEpisodeDAO();
                episodeDAO.Step = i;
                dao.AddEpisode(episodeDAO);
            }
        // Class end
        //---------------------------------


        }

        private void TestFWT()
        {
            int len = 512;
            double[] input = new double[len];
            double[] output = new double[len];
            double[] temp = new double[len];

            // Generate test data
            int f1 = 5;
            int f2 = 10;
            int f0 = 320;
            for(int i=0;i<input.Length;i++)
            {
                if(i<input.Length/2)
                 {
                    input[i] = Math.Sin(i * 2 * Math.PI * f1 / f0);
                }
                else
                {
                    input[i] = Math.Sin(i * 2 * Math.PI * f2 / f0);
                }
            }
            DllTools.FTW(input, output, temp);
        }
        private void FWT_5min_Simple()
        {
            string prefix = "FWT_5min_Simple_";
            RateMarketNEATTest test = new RateMarketNEATTest();
            test.TestName = prefix + GetTestName();
            test.TestDataRate = 0.4;
            test.PopulationNum = 100;
            test.DataBlockLength = 32;
            test.RateDataControllerName = "test03";
            test.DataSoreceType = 0;
            test.IsFWT = true;
            test.RunTest();
        }
        private void FWT_5min_Long()
        {
            string prefix = "FWT_5min_Long_";
            RateMarketNEATTest test = new RateMarketNEATTest();
            test.TestName = prefix + GetTestName();
            test.TestDataRate = 0.4;
            test.PopulationNum = 100;
            test.DataBlockLength = 2048;
            test.RateDataControllerName = "test01";
            test.DataSoreceType = 0;
            test.IsFWT = true;
            test.RunTest();
        }
        private void FWT_Cuda_Test()
        {
            // double[] input = new double[] {9, 7, 3, 5};
            double[] input = new double[] { 1, 2, 3, 4 };
            double[] result = new double[] {6, 2, 1, -1};
            double[] output = new double[input.Length];
            double[] temp = new double[input.Length];
       
            DllTools.FTW_2(input, output, temp);
        }

        

        private void TestDataBaseViewer()
        {
            this.Dispatcher.BeginInvoke(new Func(delegate()
            {
                DataBaseViewer win = new DataBaseViewer();
                win.Show();
            }));
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
