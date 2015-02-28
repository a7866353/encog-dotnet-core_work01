using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyProject01.Util.DataObject
{
    class FWTDataBlock : BasicDataBlock
    {
        private double[] _fwt_input;
        private double[] _fwt_output;
        private double[] _fwt_temp;
        private int _outputLength;
        private int _mode = 0; // 0:ALl 1: 2:

        public FWTDataBlock(DataLoader loader, int startIndex, int length, int blockLength) 
            : base(loader, startIndex, length, blockLength)
        {
            if (_mode == 0)
            {
                _outputLength = blockLength;
            }
            else
            {
                _outputLength = blockLength * 2;
            }
            _fwt_input = new double[_outputLength];
            _fwt_output = new double[_outputLength];
            _fwt_temp = new double[_outputLength];
        }

        public override int Length
        {
            get { return _length - _outputLength + 1; }
        }

        public override int DataBlockLength
        {
            get 
            {
                return _blockLength;
            }
        }

        public override double GetRate(int i)
        {
            return _loader[_startIndex + _outputLength - 1 + i].RealValue;
        }

        public override int Copy(double[] array, int index)
        {
            int remain = _data.Length - index;
            int length = Math.Min(remain, _outputLength);

            if (length <= 0)
                return 0;

            // double[] _fwt_input = new double[_blockLength];
            // double[] _fwt_temp = new double[_blockLength];

            Array.Copy(_data, index, _fwt_input, 0, length);

       
            // FWT



            DllTools.DllTools.FTW_2(_fwt_input, _fwt_output, _fwt_temp);


            DataNormallizer adj = new DataNormallizer();
            _fwt_output[0] = _fwt_output[1];
            adj.Set(_fwt_output, 0, _fwt_output.Length);
            adj.DataValueAdjust(-0.01, 0.01);
   
            _fwt_output[0] = 0;
            if(_mode == 2)
            {
                Array.Copy(_fwt_output, _fwt_output.Length/2 , array, 0, array.Length);

            }
            else
            {
                Array.Copy(_fwt_output, 0, array, 0, array.Length);
            }


            return length;
            
        }

        public override BasicDataBlock GetNewBlock(int startIndex, int length)
        {
            FWTDataBlock res = new FWTDataBlock(_loader, _startIndex + startIndex, length + _blockLength - 1, _blockLength);
            return res;
        }

        public override BasicDataBlock Clone()
        {
            FWTDataBlock res;
            Thread.BeginCriticalRegion();
            res = new FWTDataBlock(_loader, _startIndex, _length, _blockLength);
            Thread.EndCriticalRegion();
            return res;
        }
    }
}
