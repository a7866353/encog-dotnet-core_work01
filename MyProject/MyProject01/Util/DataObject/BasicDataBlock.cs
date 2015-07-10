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
        protected int _dataBufferLength;
        protected int _blockLen;

        protected double[] _data;

        public BasicDataBlock(DataLoader loader, int startIndex, int blockCount, int blockLength)
        {
            _blockIndex = 0;
            _loader = loader;
            _startIndex = startIndex;
            _dataBufferLength = Math.Min(loader.Count - _startIndex, blockCount);
            _data = new double[_dataBufferLength];
            _blockLen = blockLength;
            UpdateData();
        }

        protected int _blockLength
        {
            get { return _blockLen; }
        }

        private void UpdateData()
        {
            for (int i = 0; i < _data.Length; i++)
            {
                _data[i] = _loader[i + _startIndex].Close;
            }

        }

        protected int _blockIndex;
        
        public void Reset()
        {
            _blockIndex = 0;
        }
        public bool Next()
        {
            if ((_blockIndex + 1) >= BlockCount)
                return false;
            else
            {
                _blockIndex++;
                return true;
            }
        }

        public int Copy(double[] array)
        {
            return Copy(array, _blockIndex);
        }


        abstract public int BlockCount
        {
            get;
        }

        abstract public int BlockLength
        {
            get;
        }

        abstract public double GetRate(int i);
        abstract public DateTime GetDate(int i);

        abstract public int Copy(double[] array, int index);

        abstract public BasicDataBlock GetNewBlock(int startIndex, int length);


        abstract public BasicDataBlock Clone();
       
    }
}
