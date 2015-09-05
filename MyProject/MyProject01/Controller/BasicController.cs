using Encog.ML;
using Encog.ML.Data;
using Encog.ML.Data.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.Controller
{
    interface IController
    {
        int TotalLength { get; }
        int CurrentPosition { get; set; }
        MarketActions GetAction();

    }
    class BasicController : IController
    {
        private SensorGroup _sensorGroup;
        private BasicActor _actor;
        private BasicMLData _inData;
        private double[] _inDataArr;
        private IMLRegression _neuroNetwork;
        public BasicController()
        {
            _sensorGroup = new SensorGroup();
        }

        public void Init()
        {
            _inDataArr = new double[TotalLength];
            _inData = new BasicMLData(_inDataArr, false);
        }
        public int TotalLength
        {
            get { throw new NotImplementedException(); }
        }

        public int CurrentPosition
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public MarketActions GetAction()
        {

            _sensorGroup.CurrentPosition = CurrentPosition;
            _sensorGroup.Copy(_inDataArr, 0);

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
            get { return _inputFormater.ResultDataLength; }
        }

        public int NetworkOutputVectorLenth
        {
            get { return _outputConvertor.NetworkOutputLength; }
        }
    }

    }
}
