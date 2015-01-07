using SocketTestClient.RateDataController;
using SocketTestClient.Sender;
using SocketTestClient.RequestObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace SocketTestClient.ConnectionContoller
{
    class RateDataNeed
    {
        public string Name;
        public string SymbolName;
        public int Timeframe;
    }
    class RateDataController : IRequestController
    {
        private RateDataDAOList _rateDataList;
        private RateDataControlDAO _currentTargetDao;
        private RateRequest _lastRequest;
        private TimeSpan _sendingBlockDuration = new TimeSpan(24, 0, 0);
        private DateTimeFormatInfo _dtFormat;

        private RateDataNeed[] _need = new RateDataNeed[]
        {
            new RateDataNeed(){ Name="test01", SymbolName="USDJPYpro", Timeframe=5},
            new RateDataNeed(){ Name="test02", SymbolName="USDJPYpro", Timeframe=1},

        };


        public RateDataController()
        {
            _rateDataList = new RateDataDAOList();
            _currentTargetDao = null;

            foreach( RateDataNeed info in _need)
            {
                if (_rateDataList.Get(info.Name) == null)
                {
                    _rateDataList.Add(info.Name, info.SymbolName, info.Timeframe, new DateTime(2014, 10, 1, 0, 0, 0));
                }
            }
            

            _dtFormat = new DateTimeFormatInfo();
            _dtFormat.ShortDatePattern = "yyyy.mm.dd hh:mm:ss";
        }

        public IRequest GetRequest()
        {

            if (_currentTargetDao != null)
                return null;

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
            List<RateData> dataList = new List<RateData>(infoArr.Length);
            foreach(RateInfo info in infoArr)
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
            if(dataList.Count != 0)
                _currentTargetDao.Add(dataList.ToArray());

            _currentTargetDao.LastGetTime = _lastRequest.StopTime;
            _currentTargetDao.Save();

            Printf("Get:From" + _lastRequest.StartTime + " to " + _lastRequest.StopTime + " Count:" + dataList.Count);

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

            RateRequest req = new RateRequest();
            req.SymbolName = dao.SymbolName;
            req.TimeFrame = dao.TimeFrame;
            req.StartTime = dao.LastItemTime;
            req.StopTime = dao.LastGetTime + _sendingBlockDuration;
            if (req.StopTime > DateTime.Now)
                req.StopTime = DateTime.Now;

            return req;
        }

        private void Printf(string str)
        {
            System.Console.WriteLine("[RateDataController]" + str);
        }
    }
}
