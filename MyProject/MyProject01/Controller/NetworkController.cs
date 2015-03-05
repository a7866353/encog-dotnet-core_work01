using Encog.ML;
using Encog.ML.Data;
using Encog.ML.Data.Basic;
using Encog.ML.EA.Train;
using Encog.ML.Train.Strategy;
using Encog.Neural.NEAT;
using Encog.Neural.Networks.Training;
using MyProject01.Agent;
using MyProject01.DAO;
using MyProject01.TestCases;
using MyProject01.Util.DllTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.Controller
{
    public class NetworkController
    {
        private NEATPopulation _population;
        public IMLRegression BestNetwork;

        private ControllerDAO _dao;
        public NEATPopulation GetPopulation()
        {
           
            if (_population == null)
            {
                _population = new NEATPopulation(InputVectorLength, OutputVectorLength, PopulationNumeber);
                _population.InitialConnectionDensity = 1.0;
                _population.WeightRange = 0.1;
                _population.Reset();

                 // _population.InitialConnectionDensity = 1.0; // not required, but speeds processing.
            }
            return _population;

        }

        public string Name
        {
            get { return _dao.Name; }
        }
        public int InputVectorLength
        {
            set { _dao.InputVectorLength = value; }
            get { return _dao.InputVectorLength; }
        }
        public InputDataFormaterType InputType
        {
            set { _dao.InputType = value; }
            get { return _dao.InputType; }
        }
        public OutputDataConvertorType OutType
        {
            set { _dao.OutType = value; }
            get { return _dao.OutType; }
        }
        public int PopulationNumeber
        {
            set { _dao.PopulationNumeber = value; }
            get { return _dao.PopulationNumeber; }
        }
        public double DataOffset
        {
            set { _dao.DataOffset = value; }
            get { return _dao.DataOffset; }
        }
        public double DataScale
        {
            set { _dao.DataScale = value; }
            get { return _dao.DataScale; }
        }
        public static NetworkController Open(string name, bool isNew = false, bool needPopulation = true)
        {
            ControllerDAO dao = ControllerDAO.GetDAO(name, isNew);
            NetworkController controller;

            if (dao.BestNetwork == null)
            {
                controller = new NetworkController(dao);
                controller.InputVectorLength = controller.PopulationNumeber = -1;
                controller.InputType = InputDataFormaterType.None;
                controller.OutType = OutputDataConvertorType.None;

            }
            else
            {
                controller = new NetworkController(dao);
                if (needPopulation == true)
                    controller._population = dao.GetPopulation();
                controller.BestNetwork = dao.GetBestNetwork();

            }

            return controller;
        }

        private NetworkController(ControllerDAO dao)
        {
            this._dao = dao;
        }
      
        public void Save()
        {
            _dao.SetBestNetwork(BestNetwork);
            _dao.Save();
            _dao.UpdatePopulation(_population);
        }

        public TradeDecisionController GetDecisionController()
        {
            TradeDecisionController ctl = new TradeDecisionController();
            ctl.BestNetwork = BestNetwork;
            ctl._inputFormater = InputDataFormaterFactor.Create(InputType, InputVectorLength);
            ctl._outputConvertor = OutputDataConvertorFactory.Create(OutType);
           
            return ctl;
        }
    }
}
