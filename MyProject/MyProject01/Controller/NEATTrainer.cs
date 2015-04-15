﻿using Encog.ML;
using Encog.ML.Data;
using Encog.ML.Data.Basic;
using Encog.ML.EA.Train;
using Encog.Neural.NEAT;
using Encog.Neural.Networks.Training;
using MyProject01.Agent;
using MyProject01.DAO;
using MyProject01.ExchangeRateTrade;
using MyProject01.Util;
using MyProject01.Util.DataObject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.Controller
{
  
    abstract class Trainer
    {
        public string TestName = "DefaultTest000";
        public TrainDataList DataList;
        public NetworkController Controller;
        public ITradeDesisoin DecisionCtrl;
        public TrainResultCheckController CheckCtrl;

        private long _epoch;


        protected TrainEA train;
        protected long Epoch
        {
            get { return _epoch; }
        }

        public Trainer()
        {
            DataList = new TrainDataList();
        }
        public void RunTestCase()
        {
            LogFile.WriteLine(@"Beginning training...");
            PrepareRunnTestCase();
            _epoch = 1;

            do
            {
                 try
                 {
                     train.Iteration();

                 }
                catch(Exception)
                 {
                     LogFile.WriteLine("Train Iteration Error!");
                 }
                 PostItration();
                 _epoch++;

            } while (true);
            train.FinishTraining();
            
            // test the neural network
            LogFile.WriteLine(@"Training end");
        }

        protected abstract void PrepareRunnTestCase();

        protected abstract void PostItration();
    }

    class LogFormater
    {
        private double[] _valueArray;
        public enum ValueName
        {
            Step = 0,
            TrainScore,
            UnTrainScore,
        }

        public LogFormater()
        {
            _valueArray = new double[Enum.GetValues(typeof(ValueName)).Length];
        }

        public string GetTitle()
        {
            string title = "";
            string[] arr = Enum.GetNames(typeof(ValueName));
            for (int i = 0; i < arr.Length; i++)
                title += arr[i].ToString() + "\t";
            return title;
        }

        public string GetLog()
        {
            string resStr = "";
            for (int i = 0; i < _valueArray.Length; i++)
                resStr += _valueArray[i].ToString("G6") + "    \t";
            return resStr;
        }

        public void Set(ValueName name, double v)
        {
            this._valueArray[(int)name] = v;
        }

    }

    class TrainingData
    {
        private BasicDataBlock _dataBlock;
        private int _testLength;

        public BasicDataBlock DataBlock
        {
            get { return _dataBlock; }
        }
        public int TestLength
        {
            get { return _testLength; }
        }


        public TrainingData(BasicDataBlock block, int testLength)
        {
            _dataBlock = block;
            _testLength = testLength;
        }

    }
    class TrainDataList : List<TrainingData>
    {
        private Random _rand;

        public TrainDataList()
        {
            _rand = new Random();
        }
        public TrainingData GetNext()
        {
            return this[_rand.Next(Count)];
        }
    }
}
