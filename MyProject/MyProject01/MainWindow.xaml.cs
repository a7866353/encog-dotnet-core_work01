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
using MyProject01.Test;

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
            FeedForwardNetworkTest test = new FeedForwardNetworkTest();

        }
    }




}
