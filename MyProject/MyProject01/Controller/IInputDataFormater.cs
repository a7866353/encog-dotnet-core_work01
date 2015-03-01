using Encog.ML.Data.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.Controller
{
    interface IInputDataFormater
    {
        BasicMLData Convert(double[] rateDataArray);
    }
}
