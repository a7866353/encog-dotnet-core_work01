using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.Util
{
    interface IDescriptionProvider
    {
        string Description { get; }
    }

    class DescriptionProviderList : List<IDescriptionProvider>, IDescriptionProvider
    {
        public string Description
        {
            get 
            {
                string str = "";
                foreach(IDescriptionProvider desc in this)
                {
                    str += desc.Description + "_";
                }
                return str;
            }
        }
    }
}
