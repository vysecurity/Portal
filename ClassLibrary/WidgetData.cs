using System;
using System.Collections.Generic;

namespace ClassLibrary
{
    public class WidgetData
    {
        public List<ChartData> ChartData { get; set; }

        public List<CalendarData> Values { get; set; }

        public String WidgetId { get; set; }

        public String PageWidgetId { get; set; }

        public String Count { get; set; }

        public String PercentageofTotalWidth { get; set; }

        public String WidgetType { get; set; }

        public String width { get; set; }

        public String iseditable { get; set; }

        public String isownership { get; set; }

        public String language { get; set; }

        public String color { get; set; }

        public String name { get; set; }

        public String Zone { get; set; }

        public String GridType { get; set; }

        public List<GridRowData> GridData { get; set; }

        public FormLayout FormLayout { get; set; }

        public String Order { get; set; }

        public String IsSignature { get; set; }

        public String RedirectAfterCreate { get; set; }

        public String GridOnClick { get; set; }

        public String CalculatedOnClick { get; set; }

        public String GridOnClickOpenFormId { get; set; }

        public String GridOnClickOpenHTMLId { get; set; }

        public String GridOnClickWidgetType { get; set; }

        public String CalculatedOnClickOpenFormId { get; set; }

        public String CalculatedTheme { get; set; }

        public String CalculatedIcon { get; set; }

        public String GridClickOpenFormStyle { get; set; }

        public String GridRecordPerPage { get; set; }

        public String IsExcelExport { get; set; }

        public String FormId { get; set; }

        public String WidgetGuid { get; set; }

        public String EntityName { get; set; }

        public String HTML { get; set; }

        public String ChartType { get; set; }

        public List<LookupLink> LookupLinks { get; set; }

        public List<Actions> Actions { get; set; }

        public String ChartColor { get; set; }

        public String ChartFontSize { get; set; }

        public String ChartFontFamily { get; set; }

        public String Is3D { get; set; }

        public String ChartLegendPosition { get; set; }

        public String IsHandWrite { get; set; }

        public String LabelText { get; set; }

        public List<PictureData> PictureData { get; set; }

        public String GridLinkDescription { get; set; }

        public String ExternalLinkDescription { get; set; }

        public String FormLinkDescription { get; set; }

        public String ExternalLinkUrl { get; set; }

        public String IsExternalLink { get; set; }

        public String IsGridLink { get; set; }

        public String IsFormLink { get; set; }

        public String EntityNameForLinkWidget { get; set; }

        public String OnClickOpenFormIdForLinkWidget { get; set; }

        public String GridIcon { get; set; }

        public String GridBGColor { get; set; }

        public String FormIcon { get; set; }

        public String FormBGColor { get; set; }

        public String ExternalURLIcon { get; set; }

        public String ExternalURLBGColor { get; set; }

        public String IsSeperate { get; set; }

        public FieldInfo FieldInfo { get; set; }

        public List<String> NotificationList { get; set; }

        public String NotificationDirection { get; set; }

        public String GridHeaderColor { get; set; }

        public String GridHeaderFontColor { get; set; }
    }
}