using MyProject01.Util.DataObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.Util
{
    public class RateDataBlock : BasicDataBlock
    {
        private double _scale;
        private double _offset;

        public RateDataBlock(DataLoader loader, int startIndex, int length, int blockLength) 
            : base(loader, startIndex, length, blockLength)
        {
        }

        override public int Length
        {
            get { return _length - _blockLength + 1; }
        }
        override public int DataBlockLength
        {
            get { return _blockLength; }
        }
        override public double GetRate(int i)
        {
            return _loader[_startIndex + _blockLength - 1 + i].RealValue;
        }
        public override DateTime GetDate(int i)
        {
            return _loader[_startIndex + _blockLength - 1 + i].Date;
        }
        override public BasicDataBlock GetNewBlock(int startIndex, int length)
        {
            RateDataBlock res = new RateDataBlock(_loader, _startIndex + startIndex, _blockLength + length - 1, _blockLength);
            return res;
        }

        override public int Copy(double[] array, int index)
        {
            int remain = _data.Length - index;
            int length = Math.Min(remain, _blockLength);

            if (length <= 0)
                return 0;

            Array.Copy(_data, index, array, 0, length);

            return length;
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

        private void UpdateData()
        {
            for (int i = 0; i < _length; i++)
            {
                _data[i] = _loader[i + _startIndex].Value * _scale + _offset;
            }

        }

        public override BasicDataBlock Clone()
        {
            return (BasicDataBlock)MemberwiseClone();
        }

    }
}
