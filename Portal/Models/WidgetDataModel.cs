using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.Models
{
    public class WidgetDataModel
    {
        public String WidgetId { get; set; }
        public String PageWidgetId { get; set; }
        public String PercentageofTotalWidth { get; set; }
        public String EntityName { get; set; }
        public String Language { get; set; }
        public String WidgetGuid { get; set; }
        public String FormId { get; set; }
        public String Order { get; set; }
        public String IsSignature { get; set; }
        public String GridType { get; set; }
        public String IsEditable { get; set; }
        public String GridOnClick { get; set; }
        public String GridHeaderColor { get; set; }
        public String GridHeaderFontColor { get; set; }
        public String GridOnClickOpenFormId { get; set; }
        public String GridOnClickOpenHTMLId { get; set; }
        public String GridOnClickWidgetType { get; set; }
        public String CalculatedOnClick { get; set; }
        public String CalculatedOnClickOpenFormId { get; set; }
        public String CalculatedTheme { get; set; }
        public String CalculatedIcon { get; set; }
        public String GridRecordPerPage { get; set; }
        public String GridClickOpenFormStyle { get; set; }
        public String IsExcelExport { get; set; }
        public String RedirectAfterCreate { get; set; }
        public String Count { get; set; }
        public String WidgetType { get; set; }
        public String Width { get; set; }
        public String Zone { get; set; }
        public String IsOwnership { get; set; }
        public String Color { get; set; }
        public String Name { get; set; }
        public String HTML { get; set; }
        public String RelationShipName { get; set; }
        public String ParentId { get; set; }
        public String SubGridId { get; set; }      
        public FormLayout FormLayout { get; set; }
        public List<InitialValues> InitialValues { get; set; }
        public List<CalendarData> Values { get; set; }
        public List<ChartData> ChartData { get; set; }
        public List<GridRowData> GridData { get; set; }
        public List<FormDataDetail> FormDatas { get; set; }
        public List<LookupLinks> LookupLinks { get; set; }
        public List<Actions> Actions { get; set; }
        public String ChartType { get; set; }

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
        public String IsSeperate { get; set; }
        public String OnClickOpenFormIdForLinkWidget { get; set; }
        public String EntityNameForLinkWidget { get; set; }
        public String GridIcon { get; set; }
        public String GridBGColor { get; set; }
        public String FormIcon { get; set; }
        public String FormBGColor { get; set; }
        public String ExternalURLIcon { get; set; }
        public String ExternalURLBGColor { get; set; }

        public FieldInfo FieldInfo { get; set; }

        public List<String> NotificationList { get; set; }
        public String NotificationDirection { get; set; }
    }

   

    public class FieldInfo
    {
        public String Field1Text { get; set; }
        public String Field1Value { get; set; }
        public String Field2Text { get; set; }
        public String Field2Value { get; set; }
        public String Field3Text { get; set; }
        public String Field3Value { get; set; }
    }

    public class PictureData
    {
        public List<PictureInfo> PictureList { get; set; }
        public String PictureUrlAddress { get; set; }
        public String PictureHeight { get; set; }
    }

    public class PictureInfo
    {
        public String Name { get; set; }
        public String PictureBase64List { get; set; }
        public String MimeType { get; set; }
    }

    public class CalendarData
    {
        public String Value { get; set; }
        public DateTime? StartDateValue { get; set; }
        public DateTime? EndDateValue { get; set; }
    }

    public class ChartData
    {
        public String Series { get; set; }
        public String Series1 { get; set; }
        public String Series2 { get; set; }
        public String Series3 { get; set; }
        public String Series4 { get; set; }
        public String Series5 { get; set; }
        public String Horizontal { get; set; }
        public String SeriesColor1 { get; set; }
        public String SeriesColor2 { get; set; }
        public String SeriesColor3 { get; set; }
        public String SeriesColor4 { get; set; }
        public String SeriesColor5 { get; set; }
        public String Series1Name { get; set; }
        public String Series2Name { get; set; }
        public String Series3Name { get; set; }
        public String Series4Name { get; set; }
        public String Series5Name { get; set; }
    }

    public class GridRowData
    {
        public List<GridData> Data { get; set; }
        public String RowNumber { get; set; }
        public String IsEmptyGrid { get; set; }
        public String Width { get; set; }
    }

    public class GridData
    {
        public String ColumnName { get; set; }
        public String Value { get; set; }
        public String DisplayName { get; set; }
        public String RecordId { get; set; }
        public String Width { get; set; }

    }
    public class FormLayout
    {
        public List<Tabs> Tabs { get; set; }
        public String FormName { get; set; }
        public String Id { get; set; }
        public String EntityId { get; set; }
    }
    public class Tabs
    {
        public List<FormDataDetail> FormData { get; set; }
        public String Visible { get; set; }
        public String EntityName { get; set; }
        public String Label { get; set; }
        public String Name { get; set; }
        public String Expanded { get; set; }
        public String ShowLabel { get; set; }
        public String IsBpf { get; set; }
        public List<BusinessProcessFlow> BusinessProcessFlowList { get; set; }
        public BusinessRuleContainer BusinessRules { get; set; }
        public String IsBr { get; set; }
        public List<Columns> Columns { get; set; }
    }
    public class Columns
    {
        public String Width { get; set; }
        public List<Sections> Sections { get; set; }
    }
    public class Sections
    {
        public String Visible { get; set; }
        public String ColumnLength { get; set; }
        public String ShowLabel { get; set; }
        public String Name { get; set; }
        public List<Rows> Rows { get; set; }
    }
    public class Rows
    {
        public List<Picklist> Picklist { get; set; }
        public List<String> CustomerLogicalName { get; set; }
        public String ShowLabel { get; set; }
        public String Visible { get; set; }
        public String ClassId { get; set; }
        public String ColSpan { get; set; }
        public String Label { get; set; }
        public String ViewId { get; set; }
        public String ElementType { get; set; }
        public String DatePart { get; set; }
        public String MaxValue { get; set; }
        public String MinValue { get; set; }
        public String Precision { get; set; }
        public String RequiredLevel { get; set; }
        public String LookupLogicalName { get; set; }
        public String DisplayName { get; set; }
        public String Type { get; set; }
        public String RowSpan { get; set; }
        public String LogicalName { get; set; }
        public String Disabled { get; set; }
        public String Value { get; set; }
        public String UserSpacer { get; set; }
        public String Ispace { get; set; }
        public String LookUpValueName { get; set; }
        public String DateFormat { get; set; }
        public String TimeFormat { get; set; }
        public String BeforeTimeFormat { get; set; }
        public String BeforeDateFormat { get; set; }
        public String RelationShipName { get; set; }
        public String SubGridId { get; set; }
        public List<Attachment> Attachments { get; set; }
        public SubGridsAndLookups SubGridsAndLookups { get; set; }       
    }

    public class Picklist
    {
        public String DefaultValue { get; set; }
        public String Label { get; set; }
        public String Value { get; set; }

    }
    public class FormDataDetail
    {
        public String Value { get; set; }
        public String LookUpValueName { get; set; }
        public String LogicalName { get; set; }
        public String Type { get; set; }
        public String LookupLogicalName { get; set; }
    }

    
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