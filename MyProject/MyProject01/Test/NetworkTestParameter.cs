using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyProject01.Test
{
    public class NetworkTestParameter
    {
        public string name;
        public double errorlimit = 0.001;
        public double hidenLayerRaio = 10000;
        public int retryCnt;
        public int hidenLayerNum;

        public NetworkTestParameter(string name, double error, double hidenLayerRaio, int retryCnt)
        {
            this.name = name;
            this.errorlimit = error;
            this.hidenLayerRaio = hidenLayerRaio;
            this.retryCnt = retryCnt;
        }
        public NetworkTestParameter(string name, int hidenLayerNum)
        {
            this.name = name;
            this.hidenLayerNum = hidenLayerNum;
        }

        public override string ToString()
        {
            return "Err:" + errorlimit.ToString("G2") + " NeroRaio:" + hidenLayerRaio.ToString();
        }
    }
}
