using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    public  class Tabs
    {
        
        public String Visible { get; set; }
        
        public String EntityName { get; set; }
        
        public String Label { get; set; }
        
        public String Name { get; set; }
        
        public String ShowLabel { get; set; }
        
        public String Expanded { get; set; }
        
        public List<Columns> Columns { get; set; }
        
        public List<FormDataDetail> FormData { get; set; }
        
        public bool IsBpf { get; set; }
        
        public List<BusinessProcessFlow> BusinessProcessFlowList { get; set; }
        
        public BusinessRuleContainer BusinessRules { get; set; }
        
        public bool IsBr { get; set; }
    }
}
