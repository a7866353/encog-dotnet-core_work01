using Encog.ML;
using Encog.ML.Data;
using Encog.ML.Data.Basic;
using Encog.ML.EA.Train;
using Encog.ML.Train.Strategy;
using Encog.Neural.NEAT;
using Encog.Neural.Networks.Training;
using Encog.Util;
using Encog.Util.Banchmark;
using Encog.Util.Simple;
using MyProject01.Agent;
using MyProject01.DAO;
using MyProject01.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyProject01.DAO;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using MyProject01.Controller;

namespace MyProject01.TestCases
{

    class RateMarketNEATTest : BasicTestCase
    {
        public string TestName = "DefaultTest000";

        NEATTrainer _train = new NEATTrainer();

        public RateMarketNEATTest()
        {
            _train = new NEATTrainer();

        }

        public override void RunTest()
        {
            double testDataRate = 0.7;
            _train.SetDataLength(0, (int)(NEATTrainer._dataLoader.Count / 4 * testDataRate), NEATTrainer._dataLoader.Count / 4, 3600);

            NEATController controller = new NEATController();
            _train.Controller = controller;
            _train.IterationCount = 1000;
            _train.RunTestCase();
        }

    }
}
