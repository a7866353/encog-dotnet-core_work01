using MyProject01.Test;
using MyProject01.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyProject01
{
    abstract class BasicTestCase
    {
        public string TestName = "DefaultTestCase000";
        protected TestData _testData;
        protected LogWriter _logger;
        protected bool isParameterSet = false;
        protected void LogPrintf(string str)
        {
            _logger.WriteLine(str);
            LogFile.WriteLine(str);
        }

        #region None Protected Code

        public void SetParmVar(MyNet net, TestData testData, LogWriter log)
        {
            /*
            this._logger = log;
            this._network = net;
            this._testData = testData;

            isParameterSet = true;
            */
        }

        public abstract void RunTest();

        #endregion

    }
}
