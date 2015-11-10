﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.DataSources
{
    interface IDataSourceParam
    {
        bool CompareTo(IDataSourceParam param);
        IDataSource Create(DataSourceCtrl ctrl);
    }
}
