using SocketTestClient.Sender;
using SocketTestClient.RequestObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using MyProject01.DAO;

namespace SocketTestClient.ConnectionContoller
{
    class RateDataNeed
    {
        public string Name;
        public string SymbolName;
        public int Timeframe;
    }

    class RateDataRequestController : IRequestController
    {
        private RateDataControlDAO _dao;
        private int _updateInterval;
        private DateTimeFormatInfo _dtFormat;
        private RateRequest _lastRequest;

        public RateDataRequestController(RateDataControlDAO dao, int updateInterval)
        {
            _dao = dao;
            _updateInterval = updateInterval;

            _lastRequest = null;
            _dtFormat = new DateTimeFormatInfo();
            _dtFormat.ShortDatePattern = "yyyy.mm.dd hh:mm:ss";
        }

        public IRequest GetRequest()
        {
            // dao.Update(); // error
            TimeSpan timeDulation = new TimeSpan(0, 0, 10);
            if (DateTime.Now - _dao.LastGetTime <= timeDulation)
                return null;

            TimeSpan nextDulation = new TimeSpan((long)_dao.TimeFrame * TimeSpan.TicksPerMinute * 1024);
            RateRequest req = new RateRequest();
            req.SymbolName = _dao.SymbolName;
            req.TimeFrame = _dao.TimeFrame;
            req.StartTime = _dao.LastItemTime;
            req.StopTime = _dao.LastGetTime + nextDulation;
            if (req.StopTime > DateTime.Now)
                req.StopTime = DateTime.Now;
            req.ReqCtrl = this;

            _lastRequest = req;
            return req;
        }

        public void SetResult(IRequest req)
        {
            RateDataIndicateRequest indicate = (RateDataIndicateRequest)req;
            RateInfo[] infoArr = indicate.RateInfoArray;
            List<RateData> dataList = new List<RateData>();

            if (infoArr != null)
            {
                foreach (RateInfo info in infoArr)
                {
                    DateTime time = Convert.ToDateTime(info.time, _dtFormat);
                    if (time <= _dao.LastItemTime)
                        continue;

                    RateData data = new RateData();
                    data.time = time;
                    data.high = info.high;
                    data.low = info.low;
                    data.open = info.open;
                    data.close = info.close;
                    data.real_volume = info.real_volume;
                    data.tick_volume = info.tick_volume;
                    data.spread = info.spread;

                    dataList.Add(data);
                }
                if (dataList.Count != 0)
                    _dao.Add(dataList.ToArray());
            }

            _dao.LastGetTime = _lastRequest.StopTime;
            _dao.Save();

            if (dataList.Count > 0)
            {
                Printf("Get:" + _dao.SymbolName + "_" + _dao.TimeFrame + " From" + 
                    _lastRequest.StartTime + " to " + _lastRequest.StopTime + " Count:" + dataList.Count);
            }
            if (indicate.EndFlag == true)
            {
                _lastRequest = null;
            }
        }

        private void Printf(string str)
        {
            System.Console.WriteLine("[RateDataController]" + str);
        }

    }

    class RateDataController : IRequestController
    {
        private RateDataDAOList _rateDataList;
        private int _watchListIndex;
        private TimeSpan _sendingBlockDuration = new TimeSpan(24, 0, 0);
        private bool _isSymbolListUpdated;

        private List<RateDataRequestController> _watchList;

        private RateDataNeed[] _need = new RateDataNeed[]
        {
            new RateDataNeed(){ Name="test01", SymbolName="USDJPYpro", Timeframe=5},
            new RateDataNeed(){ Name="test02", SymbolName="USDJPYpro", Timeframe=1},
            new RateDataNeed(){ Name="test03", SymbolName="USDJPYpro", Timeframe=1440},

        };

        private int[] _timeFrameArray = new int[]
        {
            1,      // 1 minute
            5,      // 5 minutes
            15,     // 15 minutes
            30,     // 30 minutes
            60,     // 1 hour
            240,    // 4 hours
            1440,   // 1 day
            10080,  // 1 week
            43200,  // 1 month
        };

        public RateDataController()
        {
            _rateDataList = new RateDataDAOList();
            _watchList = new List<RateDataRequestController>();
            _isSymbolListUpdated = false;
            _watchListIndex = -1;
        }

        public IRequest GetRequest()
        {
            if (_isSymbolListUpdated == false)
            {
                SymbolNameListRequest req = new SymbolNameListRequest();
                req.ReqCtrl = this;
                return req;
            }
            else
            {
                IRequest rateReq = null;
                int checkCount = 0;
                while (true)
                {
                    _watchListIndex++;
                    if (_watchListIndex >= _watchList.Count)
                        _watchListIndex = 0;

                    RateDataRequestController rateReqCtrl = _watchList[_watchListIndex];
                    rateReq = rateReqCtrl.GetRequest();
                    if (rateReq != null)
                    {
                        break;
                    }

                    checkCount++;
                    if (checkCount >= _watchList.Count)
                        break;
                }

                return rateReq;
            }
        }


        public void UpdateSymbolList(string[] symbols)
        {
            // Add all Symbol
            foreach( string symbol in symbols)
            {
                foreach( int timeFrame in _timeFrameArray)
                {
                    string name = symbol + timeFrame;
                    if (_rateDataList.Get(name) == null)
                    {
                        _rateDataList.Add(name, symbol, timeFrame, new DateTime(1988, 1, 1, 0, 0, 0));
                    }

                }
            }
            _isSymbolListUpdated = true;

            // Add All symbol to watch list.
            foreach(RateDataControlDAO dao in _rateDataList)
            {
                _watchList.Add(new RateDataRequestController(dao, 512));
            }
        }


        public void SetResult(IRequest req)
        {
            throw new NotImplementedException();
        }
    }
}
