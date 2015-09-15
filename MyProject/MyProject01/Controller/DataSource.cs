using MyProject01.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.Controller
{
    interface IDataSource
    {
        void Copy(int index, DataBlock buffer, int offset, int length);
        int TotalLength{ get; }

        RateSet this[int index] { get; }

    }
    class FixDataSource : IDataSource
    {
        private DataLoader _loader;
        private DataBlock _dataBuffer;

        public FixDataSource(DataLoader loader)
        {
            _loader = loader;
            Init();
        }

        public void Copy(int index, DataBlock buffer, int offset, int length)
        {
            DataBlock.Copy(_dataBuffer, index, buffer, offset, length);
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

        private void Init()
        {
            _dataBuffer = new DataBlock(_loader.Count);
            for(int i=0;i<_dataBuffer.Length;i++)
            {
                _dataBuffer[i] = _loader[i].Close;
            }


        }
    }
}
