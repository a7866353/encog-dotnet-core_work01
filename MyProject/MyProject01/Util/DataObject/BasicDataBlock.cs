using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.Util.DataObject
{
    public abstract class BasicDataBlock : IDataBlock
    {
        protected DataLoader _loader;
        protected int _startIndex;
        protected int _length;
        protected int _blockLen;

        protected double[] _data;

        public BasicDataBlock(DataLoader loader, int startIndex, int length, int blockLength)
        {
            _index = 0;
            _loader = loader;
            _startIndex = startIndex;
            _length = Math.Min(loader.Count-_startIndex, length);
            _data = new double[_length];
            _blockLen = blockLength;
            UpdateData();
        }

        protected int _blockCount
        {
            get { return _length - _blockLen + 1; }
        }
        protected int _blockLength
        {
            get { return _blockLen; }
        }
        protected double GetRateData(int i)
        {
            return _loader[_startIndex + _blockLen - 1 + i].RealValue;
        }

        protected int RateDataCopy(double[] array, int index)
        {
            int remain = _data.Length - index;
            int length = Math.Min(remain, _blockLen);

            if (length <= 0)
                return 0;

            Array.Copy(_data, index, array, 0, length);

            return length;
        }

        private void UpdateData()
        {
            for (int i = 0; i < _data.Length; i++)
            {
                _data[i] = _loader[i + _startIndex].Value;
            }

        }

        protected int _index;
        
        public void Reset()
        {
            _index = 0;
        }
        public bool Next()
        {
            if ((_index + 1) >= _length)
                return false;
            else
            {
                _index++;
                return true;
            }
        }

        public int Copy(double[] array)
        {
            return Copy(array, _index);
        }


        abstract public int Length
        {
            get;
        }

        abstract public int DataBlockLength
        {
            get;
        }

        abstract public double GetRate(int i);

        abstract public int Copy(double[] array, int index);

        abstract public BasicDataBlock GetNewBlock(int startIndex, int length);


        abstract public BasicDataBlock Clone();
       
    }
}
