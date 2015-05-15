﻿using Encog.ML;
using Encog.ML.Data;
using Encog.ML.Data.Basic;
using Encog.ML.EA.Train;
using Encog.ML.Train.Strategy;
using Encog.Neural.NEAT;
using Encog.Neural.Networks.Training;
using MyProject01.Agent;
using MyProject01.DAO;
using MyProject01.TestCases;
using MyProject01.Util.DllTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.Controller
{
    public class NetworkController
    {
        private ITradeDesisoin _tradeDecisionController;
        public IMLRegression BestNetwork;

        private ControllerDAO _dao;

        public string Name
        {
            get { return _dao.Name; }
        }
        public string Description
        {
            set { _dao.Description = value; }
            get { return _dao.Description; }
        }
        public double DataOffset
        {
            set { _dao.DataOffset = value; }
            get { return _dao.DataOffset; }
        }
        public double DataScale
        {
            set { _dao.DataScale = value; }
            get { return _dao.DataScale; }
        }
        public static NetworkController Open(string name)
        {
            ControllerDAO dao = ControllerDAO.GetDAO(name, false);
            NetworkController controller;

            if (dao.NetworkData == null)
            {
                return null;

            }
            else
            {
                return null;
                // controller = new NetworkController(dao);

            }

            return controller;
        }

        public static NetworkController Create(string name, ITradeDesisoin ctrl)
        {
            ControllerDAO dao = ControllerDAO.GetDAO(name, true);
            NetworkController controller = new NetworkController(dao, ctrl);

            return controller;
        }

        // Get from saved data.
        private NetworkController(ControllerDAO dao)
        {
            this._dao = dao;
            this._tradeDecisionController = _dao.GetTradeDecisionController();
        }
        // Create a new one.
        private NetworkController(ControllerDAO dao, ITradeDesisoin ctrl)
        {
            this._dao = dao;
            this._tradeDecisionController = ctrl;
        }
          
        public void Save()
        {
           // TODO
           //  _dao.SetTradeDecisionController(_tradeDecisionController);
            _dao.SetNetwork(BestNetwork);

            _dao.UpdateTime = DateTime.Now;
            _dao.Save();
        }

        public ITradeDesisoin GetDecisionController()
        {
            return _tradeDecisionController.Clone() ;
        }
    }
}
