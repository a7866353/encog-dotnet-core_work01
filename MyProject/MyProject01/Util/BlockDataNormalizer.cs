using MyProject01.Util.DataObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.Util
{
    class FwtDataNormalizer
    {
        private NormalizeAnalyzer[] _normAnlzArr;
        private int _length;

        public void Init(double[] firstDataArray, double middleValue, double margin)
        {
            _length = firstDataArray.Length;
            _normAnlzArr = new NormalizeAnalyzer[_length];
            for (int i = 0; i < _length; i++)
            {
                NormalizeAnalyzer na = new NormalizeAnalyzer();
                na.SetTarget(middleValue, margin);
                na.Init(firstDataArray[i]);
                _normAnlzArr[i] = na;
            }
        }

        public void Set(double[] dataArr)
        {
            for (int i = 0; i < _length; i++)
            {
                _normAnlzArr[i].Set(dataArr[i]);
            }
        }

        public Normalizer[] NromalizerArray
        {
            get
            {
                Normalizer[] normArr = new Normalizer[_length];
                for (int i = 0; i < _length; i++)
                {
                    normArr[i] = _normAnlzArr[i].Normalizer;
                }
                return normArr;
            }
        }
    }

    class BlockDataNormalizer
    {
        private double _scale = 1;
        private double _offset = 0;

        private const double _targetDataMargin = 0.25;
        private const double _targetDataMiddleValue = 0.5;

        public double Scacle
        {
            get { return _scale; }
        }
        public double Offset
        {
            get { return _offset; }
        }

        public void Normalize(BasicDataBlock dataBlock)
        {
            double[] buffer = new double[dataBlock.BlockLength];

            dataBlock.Copy(buffer);
            NormalizeAnalyzer norm = new NormalizeAnalyzer();
            norm.SetTarget(_targetDataMiddleValue, _targetDataMargin);
            norm.Init(buffer[0]);

            while(true)
            {
               foreach (double data in buffer)
                {
                    norm.Set(data);
                }

               if (dataBlock.Next() == false)
                   break;
            }
            _scale = norm.Scale;
            _offset = norm.Offset;
        }
    

    }

    class NormalizeAnalyzer
    {
        private double _targetDataMargin = 0.01;
        private double _targetDataMid = 0.5;

        private double _targDataMax;
        private double _targDataMin;

        private double _dataMaxValue;
        private double _dataMinValue;

        public void Set(double data)
        {
            if (data > _dataMaxValue)
                _dataMaxValue = data;
            else if (data < _dataMinValue)
                _dataMinValue = data;
        }

        public void SetTarget(double middleValue, double margin)
        {
            _targetDataMid = middleValue;
            _targetDataMargin = margin;

            _targDataMax = _targetDataMid + _targetDataMargin;
            _targDataMin = _targetDataMid - _targetDataMargin;
        }

        public void Init(double initdata)
        {
            _dataMaxValue = _dataMinValue = initdata;
        }

        // out = (in + Offset) * Scale;
        public double Scale
        {
            get { return (_targDataMax - _targDataMin) / (_dataMaxValue - _dataMinValue); }
        }
        public double Offset
        {
            get { return (_dataMaxValue - _dataMinValue) * _targDataMax / (_targDataMax - _targDataMin) - _dataMaxValue; }
        }

        public Normalizer Normalizer
        {
            get { return new Normalizer(Offset, Scale); }
        }
    }

    class Normalizer
    {
        public double Offset = 0;
        public double Scale = 1;

        public Normalizer(double offset, double scale)
        {
            this.Offset = offset;
            this.Scale = scale;
        }

        public double Convert(double value)
        {
            return (value + Offset) * Scale;
        }

        public override string ToString()
        {
            return "O"+Offset.ToString("G") + "|S" + Scale.ToString("G");
        }
    }

}
