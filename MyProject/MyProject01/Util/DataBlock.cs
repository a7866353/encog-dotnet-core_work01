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


        public DataBlock(DataLoader loader, int startIndex, int length)
        {
            _loader = loader;
            _startIndex = startIndex;
            _length = length;

            _data = new double[_length];
            _scale = 1;
            _offset = 0;
            UpdateData();
        }

        public int Length
        {
            get { return _length; }
        }
        public double this[int i]
        {
            get { return _data[i]; }
        }
        public RateSet GetObject(int i)
        {
            return _loader[_startIndex + i];
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


        public double[] GetArray(int startIndex, int length)
        {
            int remain = _data.Length - _startIndex;
            length = Math.Min(remain, length);

            if (length <= 0)
                return null;

            double[] resArr = new double[length];
            Array.Copy(_data, startIndex, resArr, 0, length);

            return resArr;
        }

        public int Copy(double[] array, int offset, int startIndex, int length)
        {
            int remain = _data.Length - _startIndex;
            length = Math.Min(remain, length);

            if (length <= 0)
                return 0;

            Array.Copy(_data, startIndex, array, offset, length);

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
