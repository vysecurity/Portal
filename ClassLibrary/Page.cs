using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    public class Page
    {
        public String FormLayoutXml { get; set; }
        public String Scripts { get; set; }
        public String UseCache { get; set; }
        public String IsHeaderZoneActive { get; set; }
        public String MainZoneWidth { get; set; }
        public String IsLeftZoneActive { get; set; }
        public String LeftZoneWidth { get; set; }
        public String IsRightZoneActive { get; set; }
        public String RightZoneWidth { get; set; }
        public String IsFooterZoneActive { get; set; }
    }
}
