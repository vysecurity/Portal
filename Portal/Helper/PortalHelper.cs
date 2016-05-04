using Newtonsoft.Json;
using Portal.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml;
using System.Xml.Linq;

namespace Portal.Helper
{
    public class PortalHelper
    {
        public List<WidgetModel> GetXmlNode(String xml, String Node)
        {
            XElement form = XElement.Parse(xml);

            var elements = (from elem in form.Descendants("widget")
                            select new WidgetModel
                            {
                                id = elem.Attribute("id").Value,
                                pagewidgetid = elem.Attribute("pagewidgetid").Value,
                                name = elem.Attribute("description").Value,
                                UseCache = elem.Attribute("usecache") == null ? "false" : elem.Attribute("usecache").Value,
                                order = elem.Attribute("order").Value,
                                width = elem.Attribute("width") == null ? String.Empty : elem.Attribute("width").Value,
                                Zone = elem.Attribute("zone") == null ? String.Empty : elem.Attribute("zone").Value,
                                language = elem.Attribute("language") == null ? String.Empty : elem.Attribute("language").Value,
                                GridHeaderColor = elem.Attribute("headercolor") == null ? String.Empty : elem.Attribute("headercolor").Value,
                                GridHeaderFontColor = elem.Attribute("headerfontcolor") == null ? String.Empty : elem.Attribute("headerfontcolor").Value,
                                isownership = elem.Attribute("isownership") == null ? String.Empty : elem.Attribute("isownership").Value,
                                EntityName = elem.Attribute("entityname") == null ? String.Empty : elem.Attribute("entityname").Value,
                                iseditable = elem.Attribute("iseditable") == null ? String.Empty : elem.Attribute("iseditable").Value,
                                IsSignature = elem.Attribute("issignature") == null ? string.Empty : elem.Attribute("issignature").Value,
                                GridType = elem.Attribute("gridtype") == null ? String.Empty : elem.Attribute("gridtype").Value,
                                GridOnClick = elem.Attribute("onclick") == null ? String.Empty : elem.Attribute("onclick").Value,
                                CalculatedOnClick = elem.Attribute("onclickcalculated") == null ? String.Empty : elem.Attribute("onclickcalculated").Value,
                                CalculatedTheme = elem.Attribute("theme") == null ? String.Empty : elem.Attribute("theme").Value,
                                CalculatedIcon = elem.Attribute("icon") == null ? String.Empty : elem.Attribute("icon").Value,
                                GridRecordPerPage = elem.Attribute("recordperpage") == null ? String.Empty : elem.Attribute("recordperpage").Value,
                                GridOnClickOpenFormId = elem.Attribute("onclickopenformid") == null ? String.Empty : elem.Attribute("onclickopenformid").Value,
                                GridClickOpenFormStyle = elem.Attribute("openstyle") == null ? String.Empty : elem.Attribute("openstyle").Value,
                                CalculatedOnClickOpenFormId = elem.Attribute("onclickcalculatedopenformid") == null ? String.Empty : elem.Attribute("onclickcalculatedopenformid").Value,
                                GridOnClickOpenHTMLId = elem.Attribute("onclickopenhtmlid") == null ? String.Empty : elem.Attribute("onclickopenhtmlid").Value,
                                GridOnClickWidgetType = elem.Attribute("onclickopenwidgettype") == null ? String.Empty : elem.Attribute("onclickopenwidgettype").Value,
                                IsExcelExport = elem.Attribute("isexcelexport") == null ? String.Empty : elem.Attribute("isexcelexport").Value,
                                
                                RedirectAfterCreate = elem.Attribute("redirecttoeditpage") == null ? String.Empty : elem.Attribute("redirecttoeditpage").Value,
                                color = elem.Attribute("color") == null ? String.Empty : elem.Attribute("color").Value,
                                PercentageofTotalWidth = elem.Attribute("percentageoftotalwidth") == null ? String.Empty : elem.Attribute("percentageoftotalwidth").Value,
                                PictureHeight = elem.Attribute("p4crm_pictureheight") == null ? string.Empty : elem.Attribute("p4crm_pictureheight").Value,

                                GridLinkDescription = elem.Attribute("gridlinkdescription") == null ? String.Empty : elem.Attribute("gridlinkdescription").Value,
                                ExternalLinkDescription = elem.Attribute("externallinkdescription") == null ? String.Empty : elem.Attribute("externallinkdescription").Value,
                                FormLinkDescription = elem.Attribute("formlinkdescription") == null ? String.Empty : elem.Attribute("formlinkdescription").Value,
                                ExternalLinkUrl = elem.Attribute("externallinkurltext") == null ? String.Empty : elem.Attribute("externallinkurltext").Value,
                                IsExternalLink = elem.Attribute("isexternallink") == null ? String.Empty : elem.Attribute("isexternallink").Value,
                                IsGridLink = elem.Attribute("isgridlink") == null ? String.Empty : elem.Attribute("isgridlink").Value,
                                IsFormLink = elem.Attribute("isformlink") == null ? String.Empty : elem.Attribute("isformlink").Value,
                                EntityNameForLinkWidget = elem.Attribute("entitynameforlinkwidget") == null ? String.Empty : elem.Attribute("entitynameforlinkwidget").Value,
                                IsSeperate = elem.Attribute("isseperate") == null ? String.Empty : elem.Attribute("isseperate").Value,
                                OnClickOpenFormIdForLinkWidget = elem.Attribute("onclickopenformid") == null ? String.Empty : elem.Attribute("onclickopenformid").Value,
                                GridIcon = elem.Attribute("gridicon") == null ? String.Empty : elem.Attribute("gridicon").Value,
                                GridBGColor = elem.Attribute("gridbgcolor") == null ? String.Empty : elem.Attribute("gridbgcolor").Value,
                                FormIcon = elem.Attribute("formicon") == null ? String.Empty : elem.Attribute("formicon").Value,
                                FormBGColor = elem.Attribute("formbgcolor") == null ? String.Empty : elem.Attribute("formbgcolor").Value,
                                ExternalURLIcon = elem.Attribute("externalurlicon") == null ? String.Empty : elem.Attribute("externalurlicon").Value,
                                ExternalURLBGColor = elem.Attribute("externalurlbgcolor") == null ? String.Empty : elem.Attribute("externalurlbgcolor").Value,


                                Field1 = elem.Attribute("field1") == null ? String.Empty : elem.Attribute("field1").Value,
                                Field2 = elem.Attribute("field2") == null ? String.Empty : elem.Attribute("field2").Value,
                                Field3 = elem.Attribute("field3") == null ? String.Empty : elem.Attribute("field3").Value,

                                NotificationDirection = elem.Attribute("notificationdirection") == null ? String.Empty : elem.Attribute("notificationdirection").Value,

                                ChartColor = elem.Attribute("chartcolor") == null ? String.Empty : elem.Attribute("chartcolor").Value,
                                ChartFontSize = elem.Attribute("chartfontsize") == null ? String.Empty : elem.Attribute("chartfontsize").Value,
                                ChartFontFamily = elem.Attribute("chartfontfamily") == null ? String.Empty : elem.Attribute("chartfontfamily").Value,
                                Is3D = elem.Attribute("is3d") == null ? String.Empty : elem.Attribute("is3d").Value,
                                ChartLegendPosition = elem.Attribute("chartlegendposition") == null ? String.Empty : elem.Attribute("chartlegendposition").Value,
                                IsHandWrite = elem.Attribute("ishandwrite") == null ? String.Empty : elem.Attribute("ishandwrite").Value,
                                LabelText = elem.Attribute("labeltext") == null ? String.Empty : elem.Attribute("labeltext").Value,
                                filters = (from a in elem.Descendants("filters")
                                           select new Filters
                                           {
                                               type = a.Attribute("type").Value,
                                               filter = (from c in a.Descendants("filter")
                                                         select new Portal.Models.Filter
                                                         {
                                                             isstatic = c.Attribute("isstatic").Value,
                                                             condition = c.Attribute("condition") == null ? String.Empty : c.Attribute("condition").Value,
                                                             attributtename = c.Attribute("attributename") == null ? String.Empty : c.Attribute("attributename").Value,
                                                             value = c.Attribute("value") == null ? String.Empty : c.Attribute("value").Value,
                                                             Operator = c.Attribute("Operator") == null ? String.Empty : c.Attribute("Operator").Value,
                                                             uitype = c.Attribute("uitype") == null ? String.Empty : c.Attribute("uitype").Value,
                                                         }).ToList()
                                           }).ToList(),

                                lookupfilters = (from lf in elem.Descendants("lookupfilter")
                                                 select new LookupFilter
                                                 {
                                                     entityname = lf.Attribute("lookupentitylogicalname") == null ? String.Empty : lf.Attribute("lookupentitylogicalname").Value,
                                                     logicalname = lf.Attribute("lookupattributelogicalname") == null ? String.Empty : lf.Attribute("lookupattributelogicalname").Value,
                                                     value = lf.Attribute("lookupattributevalue") == null ? String.Empty : lf.Attribute("lookupattributevalue").Value,
                                                     isstatic = lf.Attribute("static") == null ? String.Empty : lf.Attribute("static").Value,
                                                     valuelogicalname = lf.Attribute("lookuprelationattributelogicalname") == null ? String.Empty : lf.Attribute("lookuprelationattributelogicalname").Value,
                                                     filtertype = lf.Attribute("type") == null ? String.Empty : lf.Attribute("type").Value,
                                                     Operator = lf.Attribute("operator") == null ? String.Empty : lf.Attribute("operator").Value,
                                                     iscustom = lf.Attribute("iscustom").Value,
                                                     fetchXml = lf.Attribute("FetchXml") == null ? String.Empty : HttpUtility.HtmlDecode(lf.Attribute("FetchXml").Value),
                                                 }).ToList(),
                                initialvalues = (from iv in elem.Descendants("initialvalues").Descendants("value")
                                                 select new InitialValues
                                                 {
                                                     entitylogicalname = iv.Attribute("entitylogicalname").Value,
                                                     attributelogicalname = iv.Attribute("attributelogicalname").Value,
                                                     lookuplogicalname = iv.Attribute("lookupentitylogicalname").Value,
                                                     initialvalue = iv.Attribute("initialvalue").Value,
                                                     lookupnamevalue = iv.Attribute("lookupnamevalue").Value,
                                                     Static = iv.Attribute("isstatic").Value,
                                                     iscomingfromsubgrids = iv.Attribute("issubgridnitialvalue") == null ? String.Empty : iv.Attribute("issubgridnitialvalue").Value,
                                                 }).ToList(),
                                //get subgrids views and lookup customviews
                                subgridsandlookups = (from iv in elem.Descendants("subgridandlookupcontrols").Descendants("value")
                                                      where (iv.Attribute("type").Value.Equals("subgrid") ||
                                                             (iv.Attribute("type").Value.Equals("lookup") && iv.Attribute("lookupbehaviour") != null && iv.Attribute("lookupbehaviour").Value.Equals("view")))
                                                      select new SubGridsAndLookups
                                                      {
                                                          formentitylogicalname = iv.Attribute("formentitylogicalname").Value,
                                                          type = iv.Attribute("type").Value,
                                                          SubGridViewId = iv.Attribute("subgridviewid") == null ? String.Empty : iv.Attribute("subgridviewid").Value,
                                                          SubGridId = iv.Attribute("subgridid") == null ? String.Empty : iv.Attribute("subgridid").Value,
                                                          SubGridLogicalName = iv.Attribute("subgridlogicalname") == null ? String.Empty : iv.Attribute("subgridlogicalname").Value,
                                                          NewFormId = iv.Attribute("subgridnewformid") == null ? String.Empty : iv.Attribute("subgridnewformid").Value,
                                                          UpdateFormId = iv.Attribute("subgridupdateformid") == null ? String.Empty : iv.Attribute("subgridupdateformid").Value,
                                                          LookupLogicalName = iv.Attribute("lookuplogicalname") == null ? String.Empty : iv.Attribute("lookuplogicalname").Value,
                                                          LookupEntityLogicalName = iv.Attribute("lookupentitylogicalname") == null ? String.Empty : iv.Attribute("lookupentitylogicalname").Value,
                                                          OpenFormId = iv.Attribute("lookupopenform") == null ? String.Empty : iv.Attribute("lookupopenform").Value,
                                                          LookupBehaviour = iv.Attribute("lookupbehaviour") == null ? String.Empty : iv.Attribute("lookupbehaviour").Value,
                                                      }).ToList(),
                                lookuplinks = (from iv in elem.Descendants("subgridandlookupcontrols").Descendants("value")
                                               where ((iv.Attribute("type").Value.Equals("lookup") && iv.Attribute("lookupbehaviour") != null && iv.Attribute("lookupbehaviour").Value.Equals("link")))
                                               select new LookupLinks
                                               {
                                                   formentitylogicalname = iv.Attribute("formentitylogicalname").Value,
                                                   type = iv.Attribute("type").Value,
                                                   LookupLogicalName = iv.Attribute("lookuplogicalname") == null ? String.Empty : iv.Attribute("lookuplogicalname").Value,
                                                   LookupEntityLogicalName = iv.Attribute("lookupentitylogicalname") == null ? String.Empty : iv.Attribute("lookupentitylogicalname").Value,
                                                   OpenFormId = iv.Attribute("lookupopenform") == null ? String.Empty : iv.Attribute("lookupopenform").Value,
                                                   LookupBehaviour = iv.Attribute("lookupbehaviour") == null ? String.Empty : iv.Attribute("lookupbehaviour").Value,
                                                   Editable = iv.Attribute("lookupeditable") == null ? String.Empty : iv.Attribute("lookupeditable").Value,
                                               }).ToList(),
                                Actions = (from a in elem.Descendants("actions").Descendants("action")
                                           select new Actions
                                           {
                                               Id = a.Attribute("id").Value,
                                               DisplayName = a.Attribute("displayname") == null ? String.Empty : a.Attribute("displayname").Value,
                                               EntityLogicalName = a.Attribute("entitylogicalname").Value,
                                               Order = a.Attribute("order").Value,
                                               Color = a.Attribute("color").Value,
                                               FormActions = a.Attribute("formactions") == null ? String.Empty : a.Attribute("formactions").Value,
                                               GridActions = a.Attribute("gridactions") == null ? String.Empty : a.Attribute("gridactions").Value,
                                               WorkFlowId = a.Attribute("workflowid") == null ? String.Empty : a.Attribute("workflowid").Value,
                                               OpenFormId = a.Attribute("openformid") == null ? String.Empty : a.Attribute("openformid").Value,
                                               FontColor = a.Attribute("fontcolor") == null ? String.Empty : a.Attribute("fontcolor").Value,
                                               WidgetType = a.Attribute("widgettype") == null ? String.Empty : a.Attribute("widgettype").Value,
                                               ErrorReturnMessage = a.Attribute("errorreturnmessage") == null ? String.Empty : a.Attribute("errorreturnmessage").Value,
                                               ReturnMessage = a.Attribute("returnmessage") == null ? String.Empty : a.Attribute("returnmessage").Value,
                                               IsEditable = a.Attribute("iseditable") == null ? String.Empty : a.Attribute("iseditable").Value.ToLower(),
                                               IsSignature = a.Attribute("issignature") == null ? String.Empty : a.Attribute("issignature").Value.ToLower(),
                                               UseOwnerShip = a.Attribute("isownership") == null ? String.Empty : a.Attribute("isownership").Value,
                                               OpenStyle = a.Attribute("openstyle") == null ? String.Empty : a.Attribute("openstyle").Value,
                                               OpeningWidgetType = a.Attribute("openingwidgettype") == null ? String.Empty : a.Attribute("openingwidgettype").Value,
                                           }).ToList(),


                            }).ToList();
            return elements;
        }

