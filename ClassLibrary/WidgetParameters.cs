using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    public class WidgetParameters
    {
        
        public String WidgetId { get; set; }
        
        public String PageWidgetId { get; set; }
        
        public String Order { get; set; }
        
        public String GridPerPage { get; set; }
        
        public String ActionsCount { get; set; }
        
        public String PercentageofTotalWidth { get; set; }
        
        public List<Filters> filters { get; set; }
        
        public List<SubGridModel> subgrids { get; set; }
        
        public List<QueryStrings> QueryStrings { get; set; }
        
        public String PictureHeight { get; set; }
    }
}
