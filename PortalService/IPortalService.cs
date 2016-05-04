using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using ClassLibrary;

namespace PortalService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IPortalService
    {
        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        String GetDataTrailer();

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        String GetExternalWidgetHTML(String URL);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        CrmServiceInformation GetPortalIdAndCreateSourceCrmService(String DomainURL);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        CrmServiceInformation GetConfigurationInfo(String ConfigurationId);

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        String GetDynamicScriptForEntity(String Id);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        LoginInformation GetLoginInfo(CrmServiceInformation serviceinfo);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        List<Object> AttemptLogin(CrmServiceInformation serviceinfo, String PortalId, LoginInformation LoginInformation, Login LoginValues);

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        Byte[] GetPicture(CrmServiceInformation serviceinfo, String PortalId, String EntityLogicalName, String PrimaryField, String LoginUser);

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        Page GetNavigationPage(CrmServiceInformation serviceinfo, String PortalId, String NavigationId, String PortalEntity);

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        Page GetPage(String PageId);

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        List<WidgetData> GetWidgetData(CrmServiceInformation serviceinfo, String PortalId, List<WidgetParameters> WidgetParameters, String PortalEntity, String PortalEntityUserValue, String PortalEntityUserField, String UseCache);

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        List<GridRowData> GetLookupValues(CrmServiceInformation serviceinfo, String PortalId, String LogicalName, String Page, String SearchValue, String PortalEntity, String PortalEntityUserValue, String PortalEntityUserField, List<LookupFilter> lookupfilters, String UseCache);

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        List<SubGridData> GetSubGridData(CrmServiceInformation serviceinfo, String PortalId, String ViewId);

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        CreatedData CreateData(CrmServiceInformation serviceinfo, String PortalId, String EntityName, List<FormData> FormData, String Signature, String OwnerShip, String PortalEntity, String PortalEntityUserValue, String PortalEntityUserField, String RelationShipName, String ParentId);

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        FormLayout GetEditFormLayout(CrmServiceInformation serviceinfo, String PortalId, String FormId, String EntityId, List<SubGridModel> SubGridModel);

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        String UpdateData(CrmServiceInformation serviceinfo, String PortalId, String EntityName, List<FormData> FormData, String Id, String OwnerShip, String PortalEntity, String PortalEntityUserValue, String PortalEntityUserField);

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        List<GridRowData> GetGridDataPerPage(CrmServiceInformation serviceinfo, String PortalId, List<WidgetParameters> WidgetParameters, String WidgetId, String PortalEntity, String PortalEntityUserValue, String PortalEntityUserField, String Page, String RecordCount, String SearchValue = "");

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        PersonelInformationForm GetPersonelInformation(CrmServiceInformation serviceinfo, String PortalId, String PortalEntity, String PortalEntityUserValue, String PortalEntityUserField);

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        List<Roles> GetLoginUserRoles(CrmServiceInformation serviceinfo, String PortalId, String PortalEntity, String PortalEntityUserValue, String PortalEntityUserField);

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        List<Navigation> GetExternalNavigation(String PortalId);

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        List<Navigation> GetNavigationOfUser(CrmServiceInformation serviceinfo, String PortalId, List<Roles> roles);

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        String GetGridWidgetId(CrmServiceInformation serviceinfo, String PortalId, String WidgetGuid);

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        List<GridRowData> GetCalculatedFieldRecords(CrmServiceInformation serviceinfo, String PortalId, String CalculatedWidgetId, String GridWidgetId, List<Filters> filters, String PortalEntity, String PortalEntityUserValue, String PortalEntityUserField, String Page, String SearchValue, String RecordCount);

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        List<InitialValues> GetAndChangeDynamicInitialValues(CrmServiceInformation serviceinfo, String PortalId, List<InitialValues> InitialValues, String PortalEntity, String PortalEntityUserValue, String PortalEntityUserField);

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        String GetHtmlWidgetContent(CrmServiceInformation serviceinfo, String PortalId, String WidgetGuid, String EntityId, String PortalEntity, String PortalEntityUserValue, String PortalEntityUserField);

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        String UpdateEntityImage(CrmServiceInformation serviceinfo, String PortalId, String Image, String PortalEntity, String PortalEntityUserValue, String PortalEntityUserField);

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        AttachmentReturn AddNotesToRelatedRecord(CrmServiceInformation serviceinfo, String PortalId, Attachment Attachment);

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        String DeleteNote(CrmServiceInformation serviceinfo, String PortalId, String AttachmentId);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        List<CrmEntities> GetCrmEntityList(String ConfigurationId, CrmServiceInformation information);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        List<CrmAttributes> GetEntityAttributes(String ConfigurationId, String EntityName, CrmServiceInformation information, String calculatedfieldtype, String DateTime, String Lookup);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        List<CrmViews> GetEntityViews(String ConfigurationId, String EntityName, CrmServiceInformation information);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        List<CrmForms> GetEntityForms(String ConfigurationId, String EntityName, CrmServiceInformation information);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        List<CrmWorkFlows> GetWorkFlows(String ConfigurationId, String EntityName, CrmServiceInformation information);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        List<SubGrid> GetFormSubGrids(String ConfigurationId, String EntityName, String FormId, String PortalId, CrmServiceInformation information);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        List<FormLookups> GetFormLookups(String ConfigurationId, String EntityName, String FormId, String PortalId, CrmServiceInformation information);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        List<SubGridRecords> GetSubGridRecords(CrmServiceInformation serviceinfo, String PortalId, String EntityName, String EntityId, List<String> SubGridColumns);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        List<GridRowData> GetRelatedSubGridRecords(CrmServiceInformation serviceinfo, String PortalId, String SubGridViewId, String RelationShipName, String ParentId);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        List<Language> GetLanguages(String ConfigurationId, CrmServiceInformation information);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        List<Language> GetLanguagesForPageWidget(CrmServiceInformation information);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        List<Formats> GetFormats(String LangId, String FormatType);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        List<ClassLibrary.TimeZone> GetTimeZones();

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        List<Language> GetPortalLanguages(String PortalId);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        String GetPortalBaseLanguage(String ConfigurationId, CrmServiceInformation information);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        String GetPortalMainLanguage(String PortalId, CrmServiceInformation information);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        String ResetPassword(String PortalId, String Value, String Template, CrmServiceInformation information);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        String ExecuteCustomActions(CrmServiceInformation serviceinfo, String PortalId, String EntityId, String WorkflowId);

    }

}
