using MyProject01.Util;
using MyProject01.Util.DllTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.Controller
{
    interface ISensor
    {
        int SkipCount { get; }
        int TotalLength { get; }
        int DataBlockLength { get; }
        IDataSource DataSource { set; }
        int Copy(int index, DataBlock buffer, int offset);
        void Init();
    }
    [Serializable]
    class SensorGroup : List<ISensor>, ISensor
    {
        private int _currentPosition;
       
        public int SkipCount
        {
            get
            {
                int maxValue = this[0].SkipCount;
                foreach(ISensor sen in this)
                {
                    maxValue = Math.Max(sen.SkipCount, maxValue);
                }
                return maxValue;
            }
        }

        public int TotalLength
        {
            get
            {
                int minValue = this[0].SkipCount;
                foreach (ISensor sen in this)
                {
                    minValue = Math.Min(sen.SkipCount, minValue);
                }
                return minValue;

            }
        }

        public int DataBlockLength
        {
            get
            {
                int sum = 0;
                foreach (ISensor sen in this)
                {
                    sum += sen.DataBlockLength;
                }
                return sum;
            }
        }
        public int Copy(int index, DataBlock buffer, int offset)
        {
            int idx = 0;
            foreach(ISensor sen in this)
            {
                sen.Copy(index, buffer, idx + offset);
                idx += sen.DataBlockLength;
            }
            return idx;
        }


        public IDataSource DataSource
        {
            set 
            { 
                foreach(ISensor sen in this)
                {
                    sen.DataSource = value;
                }
            
            }
        }


        public void Init()
        {
            foreach (ISensor sen in this)
            {
                sen.Init();
            }
        }
    }
    [Serializable]
    class RateSensor : ISensor
    {
        private int _dataCount;
        private int _index;
        [NonSerialized]
        private IDataSource _dataSource;
        public RateSensor(int dataCount)
        {
            _dataCount = dataCount;
            _index = 0;
        }
        public int SkipCount
        {
            get { return _dataCount-1; }
        }

        public int TotalLength
        {
            get { return _dataSource.TotalLength - SkipCount; }
        }

        public int DataBlockLength
        {
            get { return _dataCount; }
        }

        public int Copy(int index, DataBlock buffer, int offset)
        {
            _dataSource.Copy(index, buffer, offset, DataBlockLength);
            return 1;
        }

        public IDataSource DataSource
        {
            set { _dataSource = value; }
        }


        public void Init()
        {
            // Nothing
        }
    }
    [Serializable]
    class RateNormalizeSensor : ISensor
    {
        private int _dataCount;
        private int _index;
        [NonSerialized]
        private IDataSource _dataSource;
        private DataNormallizer _norm;
    [NonSerialized]
        private DataBlock _dataBuffer;
        public RateNormalizeSensor(int dataCount)
        {
            _dataCount = dataCount;
            _index = 0;
        }
        public int SkipCount
        {
            get { return _dataCount - 1; }
        }

        public int TotalLength
        {
            get { return _dataSource.TotalLength - SkipCount; }
        }

        public int DataBlockLength
        {
            get { return _dataCount; }
        }

        public int Copy(int index, DataBlock buffer, int offset)
        {
            DataBlock.Copy(_dataBuffer, index, buffer, offset, DataBlockLength);
            return 1;
        }

        public IDataSource DataSource
        {
            set { _dataSource = value; }
        }


        public void Init()
        {
            _dataBuffer = new DataBlock(_dataSource.TotalLength);
            _dataSource.Copy(0, _dataBuffer, 0, _dataBuffer.Length);
            _norm.Set(_dataBuffer.Data, 0, _dataBuffer.Length);
            _norm.DataValueAdjust(-0.1, 0.1);
        }
    }
    [Serializable]
    class RateFWTNormalizeSensor : ISensor
    {
        private int _dataCount;
        private int _index;
        [NonSerialized]
        private IDataSource _dataSource;
        private DataNormallizer _norm;
        [NonSerialized]
        private DataBlock _dataBuffer;
        [NonSerialized]
        private DataBlock _outputBuffer;

        [NonSerialized]
        private double[] _tmpBuffer;
        public RateFWTNormalizeSensor(int dataCount)
        {
            _dataCount = dataCount;
            _index = 0;
        }
        public int CurrentPosition
        {
            get
            {
                return _index;
            }
            set
            {
                _index = value;
            }
        }

        public int SkipCount
        {
            get { return _dataCount - 1; }
        }

        public int TotalLength
        {
            get { return _dataSource.TotalLength - SkipCount; }
        }

        public int DataBlockLength
        {
            get { return _dataCount; }
        }

        public int Copy(int index, DataBlock buffer, int startIndex)
        {
            _dataSource.Copy(index, _dataBuffer, 0, _dataBuffer.Length);

            DllTools.FTW_4(_dataBuffer.Data, _outputBuffer.Data, _tmpBuffer);

            _norm.Set(_outputBuffer.Data, 0, _outputBuffer.Length);
            _norm.DataValueAdjust(-0.1, 0.1);
            
            DataBlock.Copy(_outputBuffer, 0, buffer, startIndex, _outputBuffer.Length);

            return 1;
        }

        public IDataSource DataSource
        {
            set { _dataSource = value; }
        }


        public void Init()
        {
            _dataBuffer = new DataBlock(DataBlockLength);
            _outputBuffer = new DataBlock(DataBlockLength);
            _tmpBuffer = new double[DataBlockLength];
        }
    }

}
