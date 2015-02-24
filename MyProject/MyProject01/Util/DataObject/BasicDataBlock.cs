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
