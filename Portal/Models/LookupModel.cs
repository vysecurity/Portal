using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.Models
{
    public class LookupModel
    {
        public String ReturnField { get; set; }
        public String Id { get; set; }
        public String Count { get; set; }
        public String IsMultiType { get; set; }
        public LocalizationModel LocalizationModel { get; set; }
    }
  
}