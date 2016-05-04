using Newtonsoft.Json;
using Portal.Models;
using Portal.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.UI;

namespace Portal.Controllers
{

    [AllowAnonymous]
    public class LookupController : Controller
    {
        private String URL = System.Web.HttpContext.Current.Request.Url.Authority, AbsolutePath = System.Web.HttpContext.Current.Request.Url.AbsolutePath;
        String Referrer = System.Web.HttpContext.Current.Request.UrlReferrer != null ? System.Web.HttpContext.Current.Request.UrlReferrer.AbsolutePath : String.Empty;
        private String LoggedUserGuidValue;
        private String MainLangId;
        private String PortalId;
        private LocalizationModel LanguageModel = new LocalizationModel();
        private List<LanguageModel> Languages = new List<LanguageModel>();

        PortalService.CrmServiceInformation portalserviceinfo = new PortalService.CrmServiceInformation();
        private PortalService.PortalServiceClient client = null;

        public LookupController()
        {
            if (System.Web.HttpContext.Current.Request.Cookies[URL + "portalcookie"] == null)
            {
                RedirectToAction("Logout", "Dashboard");
            }
            else
            {
                client = new PortalService.PortalServiceClient(PortalHelper.GetBasicHttpBinding(), PortalHelper.GetEndPointAddress(URL));
                portalserviceinfo = JsonConvert.DeserializeObject<PortalService.CrmServiceInformation>(System.Web.HttpContext.Current.Request.Cookies[URL + "portalcookie"].Values[0]);

                if (portalserviceinfo == null)
                {
                    RedirectToAction("Logout", "Dashboard");
                }

                PortalId = portalserviceinfo.PortalId;

                if (System.Web.HttpContext.Current.Request.Cookies[URL + "usernameguidcookie"] != null)
                {
                    LoggedUserGuidValue = System.Web.HttpContext.Current.Request.Cookies[URL + "usernameguidcookie"].Values[0].ToString();
                }
                //Get All Languages 
                Languages = PortalHelper.GetLanguages(PortalId, client);
                //GET Portal Current LangId
                MainLangId = System.Web.HttpContext.Current.Request.Cookies[URL + "languagecookie"].Values[0];//PortalHelper.GetPortalCurrentLangId(System.Web.HttpContext.Current.Request);
                //CheckService for different langids
                PortalHelper.CheckPortalLanguage(ref portalserviceinfo, Languages, MainLangId, client);

                //Get Portal LocalizationXML due to CurretnLangId
                LanguageModel = Portal.Helper.PortalHelper.ParsePortalLangXml(System.Web.HttpContext.Current.Server.MapPath("~/Helper/Localization.xml"), MainLangId);
            }
        }

        [AllowAnonymous]
        public ActionResult Index(String LogicalName, String Count, String ReturnField, String IsMultiType)
        {
            LookupModel model = new LookupModel();
            model.Id = LogicalName;
            model.Count = Count;
            model.ReturnField = ReturnField;
            model.IsMultiType = IsMultiType;
            model.LocalizationModel = LanguageModel;
            return PartialView("Lookup", model);
        }

        [HttpPost]
        public ActionResult GetCrmData(String LogicalName, String Page, String WidgetId, String AttributeLogicalName, String NavigationId)
        {
            WidgetModel model = new WidgetModel();

            if( !String.IsNullOrEmpty(LoggedUserGuidValue))
                model = ((List<WidgetModel>)HttpContext.Application[LoggedUserGuidValue + "widgets"]).Where(p => p.pagewidgetid.Equals(WidgetId)).FirstOrDefault();
            else
                model = ((List<WidgetModel>)HttpContext.Application[Referrer + "widgets"]).Where(p => p.pagewidgetid.Equals(WidgetId)).FirstOrDefault();

            List<PortalService.LookupFilter> lookupfilters = new List<PortalService.LookupFilter>();
            if (model != null)
            {

                lookupfilters = model.lookupfilters.ConvertAll(p => new PortalService.LookupFilter
                 {
                     EntityName = p.entityname,
                     IsStatic = p.isstatic,
                     LogicalName = p.logicalname,
                     Value = p.value,
                     ValueLogicalName = p.valuelogicalname,
                     FilterType = p.filtertype,
                     Operator = p.Operator,
                     IsCustom = p.iscustom,
                     FetchXml = p.fetchXml,

                 }).ToList().Where(p => p.LogicalName.Equals(AttributeLogicalName.Replace("bpf_",String.Empty))).ToList();
            }
            PortalService.GridRowData[] data = client.GetLookupValues(portalserviceinfo,
                                                                      PortalId,
                                                                      LogicalName,
                                                                      Page,
                                                                      String.Empty,
                                                                      Request.Cookies[URL + "entitynamecookie"].Values[0],
                                                                      Request.Cookies[URL + "usernamevaluecookie"].Values[0],
                                                                      Request.Cookies[URL + "entityfieldcookie"].Values[0],
                                                                      lookupfilters.ToArray(),
                                                                      model.UseCache);

            var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            var result = new ContentResult
            {
                Content = serializer.Serialize(data),
                ContentType = "application/json"
            };
            return result;

        }

        [HttpPost]
        public ActionResult GetSearchCrmData(String LogicalName, String Page, String SearchValue, String WidgetId, String AttributeLogicalName)
        {

            WidgetModel model = new WidgetModel();

            if (!String.IsNullOrEmpty(LoggedUserGuidValue))
                model = ((List<WidgetModel>)HttpContext.Application[LoggedUserGuidValue + "widgets"]).Where(p => p.pagewidgetid.Equals(WidgetId)).FirstOrDefault();
            else
                model = ((List<WidgetModel>)HttpContext.Application[Referrer + "widgets"]).Where(p => p.pagewidgetid.Equals(WidgetId)).FirstOrDefault();

            List<PortalService.LookupFilter> lookupfilters = new List<PortalService.LookupFilter>();
            if (model != null)
            {
                lookupfilters = model.lookupfilters.ConvertAll(p => new PortalService.LookupFilter
               {
                   EntityName = p.entityname,
                   IsStatic = p.isstatic,
                   LogicalName = p.logicalname,
                   Value = p.value,
                   ValueLogicalName = p.valuelogicalname,
                   FilterType = p.filtertype,
                   Operator = p.Operator,
                   IsCustom = p.iscustom,
                   FetchXml = p.fetchXml,
               }).ToList().Where(p => p.LogicalName.Equals(AttributeLogicalName)).ToList();
            }
            PortalService.GridRowData[] data = client.GetLookupValues(portalserviceinfo,
                                                                      PortalId,
                                                                      LogicalName,
                                                                      Page,
                                                                      SearchValue,
                                                                      Request.Cookies[URL + "entitynamecookie"].Values[0],
                                                                      Request.Cookies[URL + "usernamevaluecookie"].Values[0],
                                                                      Request.Cookies[URL + "entityfieldcookie"].Values[0],
                                                                      lookupfilters.ToArray(),
                                                                      model.UseCache);


            var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            var result = new ContentResult
            {
                Content = serializer.Serialize(data),
                ContentType = "application/json"
            };
            return result;

        }

        [HttpPost]
        public ActionResult GetSubGridData(String ViewId)
        {
            PortalService.SubGridData[] data = client.GetSubGridData(portalserviceinfo, PortalId, ViewId);
            var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            var result = new ContentResult
            {
                Content = serializer.Serialize(data),
                ContentType = "application/json"
            };
            return result;
        }
    }
}