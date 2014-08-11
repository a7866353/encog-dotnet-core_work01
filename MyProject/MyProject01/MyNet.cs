using Encog.ML.Data;
using Encog.ML.Data.Basic;
using Encog.Neural.Networks;
using MyProject01.Test;
using MyProject01.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyProject01
{
    class MyNet
    {
        public BasicNet net;
        public NetworkTestParameter parm;
        public BasicTrainingMethod method;

        //-----------------------------------------------
        private BasicNetwork _network;

        public MyNet(BasicNet net, BasicTrainingMethod method, NetworkTestParameter parm)
        {
            this.net = net;
            this.method = method;
            this.parm = parm;
        }

        public void Rest()
        {
            this._network = null;
        }

        public void Training(TestData testData)
        {
            parm.InputSize = testData.InputSize;
            parm.OutputSize = testData.OutputSize;

            // Get Network
            if( _network == null )
            {
                _network = net.GetNet(parm);
            }
           
            // Trainning
            IMLDataSet trainingSet = new BasicMLDataSet(testData.TrainInputs, testData.TrainIdeaOutputs);
            method.TrainNetwork(_network, trainingSet);

        }

        public void Compute(MyTestDataList dataList)
        {
            foreach (MyTestData data in dataList)
            {
                IMLData res = _network.Compute(new BasicMLData(data.Input));
                double[] realArr = new double[data.OutputSize];
                res.CopyTo(realArr, 0, data.OutputSize);
                data.Real = realArr;
            }
        }
        public void Save()
        {
            // TODO:
        }

    }
}
