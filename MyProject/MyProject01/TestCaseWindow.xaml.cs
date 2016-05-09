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
using MyProject01.TrainingMethods;
using MyProject01.Networks;
using MyProject01.Test;
using Encog.Neural.Networks;
using MyProject01.DAO;
using MyProject01.Win;
using MyProject01.Util.DllTools;
using MyProject01.Controller;
using MyProject01.TestCases.RateMarketTestCases;
using System.Threading;

namespace MyProject01
{
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

    class TestCaseGroup : List<TestCaseObject>
    {
        public void Add(TestCaseGroup group)
        {
            foreach (TestCaseObject obj in group)
                Add(obj);
        }

        public void Add(BasicTestCase testCase)
        {
            TestCaseObject obj = new TestCaseObject(testCase.TestName, testCase.TestDescription, new TestCaseObject.TestFucntion(testCase.RunTest));
            Add(obj);
        }
        public void Add(BasicNewTestCase testCase)
        {
            TestCaseObject obj = new TestCaseObject(testCase.TestCaseName, "", new TestCaseObject.TestFucntion(testCase.Run));
            Add(obj);
        }

    }
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class TestCaseWindow : Window
    {
        private TestCaseGroup TestCaseList;
        private delegate void Func();

        public TestCaseWindow()
        {
            InitializeComponent();
            this.Loaded += TestCaseWindow_Loaded;

        }

        void TestCaseWindow_Loaded(object sender0, RoutedEventArgs e0)
        {

            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.Idle;
            this.Closing += TestCaseWindow_Closing;
            TestCaseList = new TestCaseGroup();
            // StartAllButton.Click += StartAllButton_Click;
            AddTestCase();
            int i = 0;

            foreach (TestCaseObject obj in TestCaseList)
            {
                string displayName = "[" + i.ToString("D2") + "]" + obj.Name + ": " + obj.Description;
                i++;
                Border border = new Border();
                border.BorderThickness = new Thickness(4, 1, 4, 1);
                border.BorderBrush = Brushes.Black;

                Button testButton = new Button();
                testButton.Height = 20;
                testButton.Content = displayName;
                testButton.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left;
                testButton.Click += new RoutedEventHandler(delegate(object sender, RoutedEventArgs e)
                {
                    MainWindow mainWin = new MainWindow(obj);
                    mainWin.Title = DateTime.Now.ToString() + ": " + displayName;
                    mainWin.Closed += new EventHandler(
                            delegate(object sender2, EventArgs args)
                            {
                                Application.Current.Shutdown();
                            }
                        );
                    mainWin.Show();
                });
                border.Child = testButton;
                MainStackPanel.Children.Add(border);
            }

            InitParamConfig();
        }
        private void InitParamConfig()
        {
            //-------------------------------
            AddParamConfigUI("ParameterConfigure:", null);


            // ServerIP
            //------------------------
#if false
            TextBox ipTb = new TextBox();
            ipTb.Text = CommonConfig.ServerIP;
            ipTb.TextChanged += new TextChangedEventHandler(
                    delegate(object sender, TextChangedEventArgs args)
                    {
                        CommonConfig.ServerIP = ipTb.Text;
                    }
                );
            AddParamConfigUI("Server IP Address", ipTb);
#endif
            StackPanel ipPanel = new StackPanel();
            foreach(ServerIPParam param in ServerIPParamList.IPs)
            {
                RadioButton rb = new RadioButton();
                rb.GroupName = "IPParam";
                rb.Content = param.IP;
                rb.Checked += new RoutedEventHandler(
                        delegate(object sender, RoutedEventArgs args)
                        {
                            CommonConfig.ServerIP = param.IP;
                            DataBaseAddress.SetIP(CommonConfig.ServerIP);
                        }
                    );
                ipPanel.Children.Add(rb);
                
                if (param.IsDefault == true)
                    rb.IsChecked = true;
            }
            AddParamConfigUI("Server IP Address", ipPanel);


            // PopulationSize
            //---------------------------
            TextBox popSizeTb = new TextBox();
            popSizeTb.Text = CommonConfig.PopulationSize.ToString();
            popSizeTb.TextChanged += new TextChangedEventHandler(
                    delegate(object sender, TextChangedEventArgs args)
                    {
                        int result;
                        if (int.TryParse(popSizeTb.Text, out result) == false)
                            return;
                        CommonConfig.PopulationSize = result;
                    }
                );
            AddParamConfigUI("Populaton Size", popSizeTb);

            // LoaderParam
            //------------------------
            StackPanel loaderPanel = new StackPanel();
            foreach(DataLoaderParam parm in DataLoaderParamList.GetParams())
            {
                RadioButton loaderParamRb = new RadioButton();
                loaderParamRb.GroupName = "LoaderParam";
                loaderParamRb.Content = parm.ToString();
                loaderParamRb.Checked += new RoutedEventHandler(
                        delegate(object sender, RoutedEventArgs args)
                        {
                            CommonConfig.LoaderParam = parm;
                        }
                    );
                loaderPanel.Children.Add(loaderParamRb);
                if (parm.IsDefault == true)
                    loaderParamRb.IsChecked = true;
            }
            AddParamConfigUI("Rate Data Loader", loaderPanel);

            // BuyOffset
            //---------------------------
            TextBox buyOffsetTb = new TextBox();
            buyOffsetTb.Text = CommonConfig.BuyOffset.ToString();
            buyOffsetTb.TextChanged += new TextChangedEventHandler(
                    delegate(object sender, TextChangedEventArgs args)
                    {
                        double result;
                        if (double.TryParse(buyOffsetTb.Text, out result) == false)
                            return;
                        CommonConfig.BuyOffset = result;
                    }
                );
            AddParamConfigUI("BuyOffset", buyOffsetTb);

            // SellOffset
            //---------------------------
            TextBox sellOffsetTb = new TextBox();
            sellOffsetTb.Text = CommonConfig.SellOffset.ToString();
            sellOffsetTb.TextChanged += new TextChangedEventHandler(
                    delegate(object sender, TextChangedEventArgs args)
                    {
                        double result;
                        if (double.TryParse(sellOffsetTb.Text, out result) == false)
                            return;
                        CommonConfig.SellOffset = result;
                    }
                );
            AddParamConfigUI("SellOffset", sellOffsetTb);

        }
        private void AddParamConfigUI(string name, UIElement ui)
        {
            StackPanel panel = this.ParamConfigStackPanel;
            panel.Children.Add(new Label() { Content = name });
            if (ui != null)
                panel.Children.Add(ui);
            panel.Children.Add(new Rectangle() { Height = 2, HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch, Fill = Brushes.Black });
        }

        void TestCaseWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.Shutdown(-1);
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
            // TODO
            /*
            DataLoader loader = new FenghuangDataLoader();
            MarketRateAnalyzer analyzer = new MarketRateAnalyzer(loader.ToArray());
            DealPointInfomation[] info = analyzer.GetDealInfo();
             */
        }