        public static LocalizationModel ParsePortalLangXml(String Xml, String LangCode)
        {
            XElement form = XElement.Load(Xml);
            var x = form.Descendants("lang").Attributes("code").ToList().Where(p => p.Value.Equals(LangCode)).ToList();
            if (x.Count == 0)
            {
                //Set it to english
                LangCode = "1033";
            }
            LocalizationModel L = (from elem in form.Descendants("lang")
                                   where elem.Attribute("code").Value.Equals(LangCode)
                                   select new LocalizationModel
                                       {
                                           LoginButton = elem.Element("LoginButton").Value,
                                           Password = elem.Element("LoginPassword").Value,
                                           UserName = elem.Element("LoginUserName").Value,
                                           LoginHeader = elem.Element("LoginHeader").Value,
                                           MyProfile = elem.Element("Profile").Value,
                                           LoggOff = elem.Element("LogOut").Value,
                                           Update = elem.Element("Update").Value,
                                           Browse = elem.Element("Browse").Value,
                                           ChangePictureHeader = elem.Element("ChangePictureHeader").Value,
                                           ExportExcel = elem.Element("ExportExcel").Value,
                                           Close = elem.Element("Close").Value,
                                           Save = elem.Element("Save").Value,
                                           Open = elem.Element("Open").Value,
                                           BrowserForNotes = elem.Element("BrowseButtonForNote").Value,
                                           AttachForNotes = elem.Element("AttachButtonForNote").Value,
                                           NewForNotes = elem.Element("NewButtonForNote").Value,
                                           DeleteForNotes = elem.Element("DeleteButtonForNote").Value,
                                           LookupHeader = elem.Element("LookupHeader").Value,
                                           ProfilePicture = elem.Element("ChangeProfilePicture").Value,
                                           ViewMore = elem.Element("MoreRecords").Value,
                                           SuccessLoginMessage = elem.Element("SuccessLoginMessage").Value,
                                           ErrorLoginMessage = elem.Element("ErrorLoginMessage").Value,
                                           EmptyLoginMessage = elem.Element("EmptyLoginMessage").Value,
                                           ForgotPassword = elem.Element("ForgotPassword").Value,
                                           ForgotPasswordMessage = elem.Element("ForgotPasswordMessage").Value,
                                           UserNameOrEmail = elem.Element("UserNameOrEmail").Value,
                                           Submit = elem.Element("Submit").Value,
                                           Back = elem.Element("Back").Value,
                                           NotFoundErrorMessageHeader = elem.Element("NotFoundErrorMessageHeader").Value,
                                           NotFoundErrorMessageDetail = elem.Element("NotFoundErrorMessageDetail").Value,
                                           HomePage = elem.Element("ReturnHome").Value,
                                           Actions = elem.Element("Actions").Value,
                                           Notifications = elem.Element("Notifications").Value
                                       })
                                  .FirstOrDefault();


            return L;
        }

