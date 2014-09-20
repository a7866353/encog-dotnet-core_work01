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
                new TestCaseObject("SampleCase", "", new TestCaseObject.TestFucntion(TestCase01)),
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
