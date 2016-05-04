using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.Models
{
    public class ExtraModel
    {
        public String WidgetId { get; set; }
        public String PageWidgetId { get; set; }
        public String DataId { get; set; }
        public String ErrorMessage { get; set; }
        public String Editable { get; set; }
        public String Ownership { get; set; }
        public String FormOpenType { get; set; }
        public String LoggedUserGuid { get; set; }
        public String IsComingFromSubGrid { get; set; }
        public String IsComingLookupDetail { get; set; }
        public String PortalId { get; set; }
        public String PortalLanguage { get; set; }
        public String IsExternalWidget { get; set; }
        public String MailAdress { get; set; }
        public String IsComingFromPersonel { get; set; }
        public String IsEditFormInNewWindow { get; set; }
        public String IsComingFromErrorPage { get; set; }
    }
}