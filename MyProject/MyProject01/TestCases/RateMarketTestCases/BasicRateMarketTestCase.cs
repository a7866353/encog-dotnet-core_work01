using MyProject01.ControllerFactorys;
using MyProject01.Controller.TrainerFactorys;
using MyProject01.TrainerFactorys;
using MyProject01.TrainingDataFactorys;
using MyProject01.Util.DataObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyProject01.Controller;

namespace MyProject01.TestCases.RateMarketTestCases
{
    abstract class BasicRateMarketTestCase : BasicTestCase
    {
        public int DataBlockLength;

        protected Trainer _train;
        abstract protected void Init();


        public override void RunTest()
        {
            Init();
            _train.RunTestCase();
        }
    }
}
