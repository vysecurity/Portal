using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.Models
{
    public class LanguageModel
    {
        public String Label { get; set; }
        public String LangId { get; set; }
        public String BaseLangId { get; set; }
        public String NativeName { get; set; }
        public String BaseNativeName { get; set; }
        public String IsMain { get; set; }
        public String ConfigurationId { get; set; }
    }
}