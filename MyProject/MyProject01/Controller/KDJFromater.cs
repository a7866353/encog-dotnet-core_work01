using Encog.ML.Data.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.Controller
{
    [Serializable]
    class KDJFormater : IInputDataFormater
    {
        private int _blockLength;
        public double[] _buffer;
        public KDJFormater(int blockLength)
        {
            _blockLength = blockLength;
            /* rate close price
             * k value
             * d value 
             * j value 
             * k d j Cross Check
             */
            _buffer = new double[blockLength * 5];
        }
        public int InputDataLength
        {
            get { return _buffer.Length; }
        }

        public int ResultDataLength
        {
            get { return _buffer.Length; }
        }

        public BasicMLData Convert(double[] rateDataArray)
        {
            MutiplayArraryReader reader = new MutiplayArraryReader(rateDataArray, _blockLength);

            Array.Copy(rateDataArray, _buffer, rateDataArray.Length);

            for(int i=0;i<_blockLength;i++)
            {
                int index = _blockLength * 4 + i;
                _buffer[index] = Math.Abs(reader.Get(1, i) - reader.Get(2, i))
                    + Math.Abs(reader.Get(1, i) - reader.Get(3, i))
                    + Math.Abs(reader.Get(2, i) - reader.Get(3, i));
            }

            return new BasicMLData(_buffer, false);
        }

        public IInputDataFormater Clone()
        {
            KDJFormater formater = new KDJFormater(_blockLength);
            return formater;
        }

        public string GetDecs()
        {
            return "KDJ";
        }
    }
    [Serializable]
    class KDJOnlyFormater : IInputDataFormater
    {
        private int _blockLength;
        public double[] _buffer;
        public KDJOnlyFormater(int blockLength)
        {
            _blockLength = blockLength;
            /* k value
             * d value 
             * j value 
             * k d j Cross Check
             */
            _buffer = new double[blockLength * 4];
        }
        public int InputDataLength
        {
            get { return _buffer.Length; }
        }

        public int ResultDataLength
        {
            get { return _buffer.Length; }
        }

        public BasicMLData Convert(double[] rateDataArray)
        {
            MutiplayArraryReader reader = new MutiplayArraryReader(rateDataArray, _blockLength);

            Array.Copy(rateDataArray, _blockLength, _buffer, 0, rateDataArray.Length - _blockLength);

            for (int i = 0; i < _blockLength; i++)
            {
                int index = _blockLength * 3 + i;
                _buffer[index] = Math.Abs(reader.Get(1, i) - reader.Get(2, i))
                    + Math.Abs(reader.Get(1, i) - reader.Get(3, i))
                    + Math.Abs(reader.Get(2, i) - reader.Get(3, i));
            }

            return new BasicMLData(_buffer, false);
        }

        public IInputDataFormater Clone()
        {
            KDJFormater formater = new KDJFormater(_blockLength);
            return formater;
        }

        public string GetDecs()
        {
            return "KDJOnly";
        }
    }

    class MutiplayArraryReader
    {
        private double[] _buffer;
        private int _dataWidth;
        private int _dataChannel;
        public MutiplayArraryReader(double[] buffer, int dataWidth)
        {
            this._buffer = buffer;
            this._dataWidth = dataWidth;
            _dataChannel = _buffer.Length / _dataWidth;
        }
        public double Get(int channel, int index)
        {
            if (channel > _dataChannel || index > _dataWidth)
                throw (new Exception("Error!"));
            return _buffer[index + channel * _dataWidth];
        }
    }

    

}
