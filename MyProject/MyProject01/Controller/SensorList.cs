using MyProject01.DataSources;
using MyProject01.DataSources.DataSourceParams;
using MyProject01.Util;
using MyProject01.Util.DllTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.Controller
{
    public interface ISensor
    {
        int SkipCount { get; }
        int TotalLength { get; }
        int DataBlockLength { get; }
        IDataSource DataSource { get; }
        DataSourceCtrl DataSourceCtrl { set; }
        int Copy(int index, DataBlock buffer, int offset);
        void Init();
    }
    [Serializable]
    class SensorGroup : List<ISensor>, ISensor
    {
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
                int minValue = this[0].TotalLength;
                foreach (ISensor sen in this)
                {
                    minValue = Math.Min(sen.TotalLength, minValue);
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




        public void Init()
        {
            foreach (ISensor sen in this)
            {
                sen.Init();
            }
        }


        public IDataSource DataSource
        {
            get
            {
                return this[0].DataSource;
            }
        }


        public DataSourceCtrl DataSourceCtrl
        {
            set
            {
                foreach (ISensor sen in this)
                {
                    sen.DataSourceCtrl = value;
                }
            }
        }
    }

    [Serializable]
    class RateSensor : ISensor
    {
        private int _dataCount;
        [NonSerialized]
        private IDataSource _dataSource;
        public RateSensor(int dataCount)
        {
            _dataCount = dataCount;
        }
        public int SkipCount
        {
            get { return _dataCount-1; }
        }

        public int TotalLength
        {
            get 
            {
                if (_dataSource == null)
                    return 0;
                else
                    return _dataSource.TotalLength; 
            }
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
        public void Init()
        {
            // Nothing
        }


        public IDataSource DataSource
        {
            get
            {
                return _dataSource;
            }
            set
            {
                _dataSource = value;
            }
        }


        public DataSourceCtrl DataSourceCtrl
        {
            set
            {
                RateDataSourceParam param = new RateDataSourceParam(5);
                this._dataSource = value.Get(param);
            }
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
        public void Init()
        {
            _dataBuffer = new DataBlock(_dataSource.TotalLength);
            _dataSource.Copy(0, _dataBuffer, 0, _dataBuffer.Length);
            _norm.Set(_dataBuffer.Data, 0, _dataBuffer.Length);
            _norm.DataValueAdjust(-0.1, 0.1);
        }


        public IDataSource DataSource
        {
            get
            {
                return _dataSource;
            }
            set
            {
                _dataSource = value;
            }
        }


        public DataSourceCtrl DataSourceCtrl
        {
            set
            {
                RateDataSourceParam param = new RateDataSourceParam(5);
                this._dataSource = value.Get(param);
            }
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
        public void Init()
        {
            _dataBuffer = new DataBlock(DataBlockLength);
            _outputBuffer = new DataBlock(DataBlockLength);
            _tmpBuffer = new double[DataBlockLength];
        }


        public IDataSource DataSource
        {
            get
            {
                return _dataSource;
            }
        }


        public DataSourceCtrl DataSourceCtrl
        {
            set
            {
                RateDataSourceParam param = new RateDataSourceParam(5);
                this._dataSource = value.Get(param);
            }
        }
    }

    //===========================================
    [Serializable]
    abstract class BasicKDJSensor : ISensor
    {
        [NonSerialized]
        protected KDJDataSource _dataSource;
        private int _blockLength;

        public BasicKDJSensor(int blockLength)
        {
            _blockLength = blockLength;
        }

        public int SkipCount
        {
            get { return _dataSource.SkipCount + _blockLength - 1; }
        }

        public int TotalLength
        {
            get
            {
                if (_dataSource == null)
                    return 0;
                else
                    return _dataSource.TotalLength;
            }
        }

        public int DataBlockLength
        {
            get { return _blockLength; }
        }

        public IDataSource DataSource
        {
            get { return _dataSource; }
        }

        public DataSourceCtrl DataSourceCtrl
        {
            set
            {
                this.DataSourceCtrl = value;
                KDJDataSourceParam param = new KDJDataSourceParam();
                _dataSource = value.Get(param) as KDJDataSource;
            }
        }

        abstract public int Copy(int index, DataBlock buffer, int offset);

        public void Init()
        {
            // Todo nothing.
            return;
        }
    }

    [Serializable]
    class KDJ_KSensor : BasicKDJSensor
    {
        public KDJ_KSensor(int blockLength): base(blockLength)
        {

        }

        public override int Copy(int index, DataBlock buffer, int offset)
        {
            _dataSource.CopyK(index, buffer, offset, this.DataBlockLength);
            return this.DataBlockLength;
        }
    }

    [Serializable]
    class KDJ_DSensor : BasicKDJSensor
    {
        public KDJ_DSensor(int blockLength): base(blockLength)
        {

        }

        public override int Copy(int index, DataBlock buffer, int offset)
        {
            _dataSource.CopyD(index, buffer, offset, this.DataBlockLength);
            return this.DataBlockLength;
        }
    }

    [Serializable]
    class KDJ_JSensor : BasicKDJSensor
    {
        public KDJ_JSensor(int blockLength): base(blockLength)
        {

        }

        public override int Copy(int index, DataBlock buffer, int offset)
        {
            _dataSource.CopyJ(index, buffer, offset, this.DataBlockLength);
            return this.DataBlockLength;
        }
    }

    [Serializable]
    class KDJ_CrossSensor : BasicKDJSensor
    {
        public KDJ_CrossSensor(int blockLength)
            : base(blockLength)
        {

        }

        public override int Copy(int index, DataBlock buffer, int offset)
        {
            for (int i = 0; i < DataBlockLength; i++)
            {
                int idx = index+i;
                double value = 0;
                value += Math.Abs(_dataSource.KArr[idx] - _dataSource.DArr[idx]);
                value += Math.Abs(_dataSource.DArr[idx] - _dataSource.JArr[idx]);
                value += Math.Abs(_dataSource.JArr[idx] - _dataSource.KArr[idx]);
                buffer[offset + i] = value;
            }
            return this.DataBlockLength;
        }
    }


}