        public static FormLayout ChangeWcfFormLayoutToMvcLayout(PortalService.FormLayout layout, String WidgetGuid)
        {
            FormLayout formlayouts = new FormLayout
            {
                EntityId = layout.EntityId,
                Id = WidgetGuid,
                FormName = layout.FormName,
                Tabs = layout.Tabs.ToList().ConvertAll(x => new Models.Tabs
                {
                    FormData = x.FormData.ToList().ConvertAll(f => new Models.FormDataDetail
                    {
                        LogicalName = f.LogicalName,
                        LookUpValueName = f.LookUpValueName,
                        Type = f.Type,
                        Value = f.Value,
                        LookupLogicalName = f.LookupLogicalName

                    }),
                    IsBpf = x.IsBpf == true ? "true" : "false",
                    BusinessProcessFlowList = BusinessProcessFlowConvert(x.BusinessProcessFlowList),
                    BusinessRules = BusinessRulesConvert(x.BusinessRules),
                    IsBr = x.IsBr == true ? "true" : "false",
                    ShowLabel = x.ShowLabel,
                    Visible = x.Visible,
                    EntityName = x.EntityName,
                    Expanded = x.Expanded,
                    Name = x.Name,
                    Label = x.Label,
                    Columns = x.Columns.ToList().ConvertAll(c => new Columns
                    {
                        Width = c.Width,
                        Sections = c.Sections.ToList().ConvertAll(s => new Sections
                        {
                            Visible = s.Visible,
                            ColumnLength = s.ColumnLength,
                            ShowLabel = s.ShowLabel,
                            Name = s.Name,
                            Rows = s.Rows.ToList().ConvertAll(r => new Rows
                            {
                                CustomerLogicalName = r.CustomerLogicalName == null ? new List<String>() : r.CustomerLogicalName.ToList(),
                                Visible = r.Visible,
                                Label = r.Label,
                                ShowLabel = r.ShowLabel,
                                ColSpan = r.ColSpan,
                                ViewId = r.ViewId,
                                ElementType = r.ElementType,
                                DatePart = r.DatePart,
                                MaxValue = r.MaxValue,
                                MinValue = r.MinValue,
                                Precision = r.Precision,
                                DisplayName = r.DisplayName,
                                RequiredLevel = r.RequiredLevel == null ? "none" : r.RequiredLevel.ToLower(),
                                Type = r.Type,
                                LookupLogicalName = r.LookupLogicalName,
                                Disabled = r.Disabled == "true" ? "disabled" : String.Empty,
                                LogicalName = r.LogicalName,
                                RowSpan = r.RowSpan,
                                ClassId = r.ClassId,
                                UserSpacer = r.UserSpacer,
                                Ispace = r.IsSpace,
                                DateFormat = r.DateFormat,
                                RelationShipName = r.RelationShipName,
                                SubGridId = r.SubGridId,
                                SubGridsAndLookups = r.SubGrids == null ?
                                null
                                :
                                r.SubGrids.ToList().ConvertAll(t => new SubGridsAndLookups
                                {
                                    SubGridViewId = t.SubGridViewId,
                                    SubGridId = t.SubGridId,
                                    NewFormId = t.NewFormId,
                                    UpdateFormId = t.UpdateFormId,
                                    SubGridLogicalName = t.SubGridLogicalName,
                                }).FirstOrDefault(),

                                Attachments = r.Attachments == null ? null : r.Attachments.ToList().ConvertAll(a => new Attachment
                                {
                                    AttachmentId = a.AttachmentId,
                                    DocumentBody = a.DocumentBody,
                                    EntityName = a.EntityName,
                                    FileName = a.FileName,
                                    MimeType = a.MimeType,
                                    RecordId = a.RecordId,
                                    Subject = a.Subject
                                }),
                                TimeFormat = r.TimeFormat,
                                BeforeDateFormat = r.BeforeDateFormat,
                                BeforeTimeFormat = r.BeforeTimeFormat,
                                Picklist = r.PicklistValues != null ? r.PicklistValues.ToList().ConvertAll(pi => new Picklist
                                {
                                    DefaultValue = pi.DefaultValue,
                                    Label = pi.Label,
                                    Value = pi.Value
                                }) : null
                            })
                        })
                    })
                })
            };
            return formlayouts;
        }

