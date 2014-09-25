using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyProject01.Test
{
    public class NetworkTestParameter
    {
        //-----------------------------------------------
        // Common
        public string name { get; set;}
        public double errorlimit = 0.001;
        public double hidenLayerRaio = 10000;
        public int retryCnt { get; set; }

        //-----------------------------------------------
        // Network
        public int InputSize;
        public int OutputSize;
        public int HidenLayerNum;

        //-----------------------------------------------
        // Test

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
            this.HidenLayerNum = hidenLayerNum;
        }

        public override string ToString()
        {
            return "Err:" + errorlimit.ToString("G2") + " NeroRaio:" + hidenLayerRaio.ToString();
        }
    }
}
