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
        public int OutputVectorLength
        {
            set { _dao.OutputVectorLength = value; }
            get { return _dao.OutputVectorLength; }
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
                controller.InputVectorLength = controller.OutputVectorLength = controller.PopulationNumeber = -1;

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

        public static NetworkController Open(IMLRegression network)
        {
            NetworkController controller = new NetworkController(null);
            controller.BestNetwork = network;
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
            return ctl;
        }
    }
}
