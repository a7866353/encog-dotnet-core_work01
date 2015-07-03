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
        private RateDataDAOList _rateDataList;
        private RateDataControlDAO _currentTargetDao;
        private RateRequest _lastRequest;
        private TimeSpan _sendingBlockDuration = new TimeSpan(24, 0, 0);
        private DateTimeFormatInfo _dtFormat;
        private bool _isSymbolListUpdated;

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
            43200,  // 1 monthv
        };

        public bool IsFinish
        {
            get 
            {
                if (_currentTargetDao == null)
                    return true;
                else
                    return false;
            }
        }

        public RateDataRequestController()
        {
            _rateDataList = new RateDataDAOList();
            _currentTargetDao = null;
            _dtFormat = new DateTimeFormatInfo();
            _dtFormat.ShortDatePattern = "yyyy.mm.dd hh:mm:ss";
            _isSymbolListUpdated = false;
        }

        public IRequest GetRequest()
        {
            /*
            if (_currentTargetDao != null)
                return null;
            */
            if (_isSymbolListUpdated == false)
            {
                SymbolNameListRequest req = new SymbolNameListRequest();
                req.ReqCtrl = this;
                return req;
            }

            foreach( RateDataControlDAO dao in _rateDataList)
            {
                _lastRequest = GetNextReq(dao);
                if (_lastRequest != null)
                {
                    _currentTargetDao = dao;
                    break;
                }
            }
            return _lastRequest;
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
                    if (time <= _currentTargetDao.LastItemTime)
                        continue;

                    RateData data = new RateData();
                    data.time = time;
                    data.high = info.high;
                    data.low = info.low;
                    data.open = info.open;
                    data.close = info.close;

                    dataList.Add(data);
                }
                if (dataList.Count != 0)
                    _currentTargetDao.Add(dataList.ToArray());
            }
            else
            {
                infoArr = infoArr;
            }
            _currentTargetDao.LastGetTime = _lastRequest.StopTime;
            _currentTargetDao.Save();

            Printf("Get:" + _currentTargetDao.SymbolName + " From" + _lastRequest.StartTime + " to " + _lastRequest.StopTime + " Count:" + dataList.Count);

            if (indicate.EndFlag == true)
            {
                _currentTargetDao = null;
                _lastRequest = null;
            }
        }


        private RateRequest GetNextReq(RateDataControlDAO dao)
        {
            // dao.Update(); // error
            TimeSpan timeDulation = new TimeSpan(0, 0, 10);
            if (DateTime.Now - dao.LastGetTime <= timeDulation)
                return null;

            TimeSpan nextDulation = new TimeSpan( (long)dao.TimeFrame * TimeSpan.TicksPerMinute * 1024 );
            RateRequest req = new RateRequest();
            req.SymbolName = dao.SymbolName;
            req.TimeFrame = dao.TimeFrame;
            req.StartTime = dao.LastItemTime;
            req.StopTime = dao.LastGetTime + nextDulation;
            if (req.StopTime > DateTime.Now)
                req.StopTime = DateTime.Now;
            req.ReqCtrl = this;

            return req;
        }

        public void UpdateSymbolList(string[] symbols)
        {
            // Add Test Symbol
            foreach (RateDataNeed info in _need)
            {
                if (_rateDataList.Get(info.Name) == null)
                {
                    _rateDataList.Add(info.Name, info.SymbolName, info.Timeframe, new DateTime(1988, 1, 1, 0, 0, 0));
                }
            }

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
        }

        private void Printf(string str)
        {
            System.Console.WriteLine("[RateDataController]" + str);
        }
    }
}
