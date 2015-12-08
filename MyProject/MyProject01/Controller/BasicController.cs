using Encog.ML;
using Encog.ML.Data;
using Encog.ML.Data.Basic;
using MyProject01.DataSources;
using MyProject01.DataSources.DataSourceParams;
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
        MarketActions GetAction();
        IController Clone();

        DataSourceCtrl DataSourceCtrl { set; }

    }

    public class ControllerFactory
    {
        public IController BaseController;

        public ControllerFactory(IController baseController)
        {
            this.BaseController = baseController;
        }
        public IController Get()
        {
            IController ctrl = BaseController.Clone();
            return ctrl;
        }
        public void Free(IController ctrl)
        {
            // TODO Nothing
        }
    }

    class BasicController : IController
    {
        private ISensor _sensor;
        private IActor _actor;
        private Normalizer[] _normalizerArray;

        private BasicMLData _inData;
        private DataBlock _inDataArr;
        private int _currentPosition;

        private DataSourceCtrl _dataSourceCtrl;
        private IMLRegression _neuroNetwork;
        private IDataSource _dataSource;
        public BasicController(ISensor sensor, IActor actor)
        {
            _sensor = sensor;
            _actor = actor;
            _currentPosition = 0;
            _inDataArr = new DataBlock(NetworkInputVectorLength);
            _inData = new BasicMLData(_inDataArr.Data, false);
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
            BasicController ctrl = (BasicController)MemberwiseClone();
            ctrl._sensor = _sensor.Clone();
            ctrl._actor = _actor.Clone();
            // ctrl._normalizerArray = _normalizerArray.Clone() as Normalizer[];

            _inDataArr = new DataBlock(NetworkInputVectorLength);
            _inData = new BasicMLData(_inDataArr.Data, false);
            _currentPosition = _sensor.SkipCount;
            return ctrl;
        }
        public DataSourceCtrl DataSourceCtrl
        {
            set
            {
                _dataSourceCtrl = value; 
                _sensor.DataSourceCtrl = value;

                RateDataSourceParam param = new RateDataSourceParam(5);
                _dataSource = _dataSourceCtrl.Get(param);
                _currentPosition = _sensor.SkipCount;
            }
            get
            {
                return _dataSourceCtrl;
            }
        }
        public IDataSource DataSource
        {
            get { return _dataSource; }
        }
        public ControllerPacker GetPacker()
        {
            ControllerPacker packer = new ControllerPacker(_sensor, _actor, _neuroNetwork, _normalizerArray);
            return packer;
        }

        public void Normilize(double middleValue, double limit)
        {
            FwtDataNormalizer norm = new FwtDataNormalizer();
            DataBlock buffer = new DataBlock(NetworkInputVectorLength);


            _sensor.Copy(SkipCount, buffer, 0);
            norm.Init(buffer.Data, middleValue, limit);

            for (int i = SkipCount+1; i < TotalLength;i++ )
            {
                _sensor.Copy(i, buffer, 0);
                norm.Set(buffer.Data);
            }

            _normalizerArray = norm.NromalizerArray;
        }

        public int GetIndexByTime(DateTime time)
        {
            return _dataSourceCtrl.GetIndexByTime(time);
        }

    }
}
