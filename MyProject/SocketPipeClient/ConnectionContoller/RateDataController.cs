﻿using SocketTestClient.Sender;
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
    interface IRataDataCollectTask
    {
        int Update();
        string Symbole { get; }
        double InvervalCount { set; get; }
    }
    class RateDataRequestController : IRataDataCollectTask
    {
        private RateDataControlDAO _dao;
        private double _updateInterval;
        private DateTimeFormatInfo _dtFormat;
        private RateByTimeRequest _lastRequest;

        public RateDataRequestController(RateDataControlDAO dao, double updateInterval)
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
            TimeSpan timeDulation = new TimeSpan(0, 0, (int)(_updateInterval * _dao.TimeFrame));
            if (DateTime.Now - _dao.LastGetTime <= timeDulation)
                return null;

            TimeSpan nextDulation;
            if (_lastRequest == null)
                nextDulation = new TimeSpan((long)((long)_dao.TimeFrame * TimeSpan.TicksPerMinute * _updateInterval));
            else
                nextDulation = new TimeSpan();
            RateByTimeRequest req = new RateByTimeRequest();
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
        }

        private void Printf(string str)
        {
            System.Console.WriteLine("[RateDataController]" + str);
        }


        public int Update()
        {
            throw new NotImplementedException();
        }

        public string Symbole
        {
            get { throw new NotImplementedException(); }
        }

        public int InvervalCount
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }


    class RateByTimeRequestController : IRataDataCollectTask
    {
        private RateDataControlDAO _dao;
        private double _updateInterval;
        private DateTimeFormatInfo _dtFormat;
        private RateByTimeRequest _lastRequest;
        private TimeSpan _getDataBlockDuration;
        private TimeSpan _getDuration;
        private ISender _sender;

        public RateByTimeRequestController(RateDataControlDAO dao, double updateInterval)
        {
            _dao = dao;
            _updateInterval = updateInterval = 2;

            _lastRequest = null;
            _dtFormat = new DateTimeFormatInfo();
            _dtFormat.ShortDatePattern = "yyyy.mm.dd hh:mm:ss";

            int min = (int)(_updateInterval * _dao.TimeFrame);
            int sec = (int)(((_updateInterval * _dao.TimeFrame) - (int)(_updateInterval * _dao.TimeFrame))*60);
            _getDataBlockDuration = new TimeSpan(0, min, sec);
            _getDuration = _getDataBlockDuration;
        }

        public RateByTimeRequest GetRequest()
        {
            // dao.Update(); // error
            DateTime tartgetTime = _dao.LastGetTime + _getDuration;
            if (tartgetTime > DateTime.Now.AddMinutes(_dao.TimeFrame*-1))
            {
                // _dao.LastGetTime = DateTime.Now;
                // _dao.Save();
                return null;
            }

            RateByTimeRequest req = new RateByTimeRequest();
            req.SymbolName = _dao.SymbolName;
            req.TimeFrame = _dao.TimeFrame;
            req.StartTime = _dao.LastItemTime;
            req.StopTime = tartgetTime;

            Printf("Send:" + _dao.SymbolName + "_" + _dao.TimeFrame + " From" +
                req.StartTime + " to " + req.StopTime );


            _lastRequest = req;
            return req;
        }

        public void SetResult(RateInfo[] infoArr)
        {
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

            if (infoArr == null || infoArr.Length == 0)
            {
                CSVLogFormater.Add(_dao.Name, _lastRequest.StartTime, _lastRequest.StopTime,
                    0,
                    _lastRequest.StartTime,
                    _lastRequest.StartTime);
            }
            else
            {
                CSVLogFormater.Add(_dao.Name, _lastRequest.StartTime, _lastRequest.StopTime,
                    infoArr.Length,
                    Convert.ToDateTime(infoArr[0].time, _dtFormat),
                    Convert.ToDateTime(infoArr[infoArr.Length - 1].time, _dtFormat));

            }
            // for debug
            if (dataList.Count > 0)
            {
                Printf("Get:" + _dao.SymbolName + "_" + _dao.TimeFrame + " From" +
                    _lastRequest.StartTime + " to " + _lastRequest.StopTime + " Count:" + dataList.Count);
            }
            if (dataList.Count == 0)
            {
                _getDuration += _getDataBlockDuration;
            }
            else
            {
                _getDuration = _getDataBlockDuration;
            }
        }

        private void Printf(string str)
        {
            System.Console.WriteLine("[RateData]" + str);
        }


        public int Update()
        {
            RateByTimeRequest req = GetRequest();
            _sender.Send(req);
            if (req .RateInfoArray != null && req.RateInfoArray.Length> 0)
            {
                SetResult(req.RateInfoArray);
                return req.RateInfoArray.Length;
            }
            else
            {
                return 0;
            }
        }

        public string Symbole
        {
            get { return _dao.CollectiongName; }
        }

        public double InvervalCount
        {
            get
            {
                return _updateInterval ;
            }
            set
            {
                _updateInterval = value; ;
            }
        }
    }

    class RateByCountRequestController
    {
        private int _getCount = 1024;
        private RateDataControlDAO _dao;
        private double _updateInterval;
        private DateTimeFormatInfo _dtFormat;
        private DateTime _lastUpdateTime;
        private RateByCountRequest _lastRequest;
        private ISender _sender;

        public RateByCountRequestController(RateDataControlDAO dao, double updateInterval)
        {
            _dao = dao;
            _updateInterval = updateInterval;
            
            _lastRequest = null;
            _dtFormat = new DateTimeFormatInfo();
            _dtFormat.ShortDatePattern = "yyyy.mm.dd hh:mm:ss";
        }
        public bool Update()
        {
            RateByCountRequest req = GetRequest();
            if (_sender.Send(req) >= 0)
            {
                SetResult(req.RateInfoArray);
                return true;
            }
            else
            {
                return false;
            }
        }
        public RateByCountRequest GetRequest()
        {
            // dao.Update(); // error
            TimeSpan timeDulation = new TimeSpan(0, 0, (int)(_updateInterval * _dao.TimeFrame));
            if (DateTime.Now - _dao.LastGetTime <= timeDulation)
                return null;

            _lastUpdateTime = DateTime.Now;
            TimeSpan nextDulation = new TimeSpan((long)_dao.TimeFrame * TimeSpan.TicksPerMinute * 1024);
            RateByCountRequest req = new RateByCountRequest();
            req.SymbolName = _dao.SymbolName;
            req.TimeFrame = _dao.TimeFrame;
            req.StartTime = _dao.LastItemTime;
            req.Count = _getCount;

            return req;
        }

        public void SetResult(RateInfo[] infoArr)
        {
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

            _dao.LastGetTime = _lastUpdateTime;
            _dao.Save();

            if (dataList.Count > 0)
            {
                Printf("Get:" + _dao.SymbolName + "_" + _dao.TimeFrame + " From" + 
                    _lastRequest.StartTime + " Count " + _lastRequest.Count + " GetCount:" + dataList.Count);
            }
        }

        private void Printf(string str)
        {
            System.Console.WriteLine("[RateDataController]" + str);
        }

    }

    class RateDataController
    {
        private RateDataDAOList _rateDataList;
        private int _watchListIndex;
        private TimeSpan _sendingBlockDuration = new TimeSpan(24, 0, 0);
        private bool _isSymbolListUpdated;

        private List<IRequestController> _watchList;

        private string[] _interalSymbols;

        private int[] _timeFrameArray = new int[]
        {
            1,      // 1 minute
#if true
            5,      // 5 minutes
            15,     // 15 minutes
            30,     // 30 minutes
            60,     // 1 hour
            240,    // 4 hours
            1440,   // 1 day
            10080,  // 1 week
            43200,  // 1 month
#endif
        };

        public RateDataController()
        {
            _rateDataList = new RateDataDAOList();
            _watchList = new List<IRequestController>();
            _isSymbolListUpdated = false;
            _watchListIndex = -1;

            //-----------------
#if false
            _interalSymbols = new string[]
            {
                "AUDJPY",
                "AUDJPYpro",
                "AUDUSD",
                "AUDUSDpro",
                "CHFJPY",
                "CHFJPYpro",
                "EURCAD",
                "EURCADpro",
                "EURCHF",
                "EURCHFpro",
                "EURGBP",
                "EURGBPpro",
                "EURJPY",
                "EURJPYpro",
                "EURUSD",
                "EURUSDpro",
                "GBPCHF",
                "GBPCHFpro",
                "GBPJPY",
                "GBPJPYpro",
                "GBPUSD",
                "GBPUSDpro",
                "NZDJPY",
                "NZDJPYpro",
                "NZDUSD",
                "NZDUSDpro",
                "USDCAD",
                "USDCADpro",
                "USDJPY",
                "USDJPYpro",
                "USDCHF",
                "USDCHFpro",
                "XAGUSD",
                "XAGUSDpro",
                "XAUUSD",
                "XAUUSDpro",
            };
#else
            _interalSymbols = new string[]
            {
                "USDJPY",
            };

#endif
            UpdateSymbolList(_interalSymbols);
        }

        public IRequest GetRequest()
        {
            // if (_isSymbolListUpdated == false)
            if (false)
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

                    IRequestController rateReqCtrl = _watchList[_watchListIndex];
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
                    string name = symbol + "_" + timeFrame;
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
                // _watchList.Add(new RateDataRequestController(dao, 512));
                _watchList.Add(new RateByTimeRequestController(dao, 512));
            }
        }

        public bool AddWatchSymbol(string name, double updateInterval)
        {
            RateDataControlDAO dao = _rateDataList.Get(name);
            if (dao == null)
                return false;
            _watchList.Add(new RateByTimeRequestController(dao, updateInterval));
            return true;
        }

        public void SetResult(IRequest req)
        {
            throw new NotImplementedException();
        }
    }
}
