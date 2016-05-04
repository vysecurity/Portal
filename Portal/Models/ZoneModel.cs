using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.Models
{
    public class ZoneModel
    {
        public String IsHeaderZoneActive { get; set; }
        public String MainZoneWidth { get; set; }
        public String IsLeftZoneActive { get; set; }
        public String LeftZoneWidth { get; set; }
        public String IsRightZoneActive { get; set; }
        public String RightZoneWidth { get; set; }
        public String IsFooterZoneActive { get; set; }
    }
}