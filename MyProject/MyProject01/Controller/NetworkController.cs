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
    class FWTFormater : IInputDataFormater
    {
        private double[] _buffer;
        private double[] _tempBuffer;

        public BasicMLData Convert(double[] rateDataArray)
        {
            if (_buffer == null || _buffer.Length != rateDataArray.Length)
            {
                _buffer = new double[rateDataArray.Length];
                _tempBuffer = new double[rateDataArray.Length];
            }

            DllTools.FTW_2(rateDataArray, _buffer, _tempBuffer);


            MyProject01.Util.DataNormallizer adj = new MyProject01.Util.DataNormallizer();
            _buffer[0] = _buffer[1];
            adj.Set(_buffer, 0, _buffer.Length);
            adj.DataValueAdjust(-0.01, 0.01);

            return new BasicMLData(_buffer, false);
        }
    }

    class RateDataFormater : IInputDataFormater
    {
        private double[] _buffer;
        private double[] _tempBuffer;

        public BasicMLData Convert(double[] rateDataArray)
        {
            return new BasicMLData(rateDataArray, false);
        }
    }

    class TradeStateResultConvertor : IOutputDataConvertor
    {


        public MarketActions Convert(IMLData output)
        {
            MarketActions currentAction;
            // Choose an action
            int maxActionIndex = 0;
            for (int i = 1; i < output.Count; i++)
            {
                if (output[maxActionIndex] < output[i])
                    maxActionIndex = i;
            }

            // Do action
            switch (maxActionIndex)
            {
                case 0:
                    currentAction = MarketActions.Nothing;
                    break;
                case 1:
                    currentAction = MarketActions.Buy;
                    break;
                case 2:
                    currentAction = MarketActions.Sell;
                    break;
                default:
                    currentAction = MarketActions.Nothing;
                    break;
            }
            return currentAction;
        }
    }
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
