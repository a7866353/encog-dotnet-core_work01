﻿using MyProject01.ControllerFactorys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.TestCases.RateMarketTestCases
{
    class NormalRateMarketTestCase : BasicRateMarketTestCase
    {
        public NormalRateMarketTestCase()
        {
            NEATExchangeControllerFactory controllerFactory = new NEATExchangeControllerFactory();
            controllerFactory.InputLength = 
            controllerFactory.Get();
        }
    }
}
