﻿using MyProject01.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.Factorys.ControllerFactorys
{
    class NEATFWTStateKeepControllerFactory : BasicControllerFactory
    {
        public NEATFWTStateKeepControllerFactory()
        {
            Name = Description;
        }
        protected override NetworkController Create()
        {
            NetworkController controller;

            TradeDecisionController decisionCtrl = new TradeDecisionController();
            decisionCtrl._inputFormater = new FWTFormater(_inputLength);
            decisionCtrl._outputConvertor = new TradeStateKeepConvertor();
            decisionCtrl.BestNetwork = null;

            controller = NetworkController.Create(Name, decisionCtrl);
            return controller;           
        }

        public override string Description
        {
            get { return "FWT_StateKeep"; }
        }
    }

    class NEATFWTStateSwitchControllerFactory : BasicControllerFactory
    {
        public NEATFWTStateSwitchControllerFactory()
        {
            Name = Description;
        }
        protected override NetworkController Create()
        {
            NetworkController controller;

            TradeDecisionController decisionCtrl = new TradeDecisionController();
            decisionCtrl._inputFormater = new FWTFormater(_inputLength);
            decisionCtrl._outputConvertor = new TradeStateSwitchConvertor();
            decisionCtrl.BestNetwork = null;

            controller = NetworkController.Create(Name, decisionCtrl);
            return controller;
        }

        public override string Description
        {
            get { return "FWT_StateSwitch"; }
        }
    }
}
