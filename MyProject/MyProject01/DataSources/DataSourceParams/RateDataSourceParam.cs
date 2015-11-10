using MyProject01.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.DataSources.DataSourceParams
{
    class FixDataSource : IDataSource
    {
        private DataLoader _loader;
        private DataBlock _dataBuffer;
        private double _lengthLimit;

        public FixDataSource(DataLoader loader, double lengthLimit = 1.0)
        {
            _loader = loader;

            _dataBuffer = new DataBlock((int)(_loader.Count * lengthLimit));
            for (int i = 0; i < _dataBuffer.Length; i++)
            {
                _dataBuffer[i] = _loader[i].Close;
            }
        }

        public void Copy(int index, DataBlock buffer, int offset, int length)
        {
            DataBlock.Copy(_dataBuffer, index - length + 1, buffer, offset, length);
        }
        public int TotalLength
        {
            get
            {
                return _dataBuffer.Length;
            }
        }
        public RateSet this[int index]
        {
            get { return _loader[index]; }
        }
    }
    class RateDataSourceParam: IDataSourceParam
    {
        private int _timeFrame;
        private double _lengthLimit;

        public int TimeFrame { get { return _timeFrame; } }

        public RateDataSourceParam(int timeFrame, double countLimit = 1.0)
        {
            _timeFrame = timeFrame;
            _lengthLimit = countLimit;
        }

        public bool CompareTo(IDataSourceParam param)
        {
            if (this.GetType() != param.GetType())
                return false;

            RateDataSourceParam inParam = (RateDataSourceParam)param;
            do
            {
                if (inParam._timeFrame != this._timeFrame)
                    return false;
                if (inParam._lengthLimit != this._lengthLimit)
                    return false;

            } while (false);

            return true;
        }

     
        public IDataSource Create(DataSourceCtrl ctrl)
        {
            return new FixDataSource(ctrl.SourceLoader, _lengthLimit);
        }

    }
}
