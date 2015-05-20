﻿using MyProject01.Controller;
using MyProject01.Util;
using MyProject01.Util.DataObject;
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

    class NEATFWTNromStateKeepControllerFactory : BasicControllerFactory
    {
        public BasicDataBlock NormalyzeData;
        public double MiddleValue = 0.5;
        public double Margin = 0.25;
        public NEATFWTNromStateKeepControllerFactory()
        {
            Name = Description;
        }
        protected override NetworkController Create()
        {
            NetworkController controller;

            TradeDecisionController decisionCtrl = new TradeDecisionController();

            FWTNormFormater form = new FWTNormFormater(_inputLength);
            form.Normilize(NormalyzeData, MiddleValue, Margin);
            decisionCtrl._inputFormater = form;
            decisionCtrl._outputConvertor = new TradeStateKeepConvertor();
            decisionCtrl.BestNetwork = null;

            controller = NetworkController.Create(Name, decisionCtrl);
            return controller;
        }


        public override string Description
        {
            get { return "FWT__"  + _inputLength.ToString() + "_Norm" + MiddleValue.ToString("G") + "|" + Margin.ToString("G") + "_StateKeep"; }
        }
    }

}
