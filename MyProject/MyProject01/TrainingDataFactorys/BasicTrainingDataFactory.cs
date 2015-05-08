using MyProject01.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.TrainingDataFactorys
{
    abstract class BasicTrainingDataFactory
    {
        public int DataBlockLength;
        public TrainingData Get()
        {
            return Create();
        }

        abstract protected TrainingData Create();
        abstract public string GetDesc();
    }
}