        private static List<BusinessProcessFlow> BusinessProcessFlowConvert(PortalService.BusinessProcessFlow[] businessProcessFlowSource)
        {
            List<BusinessProcessFlow> businessProcessFlowList = new List<BusinessProcessFlow>();

            if (businessProcessFlowSource != null && businessProcessFlowSource.Any())
            {
                var serialize = JsonConvert.SerializeObject(businessProcessFlowSource.ToList());
                var deserialze = JsonConvert.DeserializeObject<List<BusinessProcessFlow>>(serialize);

                return businessProcessFlowList = deserialze;
            }

            return null;
        }

        private static BusinessRuleContainer BusinessRulesConvert(PortalService.BusinessRuleContainer businessRuleSource)
        {
            BusinessRuleContainer businessRules = new BusinessRuleContainer();

            if (businessRuleSource != null && businessRuleSource.BusinessRules.Any())
            {
                var serialize = JsonConvert.SerializeObject(businessRuleSource);
                var deserialize = JsonConvert.DeserializeObject<BusinessRuleContainer>(serialize);

                return businessRules = deserialize;
            }

            return null;
        }

        public static PortalService.CrmServiceInformation GetCrmInformationFormConfig()
        {
            PortalService.CrmServiceInformation serviceinfo = new PortalService.CrmServiceInformation();
            serviceinfo.UserName = ConfigurationManager.AppSettings["user"].ToString();
            serviceinfo.Password = ConfigurationManager.AppSettings["password"].ToString();
            serviceinfo.Domain = ConfigurationManager.AppSettings["domain"].ToString();
            serviceinfo.OrganizationUri = ConfigurationManager.AppSettings["organizationuri"].ToString();
            serviceinfo.DiscoveryUri = ConfigurationManager.AppSettings["discoveryuri"].ToString();
            serviceinfo.Source = ConfigurationManager.AppSettings["source"].ToString();
            serviceinfo.CrmType = ConfigurationManager.AppSettings["crmtype"].ToString();
            //serviceinfo.PortalId = ConfigurationManager.AppSettings["PortalPage"].ToString();

            return serviceinfo;
        }

