using MyProject01.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.Controller
{
    public interface IDataSource
    {
        void Copy(int index, DataBlock buffer, int offset, int length);
        int TotalLength{ get; }

        RateSet this[int index] { get; }

    }
    class FixDataSource : IDataSource
    {
        private DataLoader _loader;
        private DataBlock _dataBuffer;
        private double _lengthLimit;

        public FixDataSource(DataLoader loader, double lengthLimit = 1.0)
        {
            _loader = loader;

            _dataBuffer = new DataBlock((int)(_loader.Count*lengthLimit));
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
}
