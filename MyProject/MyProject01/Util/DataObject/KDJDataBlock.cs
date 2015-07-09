using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.Util.DataObject
{
    class KDJDataBlock : BasicDataBlock
    {
        private double[] _rateArr;
        private double[] _kArr;
        private double[] _dArr;
        private double[] _jArr;


        public KDJDataBlock(DataLoader loader, int startIndex, int length, int blockLength) 
            : base(loader, startIndex, length, blockLength)
        {
        }
        public override int BlockCount
        {
            get { throw new NotImplementedException(); }
        }

        public override int BlockLength
        {
            get { throw new NotImplementedException(); }
        }

        public override double GetRate(int i)
        {
            throw new NotImplementedException();
        }

        public override DateTime GetDate(int i)
        {
            throw new NotImplementedException();
        }

        public override int Copy(double[] array, int index)
        {
            throw new NotImplementedException();
        }

        public override BasicDataBlock GetNewBlock(int startIndex, int length)
        {
            throw new NotImplementedException();
        }

        public override BasicDataBlock Clone()
        {
            throw new NotImplementedException();
        }
    }
}
