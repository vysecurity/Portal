using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    public class Rows
    {
        
        public List<Picklist> PicklistValues { get; set; }
        
        public String ShowLabel { get; set; }
        
        public String Visible { get; set; }
        
        public String Label { get; set; }
        
        public String ViewId { get; set; }
        
        public String ElementType { get; set; }
        
        public String DatePart { get; set; }
        
        public String Precision { get; set; }
        
        public String MaxValue { get; set; }
        
        public String MinValue { get; set; }
        
        public String RequiredLevel { get; set; }
        
        public String LookupLogicalName { get; set; }
        
        public String DisplayName { get; set; }
        
        public String Type { get; set; }
        
        public String RowSpan { get; set; }
        
        public String LogicalName { get; set; }
        
        public String Disabled { get; set; }
        
        public String ColSpan { get; set; }
        
        public String ClassId { get; set; }
        
        public String IsSpace { get; set; }
        
        public String DateFormat { get; set; }
        
        public String TimeFormat { get; set; }
        
        public String BeforeDateFormat { get; set; }
        
        public String BeforeTimeFormat { get; set; }
        
        public String UserSpacer { get; set; }
        
        public String SubGridTargetEntity { get; set; }
        
        public String RelationShipName { get; set; }
        
        public String SubGridId { get; set; }
        
        public List<String> CustomerLogicalName { get; set; }
        
        public List<Attachment> Attachments { get; set; }
        
        public List<SubGridModel> SubGrids { get; set; }
    }
}
