using MyProject01.Controller;
using MyProject01.DAO;
using MyProject01.TestCases;
using MyProject01.Util.View;
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
using System.Windows.Shapes;

namespace MyProject01.Win
{
    /// <summary>
    /// DataBaseViewer.xaml 的交互逻辑
    /// </summary>
    public partial class DataBaseViewer : Window
    {
        private RateMarketTestDAO[] _rateMarketTestDaoArr;
        public DataBaseViewer()
        {
            InitializeComponent();

            Loaded += DataBaseViewer_Loaded;
            TestCaseDataGrid.SelectionChanged += TestCaseDataGrid_SelectionChanged;
            EpisodeDataGrid.SelectionChanged += EpisodeDataGrid_SelectionChanged;
            DeleteButton.Click += DeleteButton_Click;
            UpdateButton.Click += UpdateButton_Click;


        }

        void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateTestCase();
        }

        void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            foreach( RateMarketTestDAO dao in TestCaseDataGrid.SelectedItems)
            {
                dao.Remove();
            }
            UpdateTestCase();

        }

        void TestCaseDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;
            RateMarketTestDAO currentDao = (RateMarketTestDAO)e.AddedItems[0];
            EpisodeDataGrid.ItemsSource = currentDao.GetAllEpisodes<RateMarketTestEpisodeDAO>();
        }

        void EpisodeDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;
            RateMarketTestEpisodeDAO dao = (RateMarketTestEpisodeDAO)e.AddedItems[0];

            // 获得Log数据
            DealLogList logList = dao.GetDealLogs();
            double[] rateArr = new double[logList.Count];
            double[] currentMoneyArr = new double[logList.Count];

            for(int i=0;i<logList.Count; i++)
            {
                DealLog log = logList[i];
                rateArr[i] = log.Rate;
                currentMoneyArr[i] = log.CurrentMoney;
            }

            // 创建显示窗口
            GraphViewer logView = new GraphViewer();
            logView.Show();
            // 增加汇率曲线
            GraphLine rateLine = logView.AddLineData(rateArr);
            // 增加交易标记
            for(int i=0;i<logList.Count; i++)
            {
                DealLog log = logList[i];
                if(log.Action == Agent.MarketActions.Buy)
                    rateLine.AddMark(i, Brushes.Red);
                else if(log.Action == Agent.MarketActions.Sell)
                    rateLine.AddMark(i, Brushes.Green);
            }
            // 增加当前收益曲线
            logView.AddLineData(currentMoneyArr);
        }

        void DataBaseViewer_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateTestCase();
        }

        private void UpdateTestCase()
        {
            _rateMarketTestDaoArr = BasicTestCaseDAO.GetAllDAOs<RateMarketTestDAO>();
            TestCaseDataGrid.ItemsSource = _rateMarketTestDaoArr;
            EpisodeDataGrid.ItemsSource = null;

        }
    }
}
