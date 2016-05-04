using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    public class Language
    {
        public String BaseLangId { get; set; }
        public String LangId { get; set; }
        public String Label { get; set; }
        public String NativeName { get; set; }
        public String BaseNativeName { get; set; }
        public String IsMain { get; set; }
        public String ConfigurationId { get; set; }
    }
}
