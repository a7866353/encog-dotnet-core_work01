﻿using MyProject01.Controller;
using MyProject01.Util;
using MyProject01.Util.DataObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.TrainingDataFactorys
{
    class OldRateTrainingDataFactory : BasicTrainingDataFactory
    {
        public double TestDataRate = 0.7;
        protected override TrainingData Create()
        {
            DataLoader loader;
            loader = new MTDataLoader("USDJPY", DataTimeType.Timer1Day);
            // loader.Fillter(new DateTime(2013, 1, 1), DateTime.Now);
           
            RateDataBlock testBlock = new RateDataBlock(loader, 0, loader.Count, DataBlockLength);
            TrainingData td = new TrainingData(
                new RateDataBlock(loader, 0, loader.Count, DataBlockLength),
                new RateDataBlock(loader, 0, (int)(loader.Count * TestDataRate), DataBlockLength)
                );

            return td;
        }

        public override string GetDesc()
        {
            return "USDJPY_1DAY";
        }
    }
}