        public static MenuModel ChangeWcfNavigationIntoMenuModel(String Name, PortalService.Navigation[] navigations)
        {
            MenuModel model = new MenuModel();
            model.Menus = new List<Menu>();
            model.LoggedUser = Name;
            foreach (var item in navigations)
            {
                Menu m = new Menu();
                m.PageId = item.PageId;
                m.Name = item.Name;
                if (item.ParentNavigationId != String.Empty)
                {
                    m.IsSubMenu = true;
                    m.ParentMenuId = item.ParentNavigationId;

                }
                m.ExternalLink = item.ExternalLink;
                m.Order = item.Order;
                m.MenuId = item.NavigationId;
                m.URLName = item.UrlName;
                m.UniqueName = item.UniqueId;
                model.Menus.Add(m);
            }
            return model;
        }

        public static String StringEncodingConvert(String strText, String strSrcEncoding, String strDestEncoding)
        {
            System.Text.Encoding srcEnc = System.Text.Encoding.GetEncoding(strSrcEncoding);
            System.Text.Encoding destEnc = System.Text.Encoding.GetEncoding(strDestEncoding);
            byte[] bData = srcEnc.GetBytes(strText);
            byte[] bResult = System.Text.Encoding.Convert(srcEnc, destEnc, bData);
            return destEnc.GetString(bResult);
        }

        public static List<LanguageModel> GetLanguages(String PortalId, PortalService.PortalServiceClient client)
        {
            //GetPortalLangueges 
            List<PortalService.Language> Languages = client.GetPortalLanguages(PortalId).ToList();
            List<LanguageModel> PortalLanguageModel = new List<LanguageModel>();
            foreach (var item in Languages)
            {
                LanguageModel L = item.ConvertClass<LanguageModel>();
                PortalLanguageModel.Add(L);
            }
            return PortalLanguageModel;
        }

        public static PortalService.Language CreateServiceLanguage(String LangId, String BaseLangId)
        {
            PortalService.Language Language = new PortalService.Language();
            Language.BaseLangId = CultureInfo.GetCultures(CultureTypes.SpecificCultures).ToList().Where(p => p.Name.Equals(BaseLangId)).FirstOrDefault().LCID.ToString();
            Language.LangId = CultureInfo.GetCultures(CultureTypes.SpecificCultures).ToList().Where(p => p.Name.Equals(LangId)).FirstOrDefault().LCID.ToString();

            return Language;
        }

