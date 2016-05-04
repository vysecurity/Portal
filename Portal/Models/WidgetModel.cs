using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.Models
{
    public class WidgetModel
    {
        public String name { get; set; }
        public String EntityName { get; set; }
        public String UseCache { get; set; }
        public String Zone { get; set; }
        public String GridRecordPerPage { get; set; }
        public String GridType { get; set; }
        public String iseditable { get; set; }
        public String IsSignature { get; set; }
        public String isownership { get; set; }
        public String GridOnClick { get; set; }
        public String GridHeaderColor { get; set; }
        public String GridHeaderFontColor { get; set; }
        public String CalculatedOnClick { get; set; }
        public String CalculatedTheme { get; set; }
        public String CalculatedIcon { get; set; }
        public String GridOnClickOpenFormId { get; set; }
        public String CalculatedOnClickOpenFormId { get; set; }
        public String GridOnClickOpenHTMLId { get; set; }
        public String GridOnClickWidgetType { get; set; }
        public String IsExcelExport { get; set; }
        public String GridClickOpenFormStyle { get; set; }
        public String RedirectAfterCreate { get; set; }
        public String id { get; set; }
        public String pagewidgetid { get; set; }
        public String order { get; set; }
        public String width { get; set; }
        public String color { get; set; }
        public String language { get; set; }
        public String PercentageofTotalWidth { get; set; }
        public List<SubGridsAndLookups> subgridsandlookups { get; set; }
        public List<InitialValues> initialvalues { get; set; }
        public List<Filters> filters { get; set; }
        public List<LookupFilter> lookupfilters { get; set; }
        public List<LookupLinks> lookuplinks { get; set; }
        public List<Actions> Actions { get; set; }

        public String ChartColor { get; set; }
        public String ChartFontSize { get; set; }
        public String ChartFontFamily { get; set; }
        public String Is3D { get; set; }
        public String ChartLegendPosition { get; set; }
        public String IsHandWrite { get; set; }
        public String LabelText { get; set; }

        public String PictureHeight { get; set; }

        public String GridLinkDescription { get; set; }
        public String ExternalLinkDescription { get; set; }
        public String FormLinkDescription { get; set; }
        public String ExternalLinkUrl { get; set; }
        public String IsExternalLink { get; set; }
        public String IsGridLink { get; set; }
        public String IsFormLink { get; set; }
        public String IsSeperate { get; set; }
        public String EntityNameForLinkWidget { get; set; }
        public String OnClickOpenFormIdForLinkWidget { get; set; }
        public String GridIcon { get; set; }
        public String GridBGColor { get; set; }
        public String FormIcon { get; set; }
        public String FormBGColor { get; set; }
        public String ExternalURLIcon { get; set; }
        public String ExternalURLBGColor { get; set; }

        public String Field1 { get; set; }
        public String Field2 { get; set; }
        public String Field3 { get; set; }

        public String NotificationDirection { get; set; }
    }
    public class NewWidgetModel
    {
        public String name { get; set; }
        public String EntityName { get; set; }
        public String GridRecordPerPage { get; set; }
        public String GridType { get; set; }
        public String iseditable { get; set; }
        public String isownership { get; set; }
        public String GridOnClick { get; set; }
        public String CalculatedOnClick { get; set; }
        public String GridOnClickOpenFormId { get; set; }
        public String CalculatedOnClickOpenFormId { get; set; }
        public String GridOnClickOpenHTMLId { get; set; }
        public String GridOnClickWidgetType { get; set; }
        public String GridClickOpenFormStyle { get; set; }
        public String id { get; set; }
        public String order { get; set; }
        public String width { get; set; }
        public String color { get; set; }
        public String language { get; set; }

    }

    public class Filters
    {
        public String type { get; set; }
        public List<Filter> filter { get; set; }
    }

    public class InitialValues
    {
        public String Static { get; set; }
        public String entitylogicalname { get; set; }
        public String attributelogicalname { get; set; }
        public String lookuplogicalname { get; set; }
        public String lookupnamevalue { get; set; }
        public String initialvalue { get; set; }
        public String iscomingfromsubgrids { get; set; }
    }

    public class SubGridsAndLookups
    {
        public String formentitylogicalname { get; set; }
        public String type { get; set; }
        public String SubGridViewId { get; set; }
        public String SubGridId { get; set; }
        public String SubGridLogicalName { get; set; }
        public String NewFormId { get; set; }
        public String UpdateFormId { get; set; }
        public String RelationShipName { get; set; }
        public String LookupLogicalName { get; set; }
        public String LookupEntityLogicalName { get; set; }
        public String OpenFormId { get; set; }
        public String LookupBehaviour { get; set; }
    }

    public class LookupLinks
    {
        public String LookupLogicalName { get; set; }
        public String LookupEntityLogicalName { get; set; }
        public String OpenFormId { get; set; }
        public String LookupBehaviour { get; set; }
        public String formentitylogicalname { get; set; }
        public String type { get; set; }
        public String Editable { get; set; }
    }
    public class Actions
    {
        public String Id { get; set; }
        public String DisplayName { get; set; }
        public String EntityLogicalName { get; set; }
        public String Order { get; set; }
        public String Color { get; set; }
        public String GridActions { get; set; }
        public String FormActions { get; set; }
        public String ReturnMessage { get; set; }
        public String WorkFlowId { get; set; }
        public String OpenFormId { get; set; }
        public String FontColor { get; set; }
        public String WidgetType { get; set; }
        public String ErrorReturnMessage { get; set; }
        public String IsEditable { get; set; }
        public String IsSignature { get; set; }
        public String UseOwnerShip { get; set; }
        public String OpenStyle { get; set; }
        public String OpeningWidgetType { get; set; } 
    }

    public class Filter
    {
        public String isstatic { get; set; }
        public String condition { get; set; }
        public String attributtename { get; set; }
        public String value { get; set; }
        public String uitype { get; set; }
        public String Operator { get; set; }
    }
    public class LookupFilter
    {
        public String logicalname { get; set; }
        public String entityname { get; set; }
        public String value { get; set; }
        public String valuelogicalname { get; set; }
        public String isstatic { get; set; }
        public String filtertype { get; set; }
        public String Operator { get; set; }
        public String iscustom { get; set; }
        public String fetchXml { get; set; }
    }
}