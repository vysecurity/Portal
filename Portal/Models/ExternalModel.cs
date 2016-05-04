using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.Models
{
    public class ExternalModel
    {
        public String PageId { get; set; }
        public String LangId { get; set; }
        public List<QueryStrings> QueryStrings { get; set; }
    }

    public class QueryStrings
    {
        public String Key { get; set; }
        public String Value { get; set; }
    }
}