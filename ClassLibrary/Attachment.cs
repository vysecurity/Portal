using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    public class Attachment
    {
        
        public String MimeType { get; set; }
        
        public String DocumentBody { get; set; }
        
        public String FileName { get; set; }
        
        public String EntityName { get; set; }
        
        public String RecordId { get; set; }
        
        public String Subject { get; set; }
        
        public String AttachmentId { get; set; }
    }
}
