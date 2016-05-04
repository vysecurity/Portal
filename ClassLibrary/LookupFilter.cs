using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    public class LookupFilter
    {
        
        public String LogicalName { get; set; }
        
        public String EntityName { get; set; }
        
        public String Value { get; set; }
        
        public String ValueLogicalName { get; set; }
        
        public String IsStatic { get; set; }
        
        public String FilterType { get; set; }
        
        public String Operator { get; set; }
        
        public String IsCustom { get; set; }
        
        public String FetchXml { get; set; }
    }
}
