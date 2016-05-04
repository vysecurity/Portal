using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    public class Sections
    {
        
        public String Visible { get; set; }
        
        public String ColumnLength { get; set; }
        
        public String ShowLabel { get; set; }
        
        public String Name { get; set; }
        
        public List<Rows> Rows { get; set; }
    }
}
