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
using System.Windows.Navigation;
using System.Windows.Shapes;
using MongoDB.Driver;
using MongoDB.Bson;
using System.IO;
using System.Threading;

namespace DataImporter
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private delegate void _func();

        private Thread _workThread;
        private System.Timers.Timer _updateTimer;
        private BasicImporter _importer;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            ExcuteButton.Click += ExcuteButton_Click;

            _updateTimer = new System.Timers.Timer(300);
            _updateTimer.Elapsed += _updateTimer_Elapsed;
            _updateTimer.AutoReset = true;
        }

        void _updateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_importer == null)
                return;
            this.Dispatcher.Invoke(new _func(delegate()
            {
                FinishRateLabel.Content = "Finish: " + _importer.FinishRate.ToString("G4");
            }));


        }

        void ExcuteButton_Click(object sender, RoutedEventArgs e)
        {
            ExcuteButton.IsEnabled = false;
            _updateTimer.Start();
            string str = InputTextBox.Text;
            _workThread = new Thread(new ThreadStart(delegate()
            {
                // From TextBox
                if (false)
                {

                    byte[] streamData = Encoding.Default.GetBytes(str);
                    _importer = new FengHuangDataImporter();
                    _importer.Load(streamData);

                }

                // From file
                if (true)
                {
                    string filePath = @"D:\workplace\FANN\Store\汇率离线数据\USDJPY\USDJPY.txt";
                    _importer = new MTDataImporter("USDJPY", 1);
                    _importer.Load(filePath);

                }

                FinishWork();
            }));

            _workThread.Start();
        }

        private void FinishWork()
        {
            this.Dispatcher.Invoke(new _func(delegate()
            {
                InputTextBox.Text = "Finish!";
                ExcuteButton.IsEnabled = true;
                _updateTimer.Stop();


            }));
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
