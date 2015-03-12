﻿using Encog.ML.Data;
using MyProject01.Agent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.Controller
{
    interface IOutputDataConvertor
    {
        MarketActions Convert(IMLData outData);
        int NetworkOutputLength { get; }
        IOutputDataConvertor Clone();

    }
}