        private void RateAnalyzeTest()
        {
            GraphViewer win;
            this.Dispatcher.BeginInvoke(new Func(delegate()
            {
                win = new GraphViewer();
                win.Show();
                // TODO
                /*
                DataLoader dataLoader = new FenghuangDataLoader();
                MarketRateAnalyzer test = new MarketRateAnalyzer(dataLoader.ToArray());
                test.GetDealInfo();
                 */ 
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
            Random rand = new Random();
            double money;
            double rate;
            for (int i = 0; i < 100; i++)
            {
                episodeDAO = (RateMarketTestEpisodeDAO)dao.CreateEpisode();
                episodeDAO.Step = i;
                episodeDAO.Save();

                DealLogList logList = new DealLogList();
                money = 10000;
                rate = 100;
                for(int j=0;j<5000;j++)
                {
                    logList.Add((MarketActions)rand.Next(3), money, rate);
                    money += rand.Next(-1000, 1000);
                    rate += (rand.NextDouble() - 0.5) * 2 * 0.001;
                }

                episodeDAO.SaveDealLogs(logList);

            }
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
            DllTools.FTW_2(input, output, temp);

            double[] output2 = new double[input.Length*2];
            DllTools.FTW_5(input, output2);


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
                win.Closed += new EventHandler(
                    delegate(object sender, EventArgs args)
                    {
                        Application.Current.Shutdown();
                    }
                );
                Thread.CurrentThread.Priority = ThreadPriority.Normal;
                win.Show();
            }));
        }
        private void ControllerViewer()
        {
            this.Dispatcher.BeginInvoke(new Func(delegate()
            {
                ControllerCheckWin win = new ControllerCheckWin();
                win.Closed += new EventHandler(
                    delegate(object sender, EventArgs args)
                    {
                        Application.Current.Shutdown();
                    }
                );
                Thread.CurrentThread.Priority = ThreadPriority.Normal;
                win.Show();
            }));
        }
 


        private void AddNewTestCase(TestCaseGroup group)
        {
            BasicNewTestCase[] testCaseArr = NewTestCollecor.GetTest();
            foreach (BasicNewTestCase ca in testCaseArr)
                group.Add(ca);
        }
        private void AddTestCase()
        {

            TestCaseGroup newTestList = new TestCaseGroup();
            newTestList.Add(new TestCaseObject("TestDataBaseViewer", "", new TestCaseObject.TestFucntion(TestDataBaseViewer)));
            newTestList.Add(new TestCaseObject("ControllerViewer", "", new TestCaseObject.TestFucntion(ControllerViewer)));

            newTestList.Add(new TestCaseObject("TestFWT", "", new TestCaseObject.TestFucntion(TestFWT)));
            // New test case
            AddNewTestCase(newTestList);
            TestCaseList.Add(newTestList);
             
        }
    }
}
