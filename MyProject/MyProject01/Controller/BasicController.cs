using Encog.ML;
using Encog.ML.Data;
using Encog.ML.Data.Basic;
using MyProject01.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.Controller
{
    public interface IController
    {
        int TotalLength { get; }
        int CurrentPosition { get; set; }
        void Init();
        MarketActions GetAction();
        IController Clone();

        IDataSource DataSource { set; }

    }
    class BasicController : IController
    {
        private ISensor _sensor;
        private IActor _actor;
        private Normalizer[] _normalizerArray;
        private BasicMLData _inData;
        private DataBlock _inDataArr;
        private IMLRegression _neuroNetwork;
        private int _currentPosition;
        public BasicController(ISensor sensor, IActor actor)
        {
            _sensor = sensor;
            _actor = actor;
            _currentPosition = _sensor.SkipCount;
        }

        public void Init()
        {
            _inDataArr = new DataBlock(NetworkInputVectorLength);
            _inData = new BasicMLData(_inDataArr.Data, false);
            _currentPosition = _sensor.SkipCount;
        }
        public int SkipCount
        {
            get { return _sensor.SkipCount; }
        }
        public int TotalLength
        {
            get { return _sensor.TotalLength; }
        }

        public int CurrentPosition
        {
            get
            {
                return _currentPosition;
            }
            set
            {
                _currentPosition = value;
            }
        }

        public Normalizer[] NormalizerArray
        {
            set { _normalizerArray = value; }
        }

        public MarketActions GetAction()
        {
            _sensor.Copy(_currentPosition, _inDataArr, 0);
            
            // Normalize
            for(int i=0;i<_inDataArr.Length;i++)
            {
                _inDataArr[i] = _normalizerArray[i].Convert(_inDataArr[i]);
            }

            IMLData output = _neuroNetwork.Compute(_inData);

            MarketActions result = _actor.GetAction(output);
            return result;
        }

        public void UpdateNetwork(IMLRegression network)
        {
            _neuroNetwork = network;
        }

        public int NetworkInputVectorLength
        {
            get { return _sensor.DataBlockLength; }
        }

        public int NetworkOutputVectorLenth
        {
            get { return _actor.DataLength; }
        }

        public IController Clone()
        {
            IController ctrl = (IController)MemberwiseClone();
            ctrl.Init();
            return ctrl;
        }
        public IDataSource DataSource
        {
            set { _sensor.DataSource = value; }
            get { return _sensor.DataSource; }
        }
        public ControllerPacker GetPacker()
        {
            ControllerPacker packer = new ControllerPacker(_sensor, _actor, _neuroNetwork, _normalizerArray);
            return packer;
        }

        public void Normilize(double middleValue, double margin)
        {
            FwtDataNormalizer norm = new FwtDataNormalizer();
            DataBlock buffer = new DataBlock(NetworkInputVectorLength);


            _sensor.Copy(SkipCount, buffer, 0);
            norm.Init(buffer.Data, middleValue, margin);

            for (int i = SkipCount+1; i < TotalLength;i++ )
            {
                _sensor.Copy(i, buffer, 0);
                norm.Set(buffer.Data);



            }

            _normalizerArray = norm.NromalizerArray;
        }
    }
}
