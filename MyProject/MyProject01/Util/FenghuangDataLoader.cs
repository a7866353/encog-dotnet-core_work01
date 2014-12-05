using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MyProject01.Util
{
    class FenghuangDataLoader : DataLoader
    {
        public FenghuangDataLoader(string path = null)
        {
            if (true != string.IsNullOrWhiteSpace(path))
            {
                StreamReader sr = new StreamReader(path);
                string str = null;
                RateSet currRateSet;
                // read title
                str = sr.ReadLine();
                while (true)
                {
                    str = sr.ReadLine();
                    if (str == null)
                        break;

                    string[] strArr = str.Split(',');
                    // Data Check
                    if (string.IsNullOrWhiteSpace(strArr[0]) == true)
                        continue;
                    if (string.IsNullOrWhiteSpace(strArr[1]) == true)
                        continue;
                    currRateSet = new RateSet(DateTime.Parse(strArr[0]), double.Parse(strArr[1]));
                    Add(currRateSet);
                }
                sr.Close();
            }
            else
            {
                MarketData[] rawDataArr = DataProvider.GetAllMarketData();
                foreach (MarketData rawObj in rawDataArr)
                {
                    RateSet currRateSet;
                    currRateSet = new RateSet(DateTime.Parse(rawObj.Data), double.Parse(rawObj.MiddleRate));
                    Add(currRateSet);
                }
            }


            SortByDate();
            DataValueAdjust();

        }
    }
}
