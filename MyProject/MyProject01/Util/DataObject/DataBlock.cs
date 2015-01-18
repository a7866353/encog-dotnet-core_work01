using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.Util
{
    public class DataBlock
    {
        private DataLoader _loader;
        private double _scale;
        private double _offset;
        private double[] _data;
        private int _startIndex;
        private int _length;
        private int _blockLength;

        public DataBlock(DataLoader loader, int startIndex, int length, int blockLength)
        {
            _loader = loader;
            _startIndex = startIndex;
            _length = Math.Min(loader.Count-_startIndex, length);
            _blockLength = blockLength;

            _data = new double[_length];
            _scale = 1;
            _offset = 0;
            UpdateData();
        }

        public int Length
        {
            get { return _length - _blockLength + 1; }
        }
        public int DataBlockLength
        {
            get { return _blockLength; }
        }
        public double GetRate(int i)
        {
            return _loader[_startIndex + _blockLength - 1 + i].RealValue;
        }

        public void SetScale(double scale, double offset)
        {
            _scale = scale;
            _offset = offset;
            UpdateData();
        }
        public double Scale
        {
            get { return _scale; }
        }

        public double Offset
        {
            get { return _offset; }
        }

        public DataBlock GetNewBlock(int startIndex, int length)
        {
            DataBlock res = new DataBlock(_loader, _startIndex + startIndex, length, _blockLength);
            return res;
        }

        public int Copy(double[] array, int index)
        {
            int remain = _data.Length - index;
            int length = Math.Min(remain, _blockLength);

            if (length <= 0)
                return 0;

            Array.Copy(_data, index, array, 0, length);

            return length;
        }

        private void UpdateData()
        {
            for (int i = 0; i < _length; i++)
            {
                _data[i] = _loader[i + _startIndex].Value * _scale + _offset;
            }

        }
    }
}
