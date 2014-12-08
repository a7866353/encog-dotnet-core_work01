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
    [Serializable]
    class NEATController
    {
        private NEATPopulation _population;
        public NEATNetwork BestNetwork;

        public string Name;
        public int InputVectorLength;
        public int OutputVectorLength;
        public int PopulationNumeber;

        public static NEATController Open(string name)
        {
            ControllerDAO dao = ControllerDAO.GetDAO(name);
            NEATController controller;

            if (dao.Data == null)
            {
                controller = null;
            }
            else
            {
                MemoryStream stream = new MemoryStream(dao.Data);
                BinaryFormatter formatter = new BinaryFormatter();
                controller = (NEATController)formatter.Deserialize(stream);
            }

            return controller;
        }

        public NEATController(string name)
        {
            this.Name = name;
            InputVectorLength = OutputVectorLength = PopulationNumeber = -1;

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

        public NEATPopulation Population
        {
            get
            {
                if (_population == null)
                {
                    _population = new NEATPopulation(InputVectorLength, OutputVectorLength, PopulationNumeber);
                    _population.Reset();
                    _population.InitialConnectionDensity = 1.0; // not required, but speeds processing.
                }
                return _population;
            }
        }

        public void Save()
        {
            ControllerDAO dao = ControllerDAO.GetDAO(Name);
            dao.Data = ToByte();
            dao.Save();
        }

        private byte[] ToByte()
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, this);

            byte[] res = stream.ToArray();
            stream.Close();
            return res;
        }

        
    }
}
