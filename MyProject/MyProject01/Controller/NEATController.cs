using Encog.ML.Data;
using Encog.ML.Data.Basic;
using Encog.ML.EA.Train;
using Encog.ML.Train.Strategy;
using Encog.Neural.NEAT;
using Encog.Neural.Networks.Training;
using MyProject01.DAO;
using MyProject01.TestCases;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.Controller
{
    class NEATController
    {
        private NEATPopulation _population;
        public NEATNetwork BestNetwork;

        public ControllerDAO Dao;
        public NEATPopulation GetPopulation()
        {
           
            if (_population == null)
            {
                _population = new NEATPopulation(InputVectorLength, OutputVectorLength, PopulationNumeber);
                _population.Reset();

                // _population.InitialConnectionDensity = 1.0; // not required, but speeds processing.
            }
            return _population;

        }

        public string Name
        {
            get { return Dao.Name; }
        }
        public int InputVectorLength
        {
            set { Dao.InputVectorLength = value; }
            get { return Dao.InputVectorLength; }
        }
        public int OutputVectorLength
        {
            set { Dao.OutputVectorLength = value; }
            get { return Dao.OutputVectorLength; }
        }
        public int PopulationNumeber
        {
            set { Dao.PopulationNumeber = value; }
            get { return Dao.PopulationNumeber; }
        }

        public static NEATController Open(string name, bool isNew = false)
        {
            ControllerDAO dao = ControllerDAO.GetDAO(name, isNew);
            NEATController controller;

            if (dao.BestNetwork == null)
            {
                controller = new NEATController(dao);
                controller.InputVectorLength = controller.OutputVectorLength = controller.PopulationNumeber = -1;

            }
            else
            {
                controller = new NEATController(dao);
                controller._population = dao.GetPopulation();
                controller.BestNetwork = dao.GetBestNetwork();

            }

            return controller;
        }

        private NEATController(ControllerDAO dao)
        {
            this.Dao = dao;
        }
        public double[] Compute(double[] input)
        {
            if (BestNetwork == null)
                return null;
            IMLData output = BestNetwork.Compute(new BasicMLData(input, false));
            double[] outputArr = new double[output.Count];
            for(int i=0; i<outputArr.Length; i++)
                outputArr[i] = output[i];
            return outputArr;
        }
        public void Save()
        {
            Dao.SetBestNetwork(BestNetwork);
            Dao.Save();
            Dao.UpdatePopulation(_population);
        }

    }
}
