using MyProject01.Controller;
using MyProject01.DAO;
using MyProject01.TestCases;
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
