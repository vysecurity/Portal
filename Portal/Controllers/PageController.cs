using Newtonsoft.Json;
using Portal.Helper;
using Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Portal.Controllers
{

    public class PageController : Controller
    {
        private String URL = System.Web.HttpContext.Current.Request.Url.Authority, AbsolutePath = System.Web.HttpContext.Current.Request.Url.AbsolutePath;
        String Referrer = System.Web.HttpContext.Current.Request.UrlReferrer != null ? System.Web.HttpContext.Current.Request.UrlReferrer.AbsolutePath : String.Empty;
        private String LoggedUserGuidValue, LocXml = String.Empty;
        private String MainLangId;
        private String PortalId;
        private LocalizationModel LanguageModel = new LocalizationModel();
        private List<LanguageModel> Languages = new List<LanguageModel>();
        ObjectCache cache = MemoryCache.Default;
        private PortalService.CrmServiceInformation portalserviceinfo = new PortalService.CrmServiceInformation();
        private PortalService.PortalServiceClient client = null;

        public PageController()
        {
            //check these cookies! If they are null user must redirect to login page
            if (System.Web.HttpContext.Current.Request.Cookies[URL + "portalcookie"] == null)
            {
                RedirectToAction("Logout", "Dashboard");
            }

            else if (System.Web.HttpContext.Current.Request.Cookies[URL + "languagecookie"] == null)
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

                else
                {
                    if (System.Web.HttpContext.Current.Request.Cookies[URL + "usernameguidcookie"] != null && !System.Web.HttpContext.Current.Request.RequestContext.RouteData.Values.Values.Contains("ExternalBuildPage"))
                    {
                        LoggedUserGuidValue = System.Web.HttpContext.Current.Request.Cookies[URL + "usernameguidcookie"].Values[0].ToString();
                    }

                    PortalId = portalserviceinfo.PortalId;
                    System.Web.HttpContext.Current.Application[URL + "PortalIds"] = PortalId;
                    //Get All Languages 
                    Languages = PortalHelper.GetLanguages(PortalId, client);
                    //GET Portal Current LangId                    
                    MainLangId = PortalHelper.GetPortalCurrentLangId(System.Web.HttpContext.Current.Request);

                    //just in case set in everytime because user may change the language
                    PortalHelper.SetCookies(URL, "languagecookie", "language", MainLangId, 1440, System.Web.HttpContext.Current.Request.Cookies[URL + "languagecookie"].Values[0]);

                    LocXml = System.Web.HttpContext.Current.Server.MapPath("~/Helper/Localization.xml");

                    if (!String.IsNullOrEmpty(MainLangId))
                    {
                        //CheckService for different langids
                        PortalHelper.CheckPortalLanguage(ref portalserviceinfo, Languages, MainLangId, client);
                        //Get Portal LocalizationXML due to CurretnLangId
                        LanguageModel = PortalHelper.ParsePortalLangXml(LocXml, MainLangId);
                    }
                }
            }
        }

        [Authorize]
        public ActionResult BuildPage(String NavigationId, String LangId, String IsComingFromExternal, String QueryStrings)
        {
            //Response.Write(DateTime.Now.ToString());
            var LanguageCheck = Languages.Where(p => p.NativeName.Equals(LangId)).FirstOrDefault();
            if (LanguageCheck == null)
            {
                return RedirectToAction("NotFound", "Error", new { LangId = LangId });
            }

            //check cookie control
            if (Request.Cookies[URL + "entitynamecookie"] == null)
            {
                return RedirectToAction("Logout", "Dashboard");

            }
            else if (String.IsNullOrEmpty(PortalId))
            {
                return RedirectToAction("Logout", "Dashboard");
            }
            else if (Request.Cookies[URL + "usernameguidcookie"] == null)
            {
                return RedirectToAction("Logout", "Dashboard");
            }
            //PortalService.PortalServiceClient client = new PortalService.PortalServiceClient();

            PortalService.Page page = new PortalService.Page();
            String CheckNavigationId = String.Empty;
            List<Menu> m = new List<Menu>();

            try
            {
                //not external page
                if (String.IsNullOrEmpty(IsComingFromExternal))
                {
                   
                    if (!cache.Contains(PortalId + LoggedUserGuidValue + "menucache"))
                    {
                        return RedirectToAction("Logout", "Dashboard");
                    }
                    String DecodedValues = Server.UrlDecode(cache[PortalId + LoggedUserGuidValue + "menucache"].ToString());
                   
                    m = JsonConvert.DeserializeObject<List<Menu>>(DecodedValues );
                    String[] UniqueNavigationArr = NavigationId.Split('-');
                    String UniqueNavigationId = UniqueNavigationArr.ElementAt(UniqueNavigationArr.Length - 1);

                   
                    var check = m.Where(p => p.UniqueName != null && p.UniqueName.Equals(UniqueNavigationId)).ToList();
                
                    if (check.Count == 0)
                    {
                        return RedirectToAction("NotFound", "Error", new { LangId = LangId });
                    }
                    
                    CheckNavigationId = check[0].MenuId;
                   
                    page = client.GetNavigationPage(portalserviceinfo, PortalId, CheckNavigationId, Request.Cookies[URL + "entitynamecookie"].Values[0].ToString());
                   
                }
                //external Page!
                else
                {
                    CheckNavigationId = NavigationId;

                    page = client.GetPage(CheckNavigationId);
                }
            }
            catch (Exception ex)
            {
                //return RedirectToAction("ExternalErrorPage", "Error", new { Message = ex.Message });
                throw new Exception(ex.Message);
            }

            Helper.PortalHelper helper = new Helper.PortalHelper();
            List<WidgetModel> widgets = helper.GetXmlNode(page.FormLayoutXml, "widget");
            widgets = widgets.OrderBy(p => Convert.ToInt32(p.order)).ToList();

            #region Prepare Parameters

            List<QueryStrings> ListQueryStrings = new List<Models.QueryStrings>();
            if (!String.IsNullOrEmpty(QueryStrings))
            {
                ListQueryStrings = JsonConvert.DeserializeObject<List<QueryStrings>>(QueryStrings);
            }
            //for external
            List<PortalService.WidgetParameters> parameters = widgets.ConvertAll(p => new PortalService.WidgetParameters
            {
                Order = p.order,
                WidgetId = p.id,
                PercentageofTotalWidth = p.PercentageofTotalWidth,
                PageWidgetId = p.pagewidgetid,
                GridPerPage = p.GridRecordPerPage,
                ActionsCount = Convert.ToString(p.Actions.Count),
                PictureHeight = p.PictureHeight,
                filters = p.filters.ConvertAll(a =>
                    new PortalService.Filters
                    {
                        type = a.type,
                        filter = a.filter.ConvertAll(x =>
                        new PortalService.Filter { Operator = x.Operator, uitype = x.uitype, attributtename = x.attributtename, condition = x.condition, isstatic = x.isstatic, value = x.value }).ToArray()
                    }).ToArray(),
                subgrids = p.subgridsandlookups.Where(z => z.type.Equals("subgrid")).ToList().ConvertAll(a =>
                new PortalService.SubGridModel
                {
                    SubGridViewId = a.SubGridViewId,
                    SubGridId = a.SubGridId,
                    SubGridLogicalName = a.SubGridLogicalName,
                    NewFormId = a.NewFormId,
                    UpdateFormId = a.UpdateFormId

                }).ToArray(),
                QueryStrings = ListQueryStrings.ConvertAll(z =>
                                 new PortalService.QueryStrings
                                 {
                                     Key = z.Key,
                                     Value = z.Value
                                 }).ToArray()

            }).ToList();

            #endregion            

            PortalService.WidgetData[] data = new PortalService.WidgetData[] { };
            data = client.GetWidgetData(portalserviceinfo,
                                        PortalId,
                                        parameters.ToArray(),
                                        Request.Cookies[URL + "entitynamecookie"].Values[0],
                                        Request.Cookies[URL + "usernamevaluecookie"].Values[0],
                                        Request.Cookies[URL + "entityfieldcookie"].Values[0],
                                        page.UseCache);

            //first change the dynamic initial values
            #region Initial Values

            foreach (var item in widgets)
            {
                if (item.initialvalues.Count > 0)
                {
                    List<PortalService.InitialValues> InitialValues = item.initialvalues.Where(p => p.iscomingfromsubgrids.Equals("0")).ToList().ConvertAll(p => new PortalService.InitialValues
                    {
                        AttributeLogicalName = p.attributelogicalname,
                        EntityLogicalName = p.entitylogicalname,
                        InitialValue = p.initialvalue,
                        LookupLogicalName = p.lookuplogicalname,
                        LookupNameValue = p.lookupnamevalue,
                        Static = p.Static
                    }).ToList();

                    List<PortalService.InitialValues> ChangedInitialValues = client.GetAndChangeDynamicInitialValues(portalserviceinfo,
                                                                                                                     PortalId,
                                                                                                                     InitialValues.ToArray(),
                                                                                                                     Request.Cookies[URL + "entitynamecookie"].Values[0],
                                                                                                                     Request.Cookies[URL + "usernamevaluecookie"].Values[0],
                                                                                                                     Request.Cookies[URL + "entityfieldcookie"].Values[0]).ToList();

                    //change WcfService InitialValue to Mvc InitialValue
                    foreach (var InitialItem in ChangedInitialValues)
                    {
                        InitialValues v = item.initialvalues.Where(p => p.attributelogicalname.Equals(InitialItem.AttributeLogicalName)).FirstOrDefault();

                        v.lookupnamevalue = InitialItem.LookupNameValue;
                        v.initialvalue = InitialItem.InitialValue;
                    }
                }
            }

            #endregion

            //for lookup filters and initial values         
            HttpContext.Application[LoggedUserGuidValue + "widgets"] = widgets;
            HttpContext.Application[LoggedUserGuidValue + "parameters"] = parameters;

            foreach (var item in widgets)
            {
                //First Select Related Service Data Widget
                PortalService.WidgetData serviceclass = data.ToList().Where(p => p.PageWidgetId.Equals(item.pagewidgetid)).ToList().FirstOrDefault();
                //Pass XML Parameters into temp data --> These parameters is in the LayoutXML but not in Page Widget Entity. So we need to compensate them
                serviceclass = item.ConvertClass<PortalService.WidgetData>();
                //Pass Temp Class members to Service Data Widget               
                serviceclass.CopyValues<PortalService.WidgetData>(data.ToList().Where(p => p.PageWidgetId.Equals(item.pagewidgetid)).ToList().FirstOrDefault());
                //convert Actions!  
                List<PortalService.Actions> ServiceActions = new List<PortalService.Actions>();
                foreach (var actions in item.Actions)
                {
                    ServiceActions.Add(actions.ConvertClass<PortalService.Actions>());
                }
                data.ToList().Where(d => d.PageWidgetId.Equals(item.pagewidgetid)).ToList().FirstOrDefault().Actions = ServiceActions.ToArray();

            }

            #region Change Wcf Service Classes into Model

            List<WidgetDataModel> modeldataview = data.ToList().ConvertAll(p => new WidgetDataModel
            {
                Values = p.Values != null ? p.Values.ToList().ConvertAll(z => new CalendarData { Value = z.Value, StartDateValue = z.StartDateValue, EndDateValue = z.EndDateValue }) : null,
                Order = p.Order,
                Zone = p.Zone,
                PercentageofTotalWidth = p.PercentageofTotalWidth,
                EntityName = p.EntityName,
                IsEditable = p.iseditable,
                IsSignature = p.IsSignature,
                IsOwnership = p.isownership,
                Language = p.language,
                FormId = p.FormId,
                GridHeaderColor = p.GridHeaderColor,
                GridHeaderFontColor = p.GridHeaderFontColor,
                FormLayout = p.FormLayout != null ? PortalHelper.ChangeWcfFormLayoutToMvcLayout(p.FormLayout, p.WidgetGuid) : null,
                WidgetGuid = p.WidgetGuid,
                WidgetId = p.WidgetId,
                PageWidgetId = p.PageWidgetId.Replace(" ", ""),
                Count = p.Count,
                WidgetType = p.WidgetType,
                RedirectAfterCreate = p.RedirectAfterCreate,
                GridType = p.GridType,
                GridOnClick = p.GridOnClick,
                GridRecordPerPage = p.GridRecordPerPage,
                GridOnClickOpenFormId = p.GridOnClickOpenFormId,
                GridOnClickOpenHTMLId = p.GridOnClickOpenHTMLId,
                GridOnClickWidgetType = p.GridOnClickWidgetType,
                IsExcelExport = p.IsExcelExport,
                CalculatedOnClick = p.CalculatedOnClick,
                CalculatedOnClickOpenFormId = p.CalculatedOnClickOpenFormId,
                CalculatedTheme = p.CalculatedTheme,
                CalculatedIcon = p.CalculatedIcon,
                GridClickOpenFormStyle = p.GridClickOpenFormStyle,
                Width = p.WidgetType == "form" ? "100" : p.width,
                Color = p.color,
                HTML = p.HTML,
                Name = p.name,
                ChartColor = p.ChartColor,
                ChartFontSize = p.ChartFontSize,
                ChartFontFamily = p.ChartFontFamily,
                Is3D = p.Is3D,
                ChartLegendPosition = p.ChartLegendPosition,
                IsHandWrite = p.IsHandWrite,
                LabelText = p.LabelText,
                ChartType = p.ChartType,
                ChartData = p.ChartData.ToList().ConvertAll(h => h.ConvertClass<ChartData>()).ToList(),
                Actions = p.Actions.ToList().ConvertAll(y => y.ConvertClass<Actions>()).ToList(),
                GridData = p.GridData == null ? null : p.GridData.ToList().
                ConvertAll(a => new GridRowData
                    {
                        IsEmptyGrid = a.IsEmptyGrid,
                        RowNumber = a.RowNumber,
                        Width = a.Width,
                        Data = a.Data.ToList().ConvertAll(x => new GridData { ColumnName = x.ColumnName, DisplayName = x.DisplayName, RecordId = x.RecordId, Value = x.Value, Width = x.Width })
                    }),
                PictureData = JsonConvert.DeserializeObject<List<PictureData>>(JsonConvert.SerializeObject(p.PictureData)),

                GridLinkDescription = p.GridLinkDescription,
                ExternalLinkDescription = p.ExternalLinkDescription,
                FormLinkDescription = p.FormLinkDescription,
                ExternalLinkUrl = p.ExternalLinkUrl,
                IsExternalLink = p.IsExternalLink,
                IsGridLink = p.IsGridLink,
                IsFormLink = p.IsFormLink,
                IsSeperate = p.IsSeperate,
                EntityNameForLinkWidget = p.EntityNameForLinkWidget,
                OnClickOpenFormIdForLinkWidget = p.OnClickOpenFormIdForLinkWidget,
                FormBGColor = p.FormBGColor,
                FormIcon = p.FormIcon,
                GridBGColor = p.GridBGColor,
                GridIcon = p.GridIcon,
                ExternalURLBGColor = p.ExternalURLBGColor,
                ExternalURLIcon = p.ExternalURLIcon,

                FieldInfo = JsonConvert.DeserializeObject<FieldInfo>(JsonConvert.SerializeObject(p.FieldInfo)),
                NotificationDirection = p.NotificationDirection,
                NotificationList = JsonConvert.DeserializeObject<List<String>>(JsonConvert.SerializeObject(p.NotificationList)),

            });
            #endregion

            if (Session[URL + "Picture"] == null)
            {
                byte[] picture = client.GetPicture(portalserviceinfo,
                                                   PortalId,
                                                   Request.Cookies[URL + "entitynamecookie"].Values[0],
                                                   Request.Cookies[URL + "entityfieldcookie"].Values[0],
                                                   Request.Cookies[URL + "usernamevaluecookie"].Values[0]);
                //pictures can not insert into cookies due to their sizes!

                Session[URL + "Picture"] = Convert.ToBase64String(picture);
            }
            //orders
            modeldataview = modeldataview.OrderBy(p => Convert.ToInt32(p.Order)).ToList();

            //get parent dynamic scripts
            String ParentScript = client.GetDynamicScriptForEntity(PortalId);

            return View(new GeneralModel()
            {
                MenuViewModel = new MenuModel()
                {
                    Picture = Session[URL + "Picture"].ToString(),
                    LoggedUser = HttpContext.User.Identity.Name,
                    Menus = m,
                    Languages = Languages,
                    Scripts = ParentScript + Environment.NewLine + page.Scripts
                },
                WidgetDataViewModel = modeldataview,
                //if it is not coming from the login page. URL can be called directly.Need to check cookie values
                LocalizationModel = LanguageModel,
                ExtraViewModel = new ExtraModel()
                {
                    LoggedUserGuid = LoggedUserGuidValue,
                    PortalLanguage = LangId,
                    IsExternalWidget = IsComingFromExternal,
                    MailAdress = portalserviceinfo.PortalEmailField
                },
                ZoneModel = new ZoneModel
                {
                    IsFooterZoneActive = page.IsFooterZoneActive,
                    IsHeaderZoneActive = page.IsHeaderZoneActive,
                    IsLeftZoneActive = page.IsLeftZoneActive,
                    IsRightZoneActive = page.IsRightZoneActive,
                    MainZoneWidth = page.MainZoneWidth,
                    LeftZoneWidth = page.LeftZoneWidth,
                    RightZoneWidth = page.RightZoneWidth
                },
                PortalLogoURL = portalserviceinfo.PortalLogoURL
            });

        }

        [AllowAnonymous]
        [OutputCache(Duration=3600)]
        public ActionResult ExternalBuildPage(String NavigationId, String LangId)
        {

            //NavigationId = HttpContext.Application[PortalId].ToString();
            //check cookie control
            if (Request.Cookies[URL + "entitynamecookie"] == null)
            {
                return RedirectToAction("Logout", "Dashboard");

            }
            else if (String.IsNullOrEmpty(PortalId))
            {
                return RedirectToAction("Logout", "Dashboard");
            }

            //PortalService.PortalServiceClient client = new PortalService.PortalServiceClient();
            MenuModel model = new MenuModel();
            PortalService.Navigation[] ExternalNavigations = new PortalService.Navigation[] { };

            if (Session[URL + "externalmenu"] == null)
            {
                //PortalService.CrmServiceInformation serviceinfo = PortalHelper.GetCrmInformationFormConfig();
                try
                {
                    ExternalNavigations = client.GetExternalNavigation(PortalId);
                }
                catch (Exception ex)
                {
                    return RedirectToAction("ExternalErrorPage", "Error", new { Message = ex.Message });
                    //throw new Exception(ex.Message);
                }
                model = PortalHelper.ChangeWcfNavigationIntoMenuModel(String.Empty, ExternalNavigations);

                Session[URL + "externalmenu"] = JsonConvert.SerializeObject(model.Menus);
            }
            model = PortalHelper.ChangeWcfNavigationIntoMenuModel(String.Empty, ExternalNavigations);

            PortalService.Page page = new PortalService.Page();
            try
            {
                page = client.GetNavigationPage(portalserviceinfo, PortalId, NavigationId, String.Empty);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ExternalErrorPage", "Error", new { Message = ex.Message });
                //throw new Exception(ex.Message);
            }

            Helper.PortalHelper helper = new Helper.PortalHelper();
            List<WidgetModel> widgets = helper.GetXmlNode(page.FormLayoutXml, "widget");
            widgets = widgets.OrderBy(p => p.order).ToList();

            //first change the dynamic initial values--> not for external widgets yet!
            #region Initial Values

            foreach (var item in widgets)
            {
                if (item.initialvalues.Count > 0)
                {
                    List<PortalService.InitialValues> InitialValues = item.initialvalues.Where(p => p.iscomingfromsubgrids.Equals("0")).ToList().ConvertAll(p => new PortalService.InitialValues
                    {
                        AttributeLogicalName = p.attributelogicalname,
                        EntityLogicalName = p.entitylogicalname,
                        InitialValue = p.initialvalue,
                        LookupLogicalName = p.lookuplogicalname,
                        LookupNameValue = p.lookupnamevalue,
                        Static = p.Static
                    }).ToList();

                    List<PortalService.InitialValues> ChangedInitialValues = client.GetAndChangeDynamicInitialValues(portalserviceinfo,
                                                                                                                     PortalId,
                                                                                                                     InitialValues.ToArray(),
                                                                                                                     Request.Cookies[URL + "entitynamecookie"].Values[0],
                                                                                                                     Request.Cookies[URL + "usernamevaluecookie"].Values[0],
                                                                                                                     Request.Cookies[URL + "entityfieldcookie"].Values[0]).ToList();

                    //change WcfService InitialValue to Mvc InitialValue
                    foreach (var InitialItem in ChangedInitialValues)
                    {
                        InitialValues v = item.initialvalues.Where(p => p.attributelogicalname.Equals(InitialItem.AttributeLogicalName)).SingleOrDefault();

                        v.lookupnamevalue = InitialItem.LookupNameValue;
                        v.initialvalue = InitialItem.InitialValue;
                    }
                }
            }

            #endregion

            #region Prepare Parameters

            List<PortalService.WidgetParameters> parameters = widgets.ConvertAll(p => new PortalService.WidgetParameters
            {
                Order = p.order,
                PercentageofTotalWidth = p.PercentageofTotalWidth,
                WidgetId = p.id,
                GridPerPage = p.GridRecordPerPage,
                PageWidgetId = p.pagewidgetid,
                ActionsCount = Convert.ToString(p.Actions.Count),
                PictureHeight = p.PictureHeight,
                filters = p.filters.ConvertAll(a =>
                    new PortalService.Filters
                    {
                        type = a.type,
                        filter = a.filter.ConvertAll(x =>
                        new PortalService.Filter { Operator = x.Operator, uitype = x.uitype, attributtename = x.attributtename, condition = x.condition, isstatic = x.isstatic, value = x.value }).ToArray()
                    }).ToArray(),
                subgrids = p.subgridsandlookups.Where(z => z.type.Equals("subgrid")).ToList().ConvertAll(a =>
                       new PortalService.SubGridModel
                       {
                           SubGridViewId = a.SubGridViewId,
                           SubGridId = a.SubGridId,
                           SubGridLogicalName = a.SubGridLogicalName,
                           NewFormId = a.NewFormId,
                           UpdateFormId = a.UpdateFormId
                       }).ToArray()
            }).ToList();
            #endregion

            PortalService.WidgetData[] data = new PortalService.WidgetData[] { };

            data = client.GetWidgetData(portalserviceinfo, PortalId,
                                        parameters.ToArray(),
                                        Request.Cookies[URL + "entitynamecookie"].Values[0],
                                        String.Empty,
                                        Request.Cookies[URL + "entityfieldcookie"].Values[0],
                                        page.UseCache);

            HttpContext.Application[AbsolutePath + "widgets"] = widgets;
            HttpContext.Application[AbsolutePath + "parameters"] = parameters;

            foreach (var item in widgets)
            {
                //First Select Related Service Data Widget
                PortalService.WidgetData serviceclass = data.ToList().Where(p => p.PageWidgetId.Equals(item.pagewidgetid)).ToList().FirstOrDefault();
                //Pass XML Parameters into temp data --> These parameters is in the LayoutXML but not in Page Widget Filter Entity. So we need to compensate them            
                serviceclass = item.ConvertClass<PortalService.WidgetData>();
                //Pass Temp Class members to Service Data Widget
                serviceclass.CopyValues<PortalService.WidgetData>(data.ToList().Where(p => p.PageWidgetId.Equals(item.pagewidgetid)).ToList().FirstOrDefault());
                //convert Actions!
                List<PortalService.Actions> ServiceActions = new List<PortalService.Actions>();
                foreach (var actions in item.Actions)
                {
                    ServiceActions.Add(actions.ConvertClass<PortalService.Actions>());
                }
                data.ToList().Where(d => d.PageWidgetId.Equals(item.pagewidgetid)).ToList().FirstOrDefault().Actions = ServiceActions.ToArray();

            }

            #region Change Wcf Service Classes into Model

            List<WidgetDataModel> modeldataview = data.ToList().ConvertAll(p => new WidgetDataModel
            {
                Values = p.Values != null ? p.Values.ToList().ConvertAll(z => new CalendarData { Value = z.Value, StartDateValue = z.StartDateValue, EndDateValue = z.EndDateValue }) : null,
                Order = p.Order,
                Zone = p.Zone,
                PercentageofTotalWidth = p.PercentageofTotalWidth,
                EntityName = p.EntityName,
                IsEditable = p.iseditable,
                Language = p.language,
                IsOwnership = p.isownership,
                FormId = p.FormId,
                FormLayout = p.FormLayout != null ? PortalHelper.ChangeWcfFormLayoutToMvcLayout(p.FormLayout, p.WidgetGuid) : null,
                WidgetGuid = p.WidgetGuid,
                WidgetId = p.WidgetId,
                PageWidgetId = p.PageWidgetId.Replace(" ", ""),
                RedirectAfterCreate = p.RedirectAfterCreate,
                Count = p.Count,
                WidgetType = p.WidgetType,
                GridType = p.GridType,
                GridOnClick = p.GridOnClick,
                GridRecordPerPage = p.GridRecordPerPage,
                GridOnClickOpenFormId = p.GridOnClickOpenFormId,
                GridOnClickOpenHTMLId = p.GridOnClickOpenHTMLId,
                GridOnClickWidgetType = p.GridOnClickWidgetType,
                IsExcelExport = p.IsExcelExport,
                CalculatedOnClick = p.CalculatedOnClick,
                CalculatedOnClickOpenFormId = p.CalculatedOnClickOpenFormId,
                CalculatedTheme = p.CalculatedTheme,
                CalculatedIcon = p.CalculatedIcon,
                GridClickOpenFormStyle = p.GridClickOpenFormStyle,
                Width = p.WidgetType == "form" ? "100" : p.width,
                HTML = p.HTML,
                Color = p.color,
                Name = p.name,
                Actions = p.Actions.ToList().ConvertAll(y => y.ConvertClass<Actions>()).ToList(),
                GridData = p.GridData == null ? null : p.GridData.ToList().
                ConvertAll(a => new GridRowData
                {
                    IsEmptyGrid = a.IsEmptyGrid,
                    RowNumber = a.RowNumber,
                    Width = a.Width,
                    Data = a.Data.ToList().ConvertAll(x => new GridData { ColumnName = x.ColumnName, DisplayName = x.DisplayName, RecordId = x.RecordId, Value = x.Value, Width = x.Width })
                }),
                PictureData = JsonConvert.DeserializeObject<List<PictureData>>(JsonConvert.SerializeObject(p.PictureData)),

                GridLinkDescription = p.GridLinkDescription,
                ExternalLinkDescription = p.ExternalLinkDescription,
                FormLinkDescription = p.FormLinkDescription,
                ExternalLinkUrl = p.ExternalLinkUrl,
                IsExternalLink = p.IsExternalLink,
                IsGridLink = p.IsGridLink,
                IsFormLink = p.IsFormLink,
                IsSeperate = p.IsSeperate,
                EntityNameForLinkWidget = p.EntityNameForLinkWidget,
                OnClickOpenFormIdForLinkWidget = p.OnClickOpenFormIdForLinkWidget,
                FormBGColor = p.FormBGColor,
                FormIcon = p.FormIcon,
                GridBGColor = p.GridBGColor,
                GridIcon = p.GridIcon,
                ExternalURLBGColor = p.ExternalURLBGColor,
                ExternalURLIcon = p.ExternalURLIcon,

                FieldInfo = JsonConvert.DeserializeObject<FieldInfo>(JsonConvert.SerializeObject(p.FieldInfo)),
                NotificationDirection = p.NotificationDirection,
                NotificationList = JsonConvert.DeserializeObject<List<String>>(JsonConvert.SerializeObject(p.NotificationList)),

            });
            #endregion

            modeldataview = modeldataview.OrderBy(p => Convert.ToInt32(p.Order)).ToList();

            //Get Localization
            LocalizationModel LocalizationModel = Portal.Helper.PortalHelper.ParsePortalLangXml(HttpContext.Server.MapPath("~/Helper/Localization.xml"), PortalHelper.GetLangIdOfCultureName(LangId));

            //get parent dynamic scripts
            String ParentScript = client.GetDynamicScriptForEntity(PortalId);


            return View("BuildPage", new GeneralModel()
                {
                    MenuViewModel = new MenuModel()
                    {
                        Picture = String.Empty,
                        LoggedUser = String.Empty,
                        Menus = JsonConvert.DeserializeObject<List<Menu>>(Session[URL + "externalmenu"].ToString()),
                        Scripts = ParentScript + Environment.NewLine + page.Scripts,
                        ComingFrom = Enums.MenuSource.External.ToString(),
                        Languages = Languages,
                    },
                    WidgetDataViewModel = modeldataview,
                    LocalizationModel = LanguageModel,
                    ExtraViewModel = new ExtraModel()
                    {
                        LoggedUserGuid = LoggedUserGuidValue,
                        PortalLanguage = LangId,
                        MailAdress = portalserviceinfo.PortalEmailField
                    },
                    ZoneModel = new ZoneModel
                    {
                        IsFooterZoneActive = page.IsFooterZoneActive,
                        IsHeaderZoneActive = page.IsHeaderZoneActive,
                        IsLeftZoneActive = page.IsLeftZoneActive,
                        IsRightZoneActive = page.IsRightZoneActive,
                        MainZoneWidth = page.MainZoneWidth,
                        LeftZoneWidth = page.LeftZoneWidth,
                        RightZoneWidth = page.RightZoneWidth
                    },
                    PortalLogoURL = portalserviceinfo.PortalLogoURL
                });

        }

        [AllowAnonymous]
        public ActionResult RenderExternalWidget(String PortalId, String WidgetId)
        {
            return View(new GeneralModel()
            {
                MenuViewModel = new MenuModel()
                {
                    URL = Request.Url.Authority
                }

            });
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult SendFormDataToCrm(String FormData, String EntityName, String Signature, String Ownership, String RelationShipName, String ParentId)
        {
            //PortalService.PortalServiceClient client = new PortalService.PortalServiceClient();
            List<PortalService.FormData> Data = (List<PortalService.FormData>)JsonConvert.DeserializeObject(FormData, typeof(List<PortalService.FormData>));
            PortalService.CreatedData Result = client.CreateData(portalserviceinfo,
                                                                  PortalId,
                                                                  EntityName,
                                                                  Data.ToArray(),
                                                                  Signature.Replace("data:image/png;base64,", ""),
                                                                  Ownership,
                                                                  Request.Cookies[URL + "entitynamecookie"].Values[0],
                                                                  Request.Cookies[URL + "usernamevaluecookie"].Values[0],
                                                                  Request.Cookies[URL + "entityfieldcookie"].Values[0],
                                                                  RelationShipName,
                                                                  ParentId);


            return Json(Result);

        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult UpdateFormDataToCrm(String FormData, String EntityName, String Id, String Ownership)
        {
            //if(Request.UrlReferrer != null)
            //    HttpResponse.RemoveOutputCacheItem(Request.UrlReferrer.AbsolutePath);
            //HttpResponse.RemoveOutputCacheItem(Request.Path);


            //PortalService.PortalServiceClient client = new PortalService.PortalServiceClient();
            List<PortalService.FormData> Data = (List<PortalService.FormData>)JsonConvert.DeserializeObject(FormData, typeof(List<PortalService.FormData>));
            String Result = client.UpdateData(
                                              portalserviceinfo,
                                              PortalId,
                                              EntityName,
                                              Data.ToArray(),
                                              Id,
                                              Ownership,
                                              Request.Cookies[URL + "entitynamecookie"].Values[0],
                                              Request.Cookies[URL + "usernamevaluecookie"].Values[0],
                                              Request.Cookies[URL + "entityfieldcookie"].Values[0]);

            return Json(Result);
        }

        [AllowAnonymous]
        public ActionResult GetCreateFormForSubGrid(String FormId, String WidgetGuid, String WidgetId, String PageWidgetId, String RelationShipName, String ParentId, String SubGridId)
        {
            PortalService.PortalServiceClient client = new PortalService.PortalServiceClient();
            PortalService.FormLayout layout = client.GetEditFormLayout(portalserviceinfo,
                                                                       PortalId,
                                                                       FormId,
                                                                       String.Empty,
                                                                       null);

            List<WidgetModel> model = ((List<WidgetModel>)HttpContext.Application[LoggedUserGuidValue + "widgets"]).ToList();
            InitialValues(model, "1");


            FormLayout formlayouts = PortalHelper.ChangeWcfFormLayoutToMvcLayout(layout, WidgetGuid);
            WidgetDataModel m = new WidgetDataModel();
            m.FormLayout = formlayouts;
            m.PageWidgetId = PageWidgetId;
            m.WidgetGuid = WidgetGuid;
            m.WidgetId = WidgetId;
            m.RelationShipName = RelationShipName;
            m.ParentId = ParentId;
            m.SubGridId = SubGridId;

            List<WidgetDataModel> WidgetDataViewModel = new List<WidgetDataModel>();
            WidgetDataViewModel.Add(m);

            GeneralModel GeneralModel = new GeneralModel
            {
                WidgetDataViewModel = WidgetDataViewModel,
                LocalizationModel = LanguageModel
            };
            return PartialView("SubGridCreateUpdateForm", GeneralModel);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult GetBusinessProcessFlowHTML(String BusinessProcessFlowList, String DataWidgetID, String SelectedBPF) 
        {
            List<BusinessProcessFlow> businessProcessFlowList = new List<BusinessProcessFlow>();

            businessProcessFlowList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BusinessProcessFlow>>(BusinessProcessFlowList);
            ViewData["data-widgetid"] = DataWidgetID;

            BusinessProcessFlow businessProcessFlow = new BusinessProcessFlow();

            if (string.IsNullOrEmpty(SelectedBPF))
            {
                businessProcessFlow = businessProcessFlowList.FirstOrDefault();
            }
            else
            {
                businessProcessFlow = businessProcessFlowList.Where(z => z.Title.Equals(SelectedBPF)).FirstOrDefault();
            }

            return PartialView("BusinessProcessFlowPartial", businessProcessFlow);
        }

        [AllowAnonymous]
        public ActionResult GetCreateForm(String FormId, String WidgetId, String WidgetGuid, String PageWidgetId)
        {
            //PortalService.PortalServiceClient client = new PortalService.PortalServiceClient();

            PortalService.FormLayout layout = client.GetEditFormLayout(portalserviceinfo,
                                                                       PortalId,
                                                                       FormId,
                                                                       String.Empty,
                                                                       null);

            FormLayout formlayouts = PortalHelper.ChangeWcfFormLayoutToMvcLayout(layout, WidgetGuid);
            WidgetDataModel m = new WidgetDataModel();
            m.FormLayout = formlayouts;
            m.PageWidgetId = PageWidgetId;
            m.WidgetGuid = WidgetGuid;
            m.WidgetId = WidgetId;
            m.WidgetType = "form";
            m.Width = "100";

            List<WidgetDataModel> WidgetDataViewModel = new List<WidgetDataModel>();
            WidgetDataViewModel.Add(m);

            GeneralModel GeneralModel = new GeneralModel
            {
                WidgetDataViewModel = WidgetDataViewModel,
                LocalizationModel = LanguageModel,
                ExtraViewModel = new ExtraModel
                {
                    FormOpenType = "modal",
                    Editable = "true",
                    Ownership = "0"
                }
            };
            return PartialView("CreateFormPartial", GeneralModel);
        }

        [AllowAnonymous]
        [OutputCache(Duration = 0)]
        public ActionResult EditForm(String FormId, String DataId, String WidgetGuid, String NavigationId, String WidgetId, String Editable, String Ownership, String FormOpenType, String PageWidgetId, String IsComingFromSubGrid, String IsComingFromLookupDetail)
        {
            //PortalService.PortalServiceClient client = new PortalService.PortalServiceClient();
            Helper.PortalHelper helper = new Helper.PortalHelper();

            if (Session[URL + "Picture"] == null)
            {
                if (Request.Cookies[URL + "entitynamecookie"].Values[0] != String.Empty)
                {
                    byte[] picture = client.GetPicture(portalserviceinfo,
                                                       PortalId,
                                                       Request.Cookies[URL + "entitynamecookie"].Values[0],
                                                       Request.Cookies[URL + "entityfieldcookie"].Values[0],
                                                       Request.Cookies[URL + "usernamevaluecookie"].Values[0]);
                    //pictures can not insert into cookies due to their sizes!
                    Session[URL + "Picture"] = Convert.ToBase64String(picture);
                }
            }

            PortalService.Page page = new PortalService.Page();
            try
            {
                page = client.GetNavigationPage(portalserviceinfo, PortalId, NavigationId, Request.Cookies[URL + "entitynamecookie"].Values[0].ToString());
            }
            catch (Exception ex)
            {
                if (ex.Message == "Service null Exception")
                    return RedirectToAction("Logout", "Dashboard");
                else
                    return RedirectToAction("ExternalErrorPage", "Error", new { Message = ex.Message });
                    //throw new Exception(ex.Message);
            }
            //for lookup filters
            PortalService.WidgetParameters Params = new PortalService.WidgetParameters();
            WidgetModel Widgets = new WidgetModel();
            if (!String.IsNullOrEmpty(LoggedUserGuidValue))
            {
                Params = ((List<PortalService.WidgetParameters>)HttpContext.Application[LoggedUserGuidValue + "parameters"]).Where(p => p.PageWidgetId.Equals(PageWidgetId)).FirstOrDefault();
                Widgets = ((List<WidgetModel>)HttpContext.Application[LoggedUserGuidValue + "widgets"]).ToList().Where(p => p.pagewidgetid.Equals(PageWidgetId)).FirstOrDefault();
            }
            //means coming from external
            else
            {
                Params = ((List<PortalService.WidgetParameters>)HttpContext.Application[Referrer + "parameters"]).Where(p => p.PageWidgetId.Equals(PageWidgetId)).FirstOrDefault();
                Widgets = ((List<WidgetModel>)HttpContext.Application[Referrer + "widgets"]).ToList().Where(p => p.pagewidgetid.Equals(PageWidgetId)).FirstOrDefault();
            }


            PortalService.FormLayout layout = client.GetEditFormLayout(portalserviceinfo,
                                                                       PortalId,
                                                                       FormId,
                                                                       DataId,
                                                                       Params.subgrids);

            List<FormDataDetail> datalist = new List<FormDataDetail>();
            foreach (var tabs in layout.Tabs)
            {
                if (Convert.ToBoolean(Convert.ToInt32(IsComingFromLookupDetail)))
                {
                    if (tabs.IsBpf)
                    {
                        continue;
                    }
                }

                foreach (var item in tabs.FormData)
                {
                    FormDataDetail d = new FormDataDetail();
                    d.LogicalName = item.LogicalName;
                    d.Value = item.Value;
                    d.LookUpValueName = item.LookUpValueName;
                    d.Type = item.Type;
                    d.LookupLogicalName = item.LookupLogicalName;
                    datalist.Add(d);
                }
            }

            #region Wcf Service convert into model

            FormLayout formlayouts = PortalHelper.ChangeWcfFormLayoutToMvcLayout(layout, WidgetGuid);

            if (Convert.ToBoolean(Convert.ToInt32(IsComingFromLookupDetail)))
            {
                formlayouts.Tabs.RemoveAll(z => z.IsBpf.Equals("true"));
            }
            #endregion

            List<WidgetDataModel> viewdatamodel = new List<WidgetDataModel>();
            WidgetDataModel m = new WidgetDataModel();
            m.FormLayout = formlayouts;
            m.FormDatas = datalist;
            m.Actions = Widgets.Actions;
            viewdatamodel.Add(m);

            String ParentScript = client.GetDynamicScriptForEntity(PortalId);
            GeneralModel model = new GeneralModel()
            {
                MenuViewModel = new MenuModel()
                {
                    IsVisible = DataId == null ? "0" : "1",
                    Picture = Session[URL + "Picture"] == null ? String.Empty : Session[URL + "Picture"].ToString(),
                    LoggedUser = HttpContext.User.Identity.Name,
                    Scripts = ParentScript + Environment.NewLine + page.Scripts,
                    Languages = Languages
                },
                WidgetDataViewModel = viewdatamodel,
                ExtraViewModel = new ExtraModel()
                {
                    Editable = Editable,
                    PageWidgetId = PageWidgetId,
                    WidgetId = WidgetId,
                    DataId = DataId,
                    Ownership = Ownership,
                    FormOpenType = FormOpenType,
                    IsComingFromSubGrid = IsComingFromSubGrid,
                    IsComingLookupDetail = IsComingFromLookupDetail,
                    PortalLanguage = PortalHelper.GetNameOfLangId(MainLangId),
                    IsEditFormInNewWindow = FormOpenType != "modal" ? "newwindow" : String.Empty
                },
                LocalizationModel = LanguageModel
            };

            if (FormOpenType != "modal")
            {
                return View("EditForm", model);
            }
            return PartialView("EditFormPartialModal", model);

        }

        [HttpPost]
        public ActionResult GetCreatedSubGriData(String EntityName, String EntityId, String Columns)
        {
            //PortalService.PortalServiceClient client = new PortalService.PortalServiceClient();
            PortalService.SubGridRecords[] records = client.GetSubGridRecords(portalserviceinfo,
                                                                                 PortalId,
                                                                                 EntityName,
                                                                                 EntityId,
                                                                                 JsonConvert.DeserializeObject<String[]>(Columns));

            var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            var result = new ContentResult
            {
                Content = serializer.Serialize(records),
                ContentType = "application/json"
            };
            return Json(result);
        }

        [HttpPost]
        public ActionResult GetAllRelatedSubGridRecords(String SubGridViewId, String RelationShipName, String ParentId)
        {
            //PortalService.PortalServiceClient client = new PortalService.PortalServiceClient();
            PortalService.GridRowData[] records = client.GetRelatedSubGridRecords(portalserviceinfo,
                                                                                     PortalId,
                                                                                     SubGridViewId,
                                                                                     RelationShipName,
                                                                                     ParentId);


            var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            var result = new ContentResult
            {
                Content = serializer.Serialize(records),
                ContentType = "application/json"
            };
            return Json(result);
        }

        [AllowAnonymous]
        public ActionResult OpenHTMLWidget(String WidgetId, String WidgetGuid, String EntityId, String OpenType, String NavigationId)
        {
            //PortalService.PortalServiceClient client = new PortalService.PortalServiceClient();
            Helper.PortalHelper helper = new Helper.PortalHelper();


            String data = client.GetHtmlWidgetContent(portalserviceinfo,
                                                      PortalId,
                                                      WidgetGuid,
                                                      EntityId,
                                                      Request.Cookies[URL + "entitynamecookie"].Values[0],
                                                       Request.Cookies[URL + "usernamevaluecookie"] == null ? String.Empty : Request.Cookies[URL + "usernamevaluecookie"].Values[0],
                                                      Request.Cookies[URL + "entityfieldcookie"].Values[0]);


            if (OpenType == "modal")
            {
                var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
                var result = new ContentResult
                {
                    Content = serializer.Serialize(data),
                    ContentType = "application/json"
                };
                return Json(result, JsonRequestBehavior.AllowGet);

            }

            if (Session[URL + "Picture"] == null)
            {
                if (Request.Cookies[URL + "entitynamecookie"].Values[0] != String.Empty)
                {
                    byte[] picture = client.GetPicture(portalserviceinfo,
                                                       PortalId,
                                                       Request.Cookies[URL + "entitynamecookie"].Values[0],
                                                       Request.Cookies[URL + "entityfieldcookie"].Values[0],
                                                       Request.Cookies[URL + "usernamevaluecookie"].Values[0]);
                    //pictures can not insert into cookies due to their sizes!

                    Session[URL + "Picture"] = Convert.ToBase64String(picture);
                }
            }

            PortalService.Page page = new PortalService.Page();
            try
            {
                page = client.GetNavigationPage(portalserviceinfo, PortalId, NavigationId, Request.Cookies[URL + "entitynamecookie"].Values[0].ToString());
            }
            catch (Exception ex)
            {
                if (ex.Message == "Service null Exception")
                    return RedirectToAction("Logout", "Dashboard");
                else
                    return RedirectToAction("ExternalErrorPage", "Error", new { Message = ex.Message });
                    //throw new Exception(ex.Message);
            }

            List<WidgetDataModel> viewdatamodel = new List<WidgetDataModel>();
            WidgetDataModel m = new WidgetDataModel();
            m.HTML = data;
            viewdatamodel.Add(m);

            String ParentScript = client.GetDynamicScriptForEntity(PortalId);
            GeneralModel model = new GeneralModel()
            {
                MenuViewModel = new MenuModel()
                {
                    IsVisible = "1",
                    Picture = Session[URL + "Picture"] == null ? String.Empty : Session[URL + "Picture"].ToString(),
                    LoggedUser = HttpContext.User == null ? String.Empty : HttpContext.User.Identity.Name,
                    Scripts = ParentScript + Environment.NewLine + page.Scripts
                },
                WidgetDataViewModel = viewdatamodel,
                ExtraViewModel = new ExtraModel()
                {
                    WidgetId = WidgetId,
                },
                PortalLogoURL = portalserviceinfo.PortalLogoURL
            };

            return View(model);
        }

        public ActionResult PersonelInfo(String LangId)
        {

            //PortalService.PortalServiceClient client = new PortalService.PortalServiceClient();

            if (Session[URL + "Picture"] == null)
            {
                byte[] picture = client.GetPicture(portalserviceinfo,
                                                   PortalId,
                                                   Request.Cookies[URL + "entitynamecookie"].Values[0],
                                                   Request.Cookies[URL + "entityfieldcookie"].Values[0],
                                                   Request.Cookies[URL + "usernamevaluecookie"].Values[0]);
                //pictures can not insert into cookies due to their sizes!

                Session[URL + "Picture"] = Convert.ToBase64String(picture);
            }


            PortalService.PersonelInformationForm form = client.GetPersonelInformation(portalserviceinfo,
                                                                                       PortalId,
                                                                                       Request.Cookies[URL + "entitynamecookie"].Values[0],
                                                                                       Request.Cookies[URL + "usernamevaluecookie"].Values[0],
                                                                                       Request.Cookies[URL + "entityfieldcookie"].Values[0]);

            PortalService.FormLayout layout = client.GetEditFormLayout(portalserviceinfo,
                                                                        PortalId,
                                                                        form.FormId,
                                                                        form.EntityId,
                                                                        null);


            #region Wcf Service convert into model
            FormLayout formlayouts = PortalHelper.ChangeWcfFormLayoutToMvcLayout(layout, form.WidgetGuid);

            List<FormDataDetail> datalist = new List<FormDataDetail>();
            foreach (var tabs in formlayouts.Tabs)
            {
                foreach (var item in tabs.FormData)
                {
                    FormDataDetail d = new FormDataDetail();
                    d.LogicalName = item.LogicalName;
                    d.Value = item.Value;
                    d.LookUpValueName = item.LookUpValueName;
                    d.Type = item.Type;
                    datalist.Add(d);
                }
            }
            #endregion

            List<WidgetDataModel> viewdatamodel = new List<WidgetDataModel>();
            WidgetDataModel m = new WidgetDataModel();
            m.FormLayout = formlayouts;
            m.FormDatas = datalist;
            viewdatamodel.Add(m);

            String ParentScript = client.GetDynamicScriptForEntity(PortalId);

            if (!cache.Contains(PortalId + LoggedUserGuidValue + "menucache"))
            {
                return RedirectToAction("Logout", "Dashboard");
            }

            String DecodedValues = Server.UrlDecode(cache[PortalId + LoggedUserGuidValue + "menucache"].ToString());

            List<Menu> menu = JsonConvert.DeserializeObject<List<Menu>>(DecodedValues);

            return View(new GeneralModel()
                              {
                                  MenuViewModel = new MenuModel()
                                  {
                                      Picture = Session[URL + "Picture"].ToString(),
                                      LoggedUser = HttpContext.User.Identity.Name,
                                      Menus = menu,
                                      Scripts = ParentScript,
                                      Languages = Languages,
                                  },
                                  WidgetDataViewModel = viewdatamodel,
                                  ExtraViewModel = new ExtraModel()
                                  {
                                      WidgetId = form.WidgetId,
                                      DataId = form.EntityId,
                                      PortalLanguage = LangId,
                                      IsComingFromPersonel = "true"
                                  },
                                  LocalizationModel = LanguageModel,
                                  PortalLogoURL = portalserviceinfo.PortalLogoURL
                              });
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetGridData(String WidgetId, String PageNumber, String RecordCount)
        {
            //PortalService.PortalServiceClient client = new PortalService.PortalServiceClient();
            PortalService.GridRowData[] data = client.GetGridDataPerPage(portalserviceinfo,
                                                                           PortalId,
                                                                           ((List<PortalService.WidgetParameters>)HttpContext.Application[LoggedUserGuidValue + "parameters"]).ToArray(),
                                                                           WidgetId,
                                                                           Request.Cookies[URL + "entitynamecookie"].Values[0],
                                                                           Request.Cookies[URL + "usernamevaluecookie"].Values[0],
                                                                           Request.Cookies[URL + "entityfieldcookie"].Values[0],
                                                                           PageNumber,
                                                                           RecordCount,
                                                                           String.Empty);

            var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            var result = new ContentResult
            {
                Content = serializer.Serialize(data),
                ContentType = "application/json"
            };
            return Json(result);

        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult GetSearchGridData(String WidgetId, String PageNumber, String RecordCount, String SearchValue)
        {
            //PortalService.PortalServiceClient client = new PortalService.PortalServiceClient();

            PortalService.WidgetParameters[] Params = null;
            if (!String.IsNullOrEmpty(LoggedUserGuidValue))
            {
                Params = ((List<PortalService.WidgetParameters>)HttpContext.Application[LoggedUserGuidValue + "parameters"]).Where(p => p.PageWidgetId.Equals(WidgetId)).ToArray();

            }
            //means coming from external
            else
            {
                Params = ((List<PortalService.WidgetParameters>)HttpContext.Application[Referrer + "parameters"]).Where(p => p.PageWidgetId.Equals(WidgetId)).ToArray();
            }

            PortalService.GridRowData[] data = client.GetGridDataPerPage(portalserviceinfo,
                                                                          PortalId,
                                                                          Params,
                                                                          WidgetId,
                                                                          Request.Cookies[URL + "entitynamecookie"].Values[0],
                                                                          Request.Cookies[URL + "usernamevaluecookie"] == null ? String.Empty : Request.Cookies[URL + "usernamevaluecookie"].Values[0],
                                                                          Request.Cookies[URL + "entityfieldcookie"].Values[0],
                                                                          PageNumber,
                                                                          RecordCount,
                                                                          SearchValue);

            var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            var result = new ContentResult
            {
                Content = serializer.Serialize(data),
                ContentType = "application/json"
            };
            return Json(result);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult GetGridForModal(String CalculatedWidgetId, String WidgetGuid, String PageNumber, String RecordCount)
        {
            //PortalService.PortalServiceClient client = new PortalService.PortalServiceClient();

            List<PortalService.WidgetParameters> model = ((List<PortalService.WidgetParameters>)HttpContext.Application[LoggedUserGuidValue + "parameters"]).Where(p => p.PageWidgetId.Equals(CalculatedWidgetId)).ToList();
            PortalService.GridRowData[] data = client.GetCalculatedFieldRecords(portalserviceinfo,
                                                                                 PortalId,
                                                                                 CalculatedWidgetId,
                                                                                 WidgetGuid,
                                                                                 model.SingleOrDefault().filters.ToArray(),
                                                                                 Request.Cookies[URL + "entitynamecookie"].Values[0],
                                                                                 Request.Cookies[URL + "usernamevaluecookie"].Values[0],
                                                                                 Request.Cookies[URL + "entityfieldcookie"].Values[0],
                                                                                 PageNumber,
                                                                                 String.Empty,
                                                                                 String.IsNullOrEmpty(model.FirstOrDefault().GridPerPage) ? "10" : model.FirstOrDefault().GridPerPage
                                                                                 );

            var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            var result = new ContentResult
            {
                Content = serializer.Serialize(data),
                ContentType = "application/json"
            };
            return Json(result);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult GetSearchGridForModal(String CalculatedWidgetId, String WidgetGuid, String PageNumber, String RecordCount, String SearchValue)
        {
            //PortalService.PortalServiceClient client = new PortalService.PortalServiceClient();

            List<PortalService.WidgetParameters> model = ((List<PortalService.WidgetParameters>)HttpContext.Application[LoggedUserGuidValue + "parameters"]).Where(p => p.PageWidgetId.Equals(CalculatedWidgetId)).ToList();
            PortalService.GridRowData[] data = client.GetCalculatedFieldRecords(portalserviceinfo,
                                                                                 PortalId,
                                                                                 CalculatedWidgetId,
                                                                                 WidgetGuid,
                                                                                 model.FirstOrDefault().filters.ToArray(),
                                                                                 Request.Cookies[URL + "entitynamecookie"].Values[0],
                                                                                 Request.Cookies[URL + "usernamevaluecookie"].Values[0],
                                                                                 Request.Cookies[URL + "entityfieldcookie"].Values[0],
                                                                                 PageNumber,
                                                                                 SearchValue,
                                                                                 String.IsNullOrEmpty(model.FirstOrDefault().GridPerPage) ? "10" : model.FirstOrDefault().GridPerPage);

            var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            var result = new ContentResult
            {
                Content = serializer.Serialize(data),
                ContentType = "application/json"
            };
            return Json(result);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult UpdateEntityImage(String Image)
        {
            //PortalService.PortalServiceClient client = new PortalService.PortalServiceClient();
            String data = client.UpdateEntityImage(portalserviceinfo,
                                                   PortalId,
                                                   Image,
                                                   Request.Cookies[URL + "entitynamecookie"].Values[0],
                                                   Request.Cookies[URL + "usernamevaluecookie"].Values[0],
                                                   Request.Cookies[URL + "entityfieldcookie"].Values[0]);
            if (data == String.Empty)
            {
                Session[URL + "Picture"] = Image;
            }

            var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            var result = new ContentResult
            {
                Content = serializer.Serialize(data),
                ContentType = "application/json"
            };
            return Json(result);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult GetNotes(String Id, String LogicalName, String Type, String JavaScript)
        {
            return PartialView("NoteButtons", new ViewDataDictionary { { "id", Id }, { "name", LogicalName }, { "type", Type } });
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult InsertAttachment(String Attachment)
        {
            //PortalService.PortalServiceClient client = new PortalService.PortalServiceClient();

            PortalService.Attachment AttachmentData = (PortalService.Attachment)JsonConvert.DeserializeObject(Attachment, typeof(PortalService.Attachment));
            PortalService.AttachmentReturn ReturnData = client.AddNotesToRelatedRecord(portalserviceinfo, PortalId, AttachmentData);

            var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            var result = new ContentResult
            {
                Content = serializer.Serialize(ReturnData),
                ContentType = "application/json"
            };
            return Json(result);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult DeleteAttachment(String AttachmentId)
        {
            //PortalService.PortalServiceClient client = new PortalService.PortalServiceClient();
            String ReturnData = client.DeleteNote(portalserviceinfo, PortalId, AttachmentId);

            var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            var result = new ContentResult
            {
                Content = serializer.Serialize(ReturnData),
                ContentType = "application/json"
            };
            return Json(result);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult RenderCustomActions(String EntityId, String WorkFlowId)
        {
            //PortalService.PortalServiceClient client = new PortalService.PortalServiceClient();
            String ReturnData = client.ExecuteCustomActions(portalserviceinfo, PortalId, EntityId, WorkFlowId);

            var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            var result = new ContentResult
            {
                Content = serializer.Serialize(ReturnData),
                ContentType = "application/json"
            };

            return Json(result);
        }




        private void InitialValues(List<WidgetModel> model, String IsSubGrid)
        {
            //PortalService.PortalServiceClient client = new PortalService.PortalServiceClient();
            foreach (var item in model)
            {
                if (item.initialvalues.Count > 0)
                {
                    List<PortalService.InitialValues> InitialValues = item.initialvalues.Where(p => p.iscomingfromsubgrids.Equals(IsSubGrid)).ToList().ConvertAll(p => new PortalService.InitialValues
                    {
                        AttributeLogicalName = p.attributelogicalname,
                        EntityLogicalName = p.entitylogicalname,
                        InitialValue = p.initialvalue,
                        LookupLogicalName = p.lookuplogicalname,
                        LookupNameValue = p.lookupnamevalue,
                        Static = p.Static
                    }).ToList();

                    List<PortalService.InitialValues> ChangedInitialValues = client.GetAndChangeDynamicInitialValues(portalserviceinfo,
                                                                                                                     PortalId,
                                                                                                                     InitialValues.ToArray(),
                                                                                                                     Request.Cookies[URL + "entitynamecookie"].Values[0],
                                                                                                                     Request.Cookies[URL + "usernamevaluecookie"].Values[0],
                                                                                                                     Request.Cookies[URL + "entityfieldcookie"].Values[0]).ToList();

                    //change WcfService InitialValue to Mvc InitialValue
                    foreach (var InitialItem in ChangedInitialValues)
                    {
                        InitialValues v = item.initialvalues.Where(p => p.attributelogicalname.Equals(InitialItem.AttributeLogicalName)).FirstOrDefault();

                        v.lookupnamevalue = InitialItem.LookupNameValue;
                        v.initialvalue = InitialItem.InitialValue;
                    }
                }
            }
        }
    }
}
