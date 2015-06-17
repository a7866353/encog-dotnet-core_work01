using MyProject01.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataImporter
{       
    class FengHuangDataObject : BasicDataObject
    {
        public String Data { set; get; }
        public double MiddleRate { set; get; }

    }
    class FengHuangDataImporter : BasicImporter
    {
         private string[] columnNames = new string[] { "Data", "MiddleRate" };
        private Type[] columnType = new Type[] { typeof(String), typeof(double) };


        protected override string CollectiongName
        {
            get { return "MiddleRate"; }
        }

        protected override BasicDataObject GetNextObject(string lineString)
        {
            string[] strArr = lineString.Split(' ');

            FengHuangDataObject obj = new FengHuangDataObject();
            obj.Data = strArr[0];
            obj.MiddleRate = double.Parse( strArr[1] );

            return obj;
        }
    }
}