        public static String GetNameOfLangId(String LangId)
        {
            if (!String.IsNullOrEmpty(LangId))
            {
                try
                {
                    LangId = CultureInfo.GetCultures(CultureTypes.SpecificCultures).ToList().Where(p => p.LCID.Equals(Convert.ToInt32(LangId))).FirstOrDefault().Name.ToString();
                }
                catch
                {

                }
            }
            return LangId;
        }

        public static String GetLangIdOfCultureName(String Name)
        {
            return CultureInfo.GetCultures(CultureTypes.SpecificCultures).ToList().Where(p => p.Name.Equals(Name)).FirstOrDefault() == null
                ? String.Empty
                : CultureInfo.GetCultures(CultureTypes.SpecificCultures).ToList().Where(p => p.Name.Equals(Name)).FirstOrDefault().LCID.ToString();
        }

        public static String GetPortalCurrentLangId(HttpRequest Request, HttpContext ErrorContext = null)
        {
            String MainLangId = String.Empty;
            var RequestURL = Request.Path.Split('/');
            //if it is not coming from modal 
            if (Request.QueryString.AllKeys.Length == 0 || Request.QueryString.AllKeys.Contains("returnurl"))
            {
                //it is not lookup or modal form or html widget
                if (Request.RequestContext.RouteData.Values.Keys.Contains("LangId"))
                {
                    MainLangId = Request.RequestContext.RouteData.Values["LangId"].ToString();
                    MainLangId = PortalHelper.GetLangIdOfCultureName(MainLangId);

                }
                else
                {
                    if (Request.UrlReferrer != null)
                    {
                        var fullUrl = Request.UrlReferrer.ToString();
                        string url = fullUrl;

                        var request = new HttpRequest(null, url, null);
                        var response = new HttpResponse(new StringWriter());
                        var httpContext = new HttpContext(request, response);

                        var routeData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(httpContext));

                        MainLangId = routeData.Values["LangId"] != null ? routeData.Values["LangId"].ToString() : HttpUtility.ParseQueryString(httpContext.Request.Url.Query)["Language"];
                    }
                    else
                    {
                        if (ErrorContext != null)
                        {
                            var routeData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(ErrorContext));
                            MainLangId = routeData.Values["LangId"].ToString();
                        }
                    }
                }
            }
            else
                if (Request.QueryString["IsComingFromSubGrid"] == "true" || !String.IsNullOrEmpty(Request.QueryString["SubGridId"]))
                {
                    var fullUrl = Request.UrlReferrer.ToString();
                    string url = fullUrl;

                    var request = new HttpRequest(null, url, null);
                    var response = new HttpResponse(new StringWriter());
                    var httpContext = new HttpContext(request, response);

                    var routeData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(httpContext));
                    MainLangId = routeData.Values["LangId"] != null ? routeData.Values["LangId"].ToString() : HttpUtility.ParseQueryString(httpContext.Request.Url.Query)["Language"];
                }
                else
                {
                    //maybe somecustom  querystring parameters passed into for update widget
                    if (Request.RequestContext.RouteData.Values.Values.Contains("BuildPage"))
                    {
                        // if its not open in new window
                        if (Request.RequestContext.RouteData.Values.Keys.Contains("LangId"))
                        {
                            MainLangId = Request.RequestContext.RouteData.Values["LangId"].ToString();
                            MainLangId = PortalHelper.GetLangIdOfCultureName(MainLangId);
                        }
                        else if (!String.IsNullOrEmpty(Request.QueryString["Language"]))
                        {
                            MainLangId = Request.QueryString["Language"];
                        }
                        else
                        {
                            int internallangId = int.MaxValue;
                            Int32.TryParse(Request.QueryString["LangId"].ToString(), out internallangId);
                            MainLangId = internallangId == 0 ? PortalHelper.GetLangIdOfCultureName(Request.QueryString["LangId"].ToString()) : Request.QueryString["LangId"];
                        }
                    }
                    else if (Request.RequestContext.RouteData.Values.Values.Contains("WithLogin"))
                    {
                        MainLangId = PortalHelper.GetLangIdOfCultureName(Request.RequestContext.RouteData.Values["LangId"].ToString());
                    }
                    else if (Request.RequestContext.RouteData.Values.Values.Contains("Error"))
                    {
                        if (Request.UrlReferrer != null)
                        {
                            var fullUrl = Request.UrlReferrer.ToString();
                            string url = fullUrl;

                            var request = new HttpRequest(null, url, null);
                            var response = new HttpResponse(new StringWriter());
                            var httpContext = new HttpContext(request, response);

                            var routeData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(httpContext));

                            MainLangId = routeData.Values["LangId"] != null ? routeData.Values["LangId"].ToString() : HttpUtility.ParseQueryString(httpContext.Request.Url.Query)["Language"];
                            MainLangId = PortalHelper.GetLangIdOfCultureName(MainLangId);
                        }
                        else
                        {
                            if (Request.QueryString["LangId"] != null)
                                MainLangId = Request.QueryString["LangId"].ToString();
                        }
                    }
                    else
                    {
                        int internallangId = int.MaxValue;
                        Int32.TryParse(Request.QueryString["Language"], out internallangId);
                        MainLangId = internallangId == 0 ? PortalHelper.GetLangIdOfCultureName(Request.QueryString["Language"].ToString()) : Request.QueryString["Language"];
                    }
                }

            return MainLangId;
        }

        public static void CheckPortalLanguage(ref PortalService.CrmServiceInformation portalserviceinfo, List<LanguageModel> Languages, String LangId, PortalService.PortalServiceClient client)
        {
            if (Languages.Count > 0)
            {
                int intLangId = int.MinValue;

                Int32.TryParse(LangId, out intLangId);
                var LanguageCheck = new LanguageModel();
                if (intLangId != 0)
                {
                    LanguageCheck = Languages.Where(p => p.NativeName.Equals(PortalHelper.GetNameOfLangId(LangId))).FirstOrDefault();
                    //Means Language Changed! We need to get the related configuration
                    if (portalserviceinfo.PortalMainLanguageCode != LangId)
                    {
                        PortalService.CrmServiceInformation info = client.GetConfigurationInfo(LanguageCheck.ConfigurationId);

                        portalserviceinfo.UserName = info.UserName;
                        portalserviceinfo.Password = info.Password;
                        portalserviceinfo.ConfigurationId = LanguageCheck.ConfigurationId;
                    }
                }
                else
                {
                    LanguageCheck = Languages.Where(p => p.NativeName.Equals(LangId)).FirstOrDefault();

                    if (portalserviceinfo.PortalMainLanguageCode != PortalHelper.GetLangIdOfCultureName(LangId))
                    {
                        PortalService.CrmServiceInformation info = client.GetConfigurationInfo(LanguageCheck.ConfigurationId);

                        portalserviceinfo.UserName = info.UserName;
                        portalserviceinfo.Password = info.Password;
                        portalserviceinfo.ConfigurationId = LanguageCheck.ConfigurationId;
                    }
                }
            }

        }

        public static LocalizationModel CheckPortalLanguageCookie(ref LocalizationModel LocalizationModel, String Xml, String LangId, String CookieValue)
        {
            int internallangId = int.MaxValue;
            Int32.TryParse(LangId, out internallangId);
            if (internallangId == 0)
            {
                LangId = PortalHelper.GetLangIdOfCultureName(LangId);
            }
            if (LangId != CookieValue)
            {
                LocalizationModel = PortalHelper.ParsePortalLangXml(Xml, LangId);
            }
            return LocalizationModel;
        }

        public static void SetCookies(String URL, String CookieName, String CookieUniqueName, String Value, int ExpirationTime, String CookieValue = null)
        {
            if (CookieName == "languagecookie")
            {
                int internallangId = int.MaxValue;
                Int32.TryParse(Value, out internallangId);

                if (internallangId == 0)
                {
                    Value = PortalHelper.GetLangIdOfCultureName(Value);
                }
            }
            HttpCookie cookieurl = new HttpCookie(URL + CookieName);
            cookieurl[URL + CookieUniqueName] = Value;
            cookieurl.Expires = DateTime.Now.AddMinutes(ExpirationTime);

            if (System.Web.HttpContext.Current.Request.Cookies[URL + CookieName] == null)
            {

                System.Web.HttpContext.Current.Response.Cookies.Add(cookieurl);
            }
            else
            {
                if ((CookieValue != Value))
                {
                    System.Web.HttpContext.Current.Response.SetCookie(cookieurl);
                }
            }
        }

        public static BasicHttpBinding GetBasicHttpBinding()
        {
            BasicHttpBinding binding = new BasicHttpBinding();
            binding.Name = "BasicHttpBinding_IHascelik";
            binding.Security.Mode = BasicHttpSecurityMode.Transport;
            //binding.Security.Mode = BasicHttpSecurityMode.None;
            binding.OpenTimeout = TimeSpan.FromMinutes(10);
            binding.CloseTimeout = TimeSpan.FromMinutes(10);
            binding.ReceiveTimeout = TimeSpan.FromMinutes(10);
            binding.SendTimeout = TimeSpan.FromMinutes(10);
            binding.MaxReceivedMessageSize = 2147483647;
            return binding;
        }

        public static EndpointAddress GetEndPointAddress(String URL)
        {
            var myEndpointAddress = new EndpointAddress(new Uri(new Azure.SqlAzureConnection().GetWcfURL(URL, ConfigurationManager.AppSettings["Environment"].ToString())));
            return myEndpointAddress;
        }

        public static String GetBootStrapClass(String Width)
        {
            String ClassName = String.Empty;
            if (Width == "16")
            {
                ClassName = "col-md-2 col-sm-12 col-xs-12";
            }
            else if (Width == "25")
            {
                ClassName = "col-md-3 col-sm-12 col-xs-12";
            }
            else if (Width == "33")
            {
                ClassName = "col-md-4 col-sm-12 col-xs-12";
            }
            else if (Width == "50")
            {
                ClassName = "col-md-6 col-sm-12 col-xs-12";
            }
            else if (Width == "66")
            {
                ClassName = "col-md-8 col-sm-12 col-xs-12";
            }
            else if (Width == "75")
            {
                ClassName = "col-md-9 col-sm-12 col-xs-12";
            }
            else if (Width == "84")
            {
                ClassName = "col-md-10 col-sm-12 col-xs-12";
            }
            else if (Width == "100")
            {
                ClassName = "col-md-12 col-sm-12 col-xs-12";
            }
            return ClassName;
        }

        public static int GetBootStrapClassSize(int Width)
        {
            int size = 0;

            if (Width == 16)
            {
                size = 2;
            }
            else if (Width == 25)
            {
                size = 3;
            }
            else if (Width == 33)
            {
                size = 4;
            }
            else if (Width == 50)
            {
                size = 6;
            }
            else if (Width == 66)
            {
                size = 8;
            }
            else if (Width == 75)
            {
                size = 9;
            }
            else if (Width ==84)
            {
                size = 10;
            }
            else if (Width == 100)
            {
                size = 12;
            }
            else
            {
                size = 0;
            }
            return size;
        }

    }
    public static class ClassIds
    {
        public static String AccessPrivilegeControl = "{F93A31B2-99AC-4084-8EC2-D4027C31369A}";
        public static String UrlControl = " {71716B6C-711E-476C-8AB8-5D11542BFB47}";
        public static String RadioControl = "{67FAC785-CD58-4F9F-ABB3-4B7DDC6ED5ED}";
        public static String CheckBoxControl = "{B0C6723A-8503-4FD7-BB28-C8A06AC933C2}";

    }

    public static class CrmHelperStatics
    {

        public static T ToNonAnonymousList<T>(this List<T> list, Type t)
        {

            //define system Type representing List of objects of T type:
            var genericType = typeof(List<>).MakeGenericType(t);

            //create an object instance of defined type:
            var l = Activator.CreateInstance(genericType);

            //get method Add from from the list:
            MethodInfo addMethod = l.GetType().GetMethod("Add");

            //loop through the calling list:
            foreach (T item in list)
            {

                //convert each object of the list into T object 
                //by calling extension ToType<T>()
                //Add this object to newly created list:
                addMethod.Invoke(l, new object[] { item.ToType(t) });
            }

            //return List of T objects:
            return (T)l;
        }

        public static object ToType<T>(this object obj, T type)
        {
            //create instance of T type object:
            object tmp = Activator.CreateInstance(Type.GetType(type.ToString()));

            //loop through the properties of the object you want to covert:          
            foreach (PropertyInfo pi in obj.GetType().GetProperties())
            {
                try
                {
                    //get the value of property and try to assign it to the property of T type object:
                    tmp.GetType().GetProperty(pi.Name).SetValue(tmp, pi.GetValue(obj, null), null);
                }
                catch (Exception ex)
                {

                }
            }
            //return the T type object:         
            return tmp;
        }

        public static T ToEntity<T>(this Object myobj)
        {

            Type target = typeof(T);

            var targetentity = Activator.CreateInstance(target, false);
            var d = from t in target.GetMembers().ToList() where t.MemberType == MemberTypes.Property select t;
            List<MemberInfo> targetmembers = d.Where(memberInfo => d.Select(c => c.Name).ToList().Contains(memberInfo.Name)).ToList();
            PropertyInfo propertyInfo;
            object value;
            foreach (var memberInfo in targetmembers)
            {
                if (myobj.GetType().GetProperty(memberInfo.Name) != null && memberInfo.Name.ToLower() != "item")
                {
                    propertyInfo = typeof(T).GetProperty(memberInfo.Name);

                    value = myobj.GetType().GetProperty(memberInfo.Name).GetValue(myobj, null);

                    if (value != null && propertyInfo.CanWrite)
                        propertyInfo.SetValue(targetentity, value, null);
                }
            }


            return (T)targetentity;
        }

        public static T ConvertClass<T>(this Object myobj)
        {
            Type objectType = myobj.GetType();
            Type target = typeof(T);
            var x = Activator.CreateInstance(target, false);
            var z = from source in objectType.GetMembers().ToList()
                    where source.MemberType == MemberTypes.Property
                    select source;
            var d = from source in target.GetMembers().ToList()
                    where source.MemberType == MemberTypes.Property
                    select source;

            List<MemberInfo> members = d.Where(memberInfo => d.Select(c => c.Name).ToList().Contains(memberInfo.Name)).ToList();
            PropertyInfo propertyInfo;
            object value;
            foreach (var memberInfo in members)
            {
                propertyInfo = typeof(T).GetProperty(memberInfo.Name);
                if (myobj.GetType().GetProperty(memberInfo.Name) != null)
                {
                    value = myobj.GetType().GetProperty(memberInfo.Name).GetValue(myobj, null);
                    if (value != null)
                    {
                        if (value.GetType().IsGenericType == false)
                        {
                            if (value.GetType().Name == "String[]")
                            {
                                propertyInfo.SetValue(x, ((String[])value).ToList(), null);
                            }
                            else
                                if (value.GetType().IsArray == false)
                                    propertyInfo.SetValue(x, value, null);
                        }
                    }
                }
            }
            return (T)x;
        }

        public static void CopyValues<T>(this T source, T target)
        {
            Type t = typeof(T);

            var properties = t.GetProperties().Where(prop => prop.CanRead && prop.CanWrite);

            foreach (var prop in properties)
            {
                var value = prop.GetValue(source, null);
                if (value != null)
                    prop.SetValue(target, value, null);
                else
                {
                    //create initializer generic array
                    if (prop.PropertyType.IsArray)
                    {
                        if (target != null && prop.GetValue(target) == null)
                            prop.SetValue(target, Array.CreateInstance(prop.PropertyType.GetElementType(), 0), null);
                    }
                }
            }
        }

        public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }

        public static PortalService.Language GetMainLanguage(this List<LanguageModel> LanguageList, String LangId)
        {
            var LCID = CultureInfo.GetCultures(CultureTypes.SpecificCultures).ToList().Where(p => p.Name.Equals(LangId)).FirstOrDefault().LCID.ToString();
            return LanguageList.Where(p => p.LangId.Equals(LCID)).FirstOrDefault().ConvertClass<PortalService.Language>();

        }

    }

    public static class CookieHelperStatic
    {

        public static String DecodedValues(this HttpCookie cookie, int index)
        {
            if (cookie == null)
            {
                return String.Empty;
            }
            return HttpUtility.UrlDecode(cookie.Values[index]);
        }
    }



}