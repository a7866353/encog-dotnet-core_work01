using MyProject01.DAO;
using SocketTestClient.ConnectionContoller;
using SocketTestClient.RequestObject;
using SocketTestClient.Sender;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketTestClient
{
    class RateTxtSaveUtility
    {
        private string _path;

        public RateTxtSaveUtility(string path)
        {
            _path = path;
        }
    }
    class RateCollectController : IRequestController
    {
        private RateDataControlDAO _dao;

        private double _updateInterval;
        private DateTimeFormatInfo _dtFormat;
        private RateByTimeRequest _lastRequest;
        private TimeSpan _getDataBlockDuration;
        private TimeSpan _getDuration;

        private string _symbol;
        private int _timeFrame;
        private DateTime _startDate;
        private DateTime _endDate;

        private DateTime _LastGetTime;
        private DateTime _LastItemTime;

        public RateCollectController(string symbol, int timeFrame, DateTime startDate, DateTime endDate)
        {
            _lastRequest = null;
            _dtFormat = new DateTimeFormatInfo();
            _dtFormat.ShortDatePattern = "yyyy.mm.dd hh:mm:ss";

            _symbol = symbol;
            _timeFrame = timeFrame;
            _startDate = startDate;
            _endDate = endDate;

            int min = (int)(_updateInterval * timeFrame);
            int sec = (int)(_updateInterval * timeFrame * 60);
            _getDataBlockDuration = new TimeSpan(0, min, sec);
            _getDuration = _getDataBlockDuration;

            _LastGetTime = _LastItemTime = _startDate;
        }

        public IRequest GetRequest()
        {
            // dao.Update(); // error
            DateTime tartgetTime = _LastGetTime + _getDuration;
            if (tartgetTime > DateTime.Now.AddMinutes(_timeFrame * -1))
                return null;

            RateByTimeRequest req = new RateByTimeRequest();
            req.SymbolName = _symbol;
            req.TimeFrame = _timeFrame;
            req.StartTime = _LastItemTime;
            req.StopTime = tartgetTime;
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
                    if (time <= _LastItemTime)
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
            System.Console.WriteLine("[RateDataController]" + str);
        }

        
    }
}
