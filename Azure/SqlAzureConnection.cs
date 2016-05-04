using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using ClassLibrary;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Azure
{
    public class SqlAzureConnection
    {
        private String _connectionString;
        private SqlConnection _connection;
        private ConfigurationSqlDataContext _sqlDataContext;

        public String ConnectionString
        {
            get { return _connectionString; }
        }

        public SqlConnection Connection
        {
            get { return _connection; }
        }

        public ConfigurationSqlDataContext SqlDataContext
        {
            get { return _sqlDataContext; }
        }

        public SqlAzureConnection()
        {
            _connectionString = String.Format("Server={0};Database={1};User ID={2};Password={3};Trusted_Connection=False;Encrypt={4};",
                                               ConfigurationManager.AppSettings["sqlserver"].ToString(),
                                               ConfigurationManager.AppSettings["sqldatabase"].ToString(),
                                               ConfigurationManager.AppSettings["sqluser"].ToString(),
                                               ConfigurationManager.AppSettings["sqlpass"].ToString(),
                                               ConfigurationManager.AppSettings["sqlencrypt"].ToString());
        }

        public String GetWcfURL(String UniqueURL, String Environment)
        {
            String URL = String.Empty;

            using (_connection = new SqlConnection(_connectionString))
            {
                using (_sqlDataContext = new ConfigurationSqlDataContext(_connection))
                {
                    var serviceinfo = (from p in _sqlDataContext.p4crm_portalBases

                                   where p.p4crm_domainurl.Equals(UniqueURL) && p.statecode.Equals(0) && p.statuscode.Equals(1)
                                   select new
                                   {
                                       DevURL = p.p4crm_configurationBase.p4crm_developmentserviceurl,
                                       TestURL = p.p4crm_configurationBase.p4crm_testserviceurl,
                                       ProdURL = p.p4crm_configurationBase.p4crm_productionserviceurl,
                                   }).FirstOrDefault();

                    if (Environment == "dev")
                    {
                        URL = serviceinfo.DevURL.Replace("/rest/","/soap/");
                    }
                    else if (Environment == "test")
                    {
                        URL = serviceinfo.TestURL.Replace("/rest/", "/soap/");
                    }
                    else if (Environment == "prod")
                    {
                        URL = serviceinfo.ProdURL.Replace("/rest/", "/soap/");
                    }
                    
                }
            }
            return URL;
        }

        public CrmServiceInformation GetCrmConnectionInformation(String URL)
        {
            CrmServiceInformation serviceinfo = new CrmServiceInformation();
            var CultureList = CultureInfo.GetCultures(CultureTypes.SpecificCultures).ToList();

            using (_connection = new SqlConnection(_connectionString))
            {
                using (_sqlDataContext = new ConfigurationSqlDataContext(_connection))
                {
                    serviceinfo = (from p in _sqlDataContext.p4crm_portalBases
                                   where p.p4crm_domainurl.Equals(URL) && p.statecode.Equals(0) && p.statuscode.Equals(1)
                                   select new CrmServiceInformation
                                   {
                                       UserName = p.p4crm_configurationBase.p4crm_username,
                                       Password = p.p4crm_configurationBase.p4crm_password,
                                       Domain = p.p4crm_configurationBase.p4crm_domain == null ? String.Empty : p.p4crm_configurationBase.p4crm_domain,
                                       OrganizationUri = p.p4crm_configurationBase.p4crm_organizationalurl,
                                       DiscoveryUri = p.p4crm_configurationBase.p4crm_discoveryurl == null ? String.Empty : p.p4crm_configurationBase.p4crm_discoveryurl,
                                       Source = p.p4crm_configurationBase.p4crm_isifd.Value == true ? "ifd" : "notifd",
                                       PortalId = p.p4crm_portalId.ToString(),
                                       ThemeType = p.p4crm_theme.Value == null ? "1" : p.p4crm_theme.Value.ToString(),
                                       LangId = p.p4crm_language.Value.ToString(),
                                       ConfigurationId = p.p4crm_configurationid.Value.ToString(),
                                       CrmType = p.p4crm_configurationBase.p4crm_crmtype.ToString(),
                                       IsOffice365 = p.p4crm_configurationBase.p4crm_isoffice365.ToString(),
                                       OrganizationName = p.p4crm_configurationBase.p4crm_organizationname,
                                       Region = p.p4crm_configurationBase.p4crm_region.ToString(),
                                       UseSSL = p.p4crm_configurationBase.p4crm_usessl.ToString(),
                                       PortalBaseLanguage = p.p4crm_portalbaselanguage,
                                       PortalMainLanguageCode = p.p4crm_mainlanguage,
                                       PortalEmailField = p.p4crm_portalemaillogicalname,
                                       PortalLogoURL = p.p4crm_portallogourl
                                   }).FirstOrDefault<CrmServiceInformation>();
                    //not return null
                    if (serviceinfo == null)
                    {
                        serviceinfo = new CrmServiceInformation();
                    }

                    else if (serviceinfo.PortalBaseLanguage != null)
                    {
                        serviceinfo.PortalBaseLanguage = CultureInfo.GetCultures(CultureTypes.SpecificCultures).ToList().Where(z => z.LCID.Equals(Convert.ToInt32(serviceinfo.PortalBaseLanguage))).FirstOrDefault().Name.ToString();
                    }
                }
            }
            return serviceinfo;
        }

        public CrmServiceInformation GetCrmConnectionInformationWithConfigurationId(String ConfigurationId)
        {
            CrmServiceInformation serviceinfo = new CrmServiceInformation();
            var CultureList = CultureInfo.GetCultures(CultureTypes.SpecificCultures).ToList();

            using (_connection = new SqlConnection(_connectionString))
            {
                using (_sqlDataContext = new ConfigurationSqlDataContext(_connection))
                {
                    serviceinfo = (from p in _sqlDataContext.p4crm_configurationBases
                                   where p.p4crm_configurationId.Equals(new Guid(ConfigurationId)) && p.statecode.Equals(0) && p.statuscode.Equals(1)
                                   select new CrmServiceInformation
                                   {
                                       UserName = p.p4crm_username,
                                       Password = p.p4crm_password,
                                       Domain = p.p4crm_domain == null ? String.Empty : p.p4crm_domain,
                                       OrganizationUri = p.p4crm_organizationalurl,
                                       DiscoveryUri = p.p4crm_discoveryurl == null ? String.Empty : p.p4crm_discoveryurl,
                                       Source = p.p4crm_isifd.Value == true ? "ifd" : "notifd",
                                       PortalId = p.ToString(),
                                       ConfigurationId = p.p4crm_configurationId.ToString(),
                                       CrmType = p.p4crm_crmtype == null ? "onpremise" :
                                       p.p4crm_crmtype.Value == 1 ? "onpremise" :
                                       p.p4crm_crmtype.Value == 2 ? "online" : "onpremise",

                                   }).FirstOrDefault<CrmServiceInformation>();
                    //not return null
                    if (serviceinfo == null)
                    {
                        serviceinfo = new CrmServiceInformation();
                    }
                }
            }
            return serviceinfo;
        }

        public String GetDynamicScriptForEntity(String Id)
        {
            String ReturnScripts = String.Empty;

            using (_connection = new SqlConnection(_connectionString))
            {
                using (_sqlDataContext = new ConfigurationSqlDataContext(_connection))
                {
                    ReturnScripts = (from p in _sqlDataContext.p4crm_portalBases
                                     where p.p4crm_portalId.Equals(Id) && p.statecode.Equals(0) && p.statuscode.Equals(1)
                                     select p.p4crm_customscript).FirstOrDefault();

                }

            }
            return ReturnScripts;
        }

        public LoginInformation GetLoginInfo(CrmServiceInformation serviceinfo)
        {
            LoginInformation LoginInfo = new LoginInformation();

            using (_connection = new SqlConnection(_connectionString))
            {
                using (_sqlDataContext = new ConfigurationSqlDataContext(_connection))
                {
                    LoginInfo = (from p in _sqlDataContext.p4crm_portalBases
                                 where p.p4crm_portalId.Equals(serviceinfo.PortalId) && p.statecode.Equals(0) && p.statuscode.Equals(1)
                                 select new LoginInformation
                                 {
                                     EntityName = p.p4crm_entitylogicalname,
                                     UserNameField = p.p4crm_userlogicalname,
                                     PasswordField = p.p4crm_passwordlogicalname
                                 }).FirstOrDefault();

                }
            }
            return LoginInfo;
        }

        public Page GetNavigationPage(String NavigationId)
        {
            Page Page = new Page();

            using (_connection = new SqlConnection(_connectionString))
            {
                using (_sqlDataContext = new ConfigurationSqlDataContext(_connection))
                {
                    Guid NewNavId = Guid.Empty;
                    Guid.TryParse(NavigationId, out NewNavId);

                    if (NewNavId != Guid.Empty)
                    {
                        Page = (from p in _sqlDataContext.p4crm_navigationBases
                                where p.p4crm_navigationId.Equals(NavigationId) && p.statecode.Equals(0) && p.statuscode.Equals(1)
                                select new Page
                                {
                                    FormLayoutXml = p.p4crm_pageBase.p4crm_layoutxml,
                                    Scripts = p.p4crm_pageBase.p4crm_customscript,
                                    UseCache = p.p4crm_pageBase.p4crm_usercache == null
                                              ? "false" :
                                              p.p4crm_pageBase.p4crm_usercache.Value == false ? "false" : "true",
                                    IsHeaderZoneActive = p.p4crm_pageBase.p4crm_isheaderzoneactive == null ? "false" :
                                                         p.p4crm_pageBase.p4crm_isheaderzoneactive.Value == false ? "false" :  "true",

                                    MainZoneWidth = p.p4crm_pageBase.p4crm_mainzonewidth == null ? "8" : ChangeZoneWidth(p.p4crm_pageBase.p4crm_mainzonewidth.Value),
                                    IsLeftZoneActive = p.p4crm_pageBase.p4crm_isleftzoneactive == null ? "false" :
                                                       p.p4crm_pageBase.p4crm_isleftzoneactive.Value == false ? "false" : "true",

                                    LeftZoneWidth = p.p4crm_pageBase.p4crm_leftzonewidth == null ? "8" : ChangeZoneWidth(p.p4crm_pageBase.p4crm_leftzonewidth.Value),
                                    IsRightZoneActive = p.p4crm_pageBase.p4crm_isrightzoneactive == null ? "false" :
                                                        p.p4crm_pageBase.p4crm_isrightzoneactive.Value == false ? "false" : "true",

                                    RightZoneWidth = p.p4crm_pageBase.p4crm_rightzonewidth == null ? "8" : ChangeZoneWidth(p.p4crm_pageBase.p4crm_rightzonewidth.Value),
                                    IsFooterZoneActive = p.p4crm_pageBase.p4crm_isfooterzoneactive == null ? "false" :
                                                         p.p4crm_pageBase.p4crm_isfooterzoneactive.Value == false ? "false" : "true",
                                }).FirstOrDefault();

                    }
                    else
                    {
                        Page = (from p in _sqlDataContext.p4crm_navigationBases
                                where p.p4crm_uniqueid.Equals(HttpUtility.UrlDecode(NavigationId.Split('-').ElementAt(NavigationId.Split('-').Length - 1)))
                                      && p.statecode.Equals(0) && p.statuscode.Equals(1)
                                select new Page
                                {
                                    FormLayoutXml = p.p4crm_pageBase.p4crm_layoutxml,
                                    Scripts = p.p4crm_pageBase.p4crm_customscript,
                                    IsHeaderZoneActive = p.p4crm_pageBase.p4crm_isheaderzoneactive == null ? "false" :
                                                         p.p4crm_pageBase.p4crm_isheaderzoneactive.Value == false ? "false" : "true",

                                    MainZoneWidth = p.p4crm_pageBase.p4crm_mainzonewidth == null ? "8" : ChangeZoneWidth(p.p4crm_pageBase.p4crm_mainzonewidth.Value),
                                    IsLeftZoneActive = p.p4crm_pageBase.p4crm_isleftzoneactive == null ? "false" :
                                                       p.p4crm_pageBase.p4crm_isleftzoneactive.Value == false ? "false" : "true",

                                    LeftZoneWidth = p.p4crm_pageBase.p4crm_leftzonewidth == null ? "8" : ChangeZoneWidth(p.p4crm_pageBase.p4crm_leftzonewidth.Value),
                                    IsRightZoneActive = p.p4crm_pageBase.p4crm_isrightzoneactive == null ? "false" :
                                                        p.p4crm_pageBase.p4crm_isrightzoneactive.Value == false ? "false" : "true",

                                    RightZoneWidth = p.p4crm_pageBase.p4crm_rightzonewidth == null ? "8" : ChangeZoneWidth(p.p4crm_pageBase.p4crm_rightzonewidth.Value),
                                    IsFooterZoneActive = p.p4crm_pageBase.p4crm_isfooterzoneactive == null ? "false" :
                                                         p.p4crm_pageBase.p4crm_isfooterzoneactive.Value == false ? "false" : "true",
                                }).FirstOrDefault();

                    }

                }
            }
            return Page;
        }

        public Page GetPage(String PageId)
        {
            Page Page = new Page();

            using (_connection = new SqlConnection(_connectionString))
            {
                using (_sqlDataContext = new ConfigurationSqlDataContext(_connection))
                {

                    Page = (from p in _sqlDataContext.p4crm_pageBases
                            where p.p4crm_pagename.Equals(PageId)
                               && p.statecode.Equals(0) && p.statuscode.Equals(1)
                            select new Page
                            {
                                FormLayoutXml = p.p4crm_layoutxml,
                                Scripts = p.p4crm_customscript,
                                UseCache = p.p4crm_usercache == null
                                              ? "false" :
                                              p.p4crm_usercache.Value == false ? "false" : "true",
                                IsHeaderZoneActive = p.p4crm_isheaderzoneactive == null ? "false" :
                                                         p.p4crm_isheaderzoneactive.Value == false ? "false" : "true",

                                MainZoneWidth = p.p4crm_mainzonewidth == null ? "8" : ChangeZoneWidth(p.p4crm_mainzonewidth.Value),
                                IsLeftZoneActive = p.p4crm_isleftzoneactive == null ? "false" :
                                                   p.p4crm_isleftzoneactive.Value == false ? "false" : "true",

                                LeftZoneWidth = p.p4crm_leftzonewidth == null ? "8" : ChangeZoneWidth(p.p4crm_leftzonewidth.Value),
                                IsRightZoneActive = p.p4crm_isrightzoneactive == null ? "false" :
                                                    p.p4crm_isrightzoneactive.Value == false ? "false" : "true",

                                RightZoneWidth = p.p4crm_rightzonewidth == null ? "8" : ChangeZoneWidth(p.p4crm_rightzonewidth.Value),
                                IsFooterZoneActive = p.p4crm_isfooterzoneactive == null ? "false" :
                                                     p.p4crm_isfooterzoneactive.Value == false ? "false" : "true",
                            }).FirstOrDefault();
                }
            }
            return Page;
        }

        public List<WidgetProperties> GetPageWidgetEntities(List<WidgetParameters> WidgetParameters)
        {
            List<WidgetProperties> WidgetProperties = new List<WidgetProperties>();

            using (_connection = new SqlConnection(_connectionString))
            {
                using (_sqlDataContext = new ConfigurationSqlDataContext(_connection))
                {
                    var arr = WidgetParameters.Select(p => p.PageWidgetId).ToArray();

                    WidgetProperties = (from p in _sqlDataContext.p4crm_pagewidgetBases
                                        where arr.Contains(p.p4crm_pagewidgetname) && p.statecode.Equals(0) && p.statuscode.Equals(1)
                                        select new WidgetProperties
                                        {
                                            PageWidgetName = p.p4crm_pagewidgetname,
                                            WidgetId = p.p4crm_widgetBase.p4crm_widgetId.ToString(),
                                            WidgetType = p.p4crm_widgetBase.p4crm_widgettype.Value,
                                            CustomFilter = p.p4crm_widgetBase.p4crm_customfilter,
                                            UseCustomFilter = p.p4crm_widgetBase.p4crm_usecustomfilter == null ? false : p.p4crm_widgetBase.p4crm_usecustomfilter.Value,
                                            ViewId = p.p4crm_widgetBase.p4crm_viewid,
                                            UrlAttributeLogicalname = p.p4crm_widgetBase.p4crm_urlattributelogicalname,
                                            WidgetUniqueId = p.p4crm_widgetBase.p4crm_widgetuniqueid,
                                            WidgetName = p.p4crm_widgetBase.p4crm_widgetname,
                                            CalculatedWidgetType = p.p4crm_widgetBase.p4crm_calculatedfieldtype.Value,
                                            EntityLogicalName = p.p4crm_widgetBase.p4crm_entitynamelogicalname,
                                            CalculatedFieldLogicalName = p.p4crm_widgetBase.p4crm_calculatedfieldlogicalname,
                                            CalendarLogicalName = p.p4crm_widgetBase.p4crm_calendarlogicalname,
                                            CalendarStartDateLogicalName = p.p4crm_widgetBase.p4crm_calendarstartdatelogicalname,
                                            CalendarEndDateLogicalName = p.p4crm_widgetBase.p4crm_calendarenddatelogicalname,
                                            FormId = p.p4crm_widgetBase.p4crm_formid,
                                            UpdateFormFetchXML = p.p4crm_widgetBase.p4crm_formupdatefetchxml,
                                            HTMLSource = p.p4crm_widgetBase.p4crm_htmlsource,
                                            SignatureEnabled = p.p4crm_widgetBase.p4crm_signatureenabled.Value,
                                            ChartType = p.p4crm_widgetBase.p4crm_charttypecode == null ? 0 : p.p4crm_widgetBase.p4crm_charttypecode.Value,
                                            SeriesLogicalName = p.p4crm_widgetBase.p4crm_serieslogicalname,
                                            HorizontalLogicalName = p.p4crm_widgetBase.p4crm_horizontallogicalname,
                                            Series0LogicalName = p.p4crm_widgetBase.p4crm_series0_logicalname,
                                            Series1LogicalName = p.p4crm_widgetBase.p4crm_series1_logicalname,
                                            Series2LogicalName = p.p4crm_widgetBase.p4crm_series2_logicalname,
                                            Series3LogicalName = p.p4crm_widgetBase.p4crm_series3_logicalname,
                                            Series4LogicalName = p.p4crm_widgetBase.p4crm_series4_logicalname,
                                            Series0Aggregate = p.p4crm_widgetBase.p4crm_series0aggregate,
                                            Series1Aggregate = p.p4crm_widgetBase.p4crm_series1aggregate,
                                            Series2Aggregate = p.p4crm_widgetBase.p4crm_series2aggregate,
                                            Series3Aggregate = p.p4crm_widgetBase.p4crm_series3aggregate,
                                            Series4Aggregate = p.p4crm_widgetBase.p4crm_series4aggregate,
                                            LegendColor0 = p.p4crm_widgetBase.p4crm_legendcolor0,
                                            LegendColor1 = p.p4crm_widgetBase.p4crm_legendcolor1,
                                            LegendColor2 = p.p4crm_widgetBase.p4crm_legendcolor2,
                                            LegendColor3 = p.p4crm_widgetBase.p4crm_legendcolor3,
                                            LegendColor4 = p.p4crm_widgetBase.p4crm_legendcolor4,
                                            Field1 = p.p4crm_widgetBase.p4crm_fieldwidgetfield1,
                                            Field2 = p.p4crm_widgetBase.p4crm_fieldwidgetfield2,
                                            Field3 = p.p4crm_widgetBase.p4crm_fieldwidgetfield3,
                                            NotificationAttributeName = p.p4crm_widgetBase.p4crm_notificationattributename
                                        }).ToList();

                }
            }
            return WidgetProperties;
        }

        public WidgetProperties GetPageWidgetWithPageWidgetName(String PageWidgetId)
        {
            WidgetProperties WidgetProperties = new WidgetProperties();

            using (_connection = new SqlConnection(_connectionString))
            {
                using (_sqlDataContext = new ConfigurationSqlDataContext(_connection))
                {

                    WidgetProperties = (from p in _sqlDataContext.p4crm_pagewidgetBases
                                        where p.p4crm_pagewidgetname.Equals(PageWidgetId) && p.statecode.Equals(0) && p.statuscode.Equals(1)
                                        select new WidgetProperties
                                        {
                                            PageWidgetName = p.p4crm_pagewidgetname,
                                            WidgetId = p.p4crm_widgetBase.p4crm_widgetId.ToString(),
                                            WidgetType = p.p4crm_widgetBase.p4crm_widgettype.Value,
                                            CustomFilter = p.p4crm_widgetBase.p4crm_customfilter,
                                            ViewId = p.p4crm_widgetBase.p4crm_viewid,
                                            WidgetUniqueId = p.p4crm_widgetBase.p4crm_widgetuniqueid,
                                            WidgetName = p.p4crm_widgetBase.p4crm_widgetname,
                                            CalculatedWidgetType = p.p4crm_widgetBase.p4crm_calculatedfieldtype.Value,
                                            EntityLogicalName = p.p4crm_widgetBase.p4crm_entitynamelogicalname,
                                            CalculatedFieldLogicalName = p.p4crm_widgetBase.p4crm_calculatedfieldlogicalname,
                                            CalendarLogicalName = p.p4crm_widgetBase.p4crm_calendarlogicalname,
                                            CalendarStartDateLogicalName = p.p4crm_widgetBase.p4crm_calendarstartdatelogicalname,
                                            CalendarEndDateLogicalName = p.p4crm_widgetBase.p4crm_calendarenddatelogicalname,
                                            FormId = p.p4crm_widgetBase.p4crm_formid,
                                            UpdateFormFetchXML = p.p4crm_widgetBase.p4crm_formupdatefetchxml,
                                            HTMLSource = p.p4crm_widgetBase.p4crm_htmlsource,
                                            SignatureEnabled = p.p4crm_widgetBase.p4crm_signatureenabled.Value,
                                            ChartType = p.p4crm_widgetBase.p4crm_charttypecode == null ? 0 : p.p4crm_widgetBase.p4crm_charttypecode.Value,
                                            SeriesLogicalName = p.p4crm_widgetBase.p4crm_serieslogicalname,
                                            HorizontalLogicalName = p.p4crm_widgetBase.p4crm_horizontallogicalname,
                                            Series0LogicalName = p.p4crm_widgetBase.p4crm_series0_logicalname,
                                            Series1LogicalName = p.p4crm_widgetBase.p4crm_series1_logicalname,
                                            Series2LogicalName = p.p4crm_widgetBase.p4crm_series2_logicalname,
                                            Series3LogicalName = p.p4crm_widgetBase.p4crm_series3_logicalname,
                                            Series4LogicalName = p.p4crm_widgetBase.p4crm_series4_logicalname,
                                            Series0Aggregate = p.p4crm_widgetBase.p4crm_series0aggregate,
                                            Series1Aggregate = p.p4crm_widgetBase.p4crm_series1aggregate,
                                            Series2Aggregate = p.p4crm_widgetBase.p4crm_series2aggregate,
                                            Series3Aggregate = p.p4crm_widgetBase.p4crm_series3aggregate,
                                            Series4Aggregate = p.p4crm_widgetBase.p4crm_series4aggregate,
                                            LegendColor0 = p.p4crm_widgetBase.p4crm_legendcolor0,
                                            LegendColor1 = p.p4crm_widgetBase.p4crm_legendcolor1,
                                            LegendColor2 = p.p4crm_widgetBase.p4crm_legendcolor2,
                                            LegendColor3 = p.p4crm_widgetBase.p4crm_legendcolor3,
                                            LegendColor4 = p.p4crm_widgetBase.p4crm_legendcolor4,
                                        }).FirstOrDefault();

                }
            }
            return WidgetProperties;
        }

        public WidgetProperties GetWidgetWithWidgetId(String WidgetId)
        {
            WidgetProperties WidgetProperties = new WidgetProperties();

            using (_connection = new SqlConnection(_connectionString))
            {
                using (_sqlDataContext = new ConfigurationSqlDataContext(_connection))
                {
                    WidgetProperties = (from p in _sqlDataContext.p4crm_widgetBases
                                        where p.p4crm_widgetId.Equals(WidgetId) && p.statecode.Equals(0) && p.statuscode.Equals(1)
                                        select new WidgetProperties
                                        {
                                            WidgetId = p.p4crm_widgetId.ToString(),
                                            WidgetType = p.p4crm_widgettype.Value,
                                            CustomFilter = p.p4crm_customfilter,
                                            ViewId = p.p4crm_viewid,
                                            WidgetUniqueId = p.p4crm_widgetuniqueid,
                                            WidgetName = p.p4crm_widgetname,
                                            CalculatedWidgetType = p.p4crm_calculatedfieldtype.Value,
                                            EntityLogicalName = p.p4crm_entitynamelogicalname,
                                            CalculatedFieldLogicalName = p.p4crm_calculatedfieldlogicalname,
                                            CalendarLogicalName = p.p4crm_calendarlogicalname,
                                            CalendarStartDateLogicalName = p.p4crm_calendarstartdatelogicalname,
                                            CalendarEndDateLogicalName = p.p4crm_calendarenddatelogicalname,
                                            FormId = p.p4crm_formid,
                                            UpdateFormFetchXML = p.p4crm_formupdatefetchxml,
                                            HTMLSource = p.p4crm_htmlsource,
                                            SignatureEnabled = p.p4crm_signatureenabled.Value,
                                            UseCustomFilter = p.p4crm_usecustomfilter.Value,
                                        }).FirstOrDefault();

                }
            }
            return WidgetProperties;
        }

        public WidgetProperties GetWidgetHTMLContent(String WidgetId)
        {
            WidgetProperties WidgetProperties = new WidgetProperties();

            using (_connection = new SqlConnection(_connectionString))
            {
                using (_sqlDataContext = new ConfigurationSqlDataContext(_connection))
                {
                    WidgetProperties = (from p in _sqlDataContext.p4crm_widgetBases
                                        where p.p4crm_widgetId.Equals(WidgetId) && p.statecode.Equals(0) && p.statuscode.Equals(1)
                                        select new WidgetProperties
                                        {
                                            EntityLogicalName = p.p4crm_entitynamelogicalname,
                                            HTMLSource = p.p4crm_htmlsource,
                                        }).FirstOrDefault();

                }
            }
            return WidgetProperties;
        }

        public PortalMembers GetPortalOwnerFields(String PortalId)
        {
            PortalMembers PortalMembers = new PortalMembers();

            using (_connection = new SqlConnection(_connectionString))
            {
                using (_sqlDataContext = new ConfigurationSqlDataContext(_connection))
                {
                    PortalMembers = (from p in _sqlDataContext.p4crm_portalBases
                                     where p.p4crm_portalId.Equals(PortalId) && p.statecode.Equals(0) && p.statuscode.Equals(1)
                                     select new PortalMembers
                                     {
                                         CreatedBy = p.p4crm_portalcreatedby,
                                         ModifiedBy = p.p4crm_portalmodifiedby,
                                     }).FirstOrDefault();

                }
            }
            return PortalMembers;
        }

        public WidgetProperties GetProfileWidget(String PortalId)
        {
            WidgetProperties WidgetProperties = new WidgetProperties();

            using (_connection = new SqlConnection(_connectionString))
            {
                using (_sqlDataContext = new ConfigurationSqlDataContext(_connection))
                {
                    WidgetProperties = (from p in _sqlDataContext.p4crm_portalBases
                                        where p.p4crm_portalId.Equals(PortalId) && p.statecode.Equals(0) && p.statuscode.Equals(1)
                                        select new WidgetProperties
                                        {
                                            Profile = p.p4crm_widgetBase.p4crm_formupdatefetchxml,
                                            FormId = p.p4crm_widgetBase.p4crm_formid,
                                            WidgetId = p.p4crm_widgetBase.p4crm_widgetId.ToString(),
                                            WidgetUniqueId = p.p4crm_widgetBase.p4crm_widgetuniqueid
                                        }).FirstOrDefault();

                }
            }
            return WidgetProperties;
        }

        public List<UserRoles> GetUserRoles(String PortalId)
        {
            List<UserRoles> roles = new List<UserRoles>();

            using (_connection = new SqlConnection(_connectionString))
            {
                using (_sqlDataContext = new ConfigurationSqlDataContext(_connection))
                {
                    roles = (from p in _sqlDataContext.p4crm_portalroleBases
                             select new UserRoles
                                   {
                                       PortalRole_PortalId = p.p4crm_portalid.Value.ToString(),
                                       PortalRoleName = p.p4crm_portalrolename,
                                       PortalRoleId = p.p4crm_portalroleId.ToString(),

                                   })
                                   .Where(p => p.PortalRole_PortalId.Equals(PortalId))
                                   .ToList();

                }
            }
            return roles;
        }

        public List<UserRoleConditions> GetUserRolesConditions(String RoleId)
        {
            List<UserRoleConditions> roles = new List<UserRoleConditions>();

            using (_connection = new SqlConnection(_connectionString))
            {
                using (_sqlDataContext = new ConfigurationSqlDataContext(_connection))
                {
                    roles = (from p in _sqlDataContext.p4crm_portalroleconditionBases
                             where p.p4crm_portalroleid.Value.Equals(new Guid(RoleId)) && p.statecode.Equals(0) && p.statuscode.Equals(1)
                             select new UserRoleConditions
                             {
                                 FilterType = p.p4crm_filtertype.Value,
                                 AttributeLogicalName = p.p4crm_attributelogicalname,
                                 AttributeValue = p.p4crm_attributevalue
                             }).ToList();


                }
            }
            return roles;
        }

        public List<Navigation> GetExternalNavigation(String PortalId)
        {
            List<Navigation> navigations = new List<Navigation>();

            using (_connection = new SqlConnection(_connectionString))
            {
                using (_sqlDataContext = new ConfigurationSqlDataContext(_connection))
                {
                    navigations = (from p in _sqlDataContext.p4crm_navigationBases
                                   where p.p4crm_isexternal.Value.Equals(true) && p.p4crm_portalid.Value.Equals(new Guid(PortalId)) && p.statecode.Equals(0) && p.statuscode.Equals(1)
                                   select new Navigation
                                     {
                                         PageId = p.p4crm_pageid != null ? p.p4crm_pageid.Value.ToString() : String.Empty,
                                         Name = p.p4crm_navigationname,
                                         NavigationId = p.p4crm_navigationId.ToString(),
                                         Order = p.p4crm_order.Value.ToString(),
                                         ParentNavigationName = p.p4crm_parentnavigationid != null ?
                                                                (from pparent in _sqlDataContext.p4crm_navigationBases where pparent.p4crm_navigationId.Equals(p.p4crm_parentnavigationid.Value) select pparent.p4crm_navigationname).FirstOrDefault()
                                                                : String.Empty,
                                         ParentNavigationId = p.p4crm_parentnavigationid != null ? p.p4crm_parentnavigationid.Value.ToString() : String.Empty,
                                         ExternalLink = p.p4crm_externallink != null ? p.p4crm_externallink.ToString() : String.Empty,
                                         UrlName = p.p4crm_urlname,
                                         UniqueId = p.p4crm_uniqueid,
                                     }).ToList();

                }
            }
            return navigations;
        }

        public String[] GetRelationOfNavigationAndPortalRole(String[] RoleIds)
        {
            String[] Arr;

            using (_connection = new SqlConnection(_connectionString))
            {
                using (_sqlDataContext = new ConfigurationSqlDataContext(_connection))
                {

                    Arr = (from p in _sqlDataContext.p4crm_p4crm_portalrole_p4crm_navigationBases
                           where RoleIds.Contains(p.p4crm_portalroleid.ToString())
                           select p.p4crm_navigationid.ToString()).ToArray();

                }
            }
            return Arr;
        }

        public List<Navigation> GetNavigationOfUser(String[] NavigationIds)
        {
            List<Navigation> navigations = new List<Navigation>();

            using (_connection = new SqlConnection(_connectionString))
            {
                using (_sqlDataContext = new ConfigurationSqlDataContext(_connection))
                {
                    navigations = (from p in _sqlDataContext.p4crm_navigationBases
                                   where p.p4crm_isexternal.Value.Equals(false) && NavigationIds.Contains(p.p4crm_navigationId.ToString()) && p.statecode.Equals(0) && p.statuscode.Equals(1)
                                   select new Navigation
                                   {
                                       PageId = p.p4crm_pageid != null ? p.p4crm_pageid.Value.ToString() : String.Empty,
                                       Name = p.p4crm_navigationname,
                                       NavigationId = p.p4crm_navigationId.ToString(),
                                       Order = p.p4crm_order.Value.ToString(),
                                       ParentNavigationName = p.p4crm_parentnavigationid != null ?
                                                              (from pparent in _sqlDataContext.p4crm_navigationBases where pparent.p4crm_navigationId.Equals(p.p4crm_parentnavigationid.Value) select pparent.p4crm_navigationname).FirstOrDefault()
                                                              : String.Empty,
                                       ParentNavigationId = p.p4crm_parentnavigationid != null ? p.p4crm_parentnavigationid.Value.ToString() : String.Empty,
                                       ExternalLink = p.p4crm_externallink != null ? p.p4crm_externallink.ToString() : String.Empty,
                                       UrlName = p.p4crm_urlname,
                                       UniqueId = p.p4crm_uniqueid,
                                   }).ToList();

                }
            }
            return navigations;
        }

        public List<Language> GetPortalLanguages(String PortalId)
        {
            List<Language> Language = new List<Language>();

            using (_connection = new SqlConnection(_connectionString))
            {
                using (_sqlDataContext = new ConfigurationSqlDataContext(_connection))
                {
                    if (PortalId != null)
                    {
                        var configurations = (from p in _sqlDataContext.p4crm_p4crm_portal_p4crm_configurationBases
                                              where p.p4crm_portalid.Equals(new Guid(PortalId))
                                              select p).ToList();

                        foreach (var item in configurations)
                        {

                            DestinationService Destination = new DestinationService(PortalId,
                                                                                    item.p4crm_configurationBase.p4crm_username,
                                                                                    item.p4crm_configurationBase.p4crm_password,
                                                                                    item.p4crm_configurationBase.p4crm_domain,
                                                                                    item.p4crm_configurationBase.p4crm_organizationalurl,
                                                                                    item.p4crm_configurationBase.p4crm_discoveryurl,
                                                                                    String.Empty,
                                                                                    item.p4crm_configurationBase.p4crm_crmtype.ToString(),
                                                                                    item.p4crm_configurationBase.p4crm_organizationname,
                                                                                    item.p4crm_configurationBase.p4crm_region.ToString(),
                                                                                    item.p4crm_configurationBase.p4crm_usessl.ToString(),
                                                                                    item.p4crm_configurationBase.p4crm_isoffice365.ToString(),
                                                                                    item.p4crm_configurationid.ToString(),
                                                                                    "false");
                            IOrganizationService Service = Destination.IOrganizationService;
                            RetrieveUserSettingsSystemUserResponse Response = CreateUserSettings(Service, PortalId,
                                                                               new CrmServiceInformation
                                                                               {
                                                                                   ConfigurationId = item.p4crm_configurationid.ToString(),
                                                                                   CrmType = item.p4crm_configurationBase.p4crm_crmtype == null ? "onpremise" :
                                                                                             item.p4crm_configurationBase.p4crm_crmtype.Value == 1 ? "onpremise" :
                                                                                             item.p4crm_configurationBase.p4crm_crmtype.Value == 2 ? "online" : "onpremise",
                                                                                   Source = item.p4crm_configurationBase.p4crm_isifd.Value == true ? "ifd" : "notifd",
                                                                                   UserName = item.p4crm_configurationBase.p4crm_username,
                                                                                   Password = item.p4crm_configurationBase.p4crm_password,
                                                                                   Domain = item.p4crm_configurationBase.p4crm_domain == null ? String.Empty : item.p4crm_configurationBase.p4crm_domain,
                                                                                   OrganizationUri = item.p4crm_configurationBase.p4crm_organizationalurl,
                                                                                   DiscoveryUri = item.p4crm_configurationBase.p4crm_discoveryurl == null ? String.Empty : item.p4crm_configurationBase.p4crm_discoveryurl,
                                                                                   PortalId = item.p4crm_portalid.ToString()

                                                                               });
                            var nativename = CultureInfo.GetCultures(CultureTypes.SpecificCultures).ToList().Where(z => z.LCID.Equals(Convert.ToInt32(Response.Entity["uilanguageid"].ToString()))).FirstOrDefault().NativeName.ToString();
                            Language L = new Language()
                            {
                                Label = nativename.Substring(0, nativename.IndexOf("(")).Replace(" ", ""),
                                LangId = Response.Entity["uilanguageid"].ToString(),
                                BaseLangId = (from por in _sqlDataContext.p4crm_portalBases where por.p4crm_portalId.Equals(new Guid(PortalId)) select por.p4crm_portalbaselanguage).FirstOrDefault(),
                                IsMain = (from por in _sqlDataContext.p4crm_portalBases
                                          where por.p4crm_portalId.Equals(new Guid(PortalId))
                                          select por.p4crm_configurationBase.p4crm_configurationId)
                                                      .FirstOrDefault() == item.p4crm_configurationid
                                                      ? "1"
                                                      : "0",
                                ConfigurationId = item.p4crm_configurationid.ToString()

                            };
                            Language.Add(L);

                        }
                        #region Add Base Language
                        var baseportal = (from por in _sqlDataContext.p4crm_portalBases where por.p4crm_portalId.Equals(new Guid(PortalId)) select por).FirstOrDefault();
                        var mainnativename = CultureInfo.GetCultures(CultureTypes.SpecificCultures).ToList().Where(z => z.LCID.Equals(Convert.ToInt32(baseportal.p4crm_mainlanguage))).FirstOrDefault().NativeName.ToString();
                        Language BaseLanguage = new Language();
                        BaseLanguage.Label = mainnativename.Substring(0, mainnativename.IndexOf("(")).Replace(" ", "");
                        BaseLanguage.ConfigurationId = baseportal.p4crm_configurationid.Value.ToString();
                        BaseLanguage.LangId = baseportal.p4crm_mainlanguage;
                        BaseLanguage.BaseLangId = baseportal.p4crm_portalbaselanguage;
                        BaseLanguage.IsMain = "1";
                        BaseLanguage.NativeName = CultureInfo.GetCultures(CultureTypes.SpecificCultures).ToList().Where(z => z.LCID.Equals(Convert.ToInt32(baseportal.p4crm_mainlanguage))).FirstOrDefault().Name.ToString();
                        BaseLanguage.BaseNativeName = CultureInfo.GetCultures(CultureTypes.SpecificCultures).ToList().Where(z => z.LCID.Equals(Convert.ToInt32(baseportal.p4crm_portalbaselanguage))).FirstOrDefault().Name.ToString();
                        Language.Add(BaseLanguage);
                        #endregion

                        Language.ForEach(c => c.NativeName = CultureInfo.GetCultures(CultureTypes.SpecificCultures).ToList().Where(z => z.LCID.Equals(Convert.ToInt32(c.LangId))).FirstOrDefault().Name.ToString());
                        Language.ForEach(c => c.BaseNativeName = CultureInfo.GetCultures(CultureTypes.SpecificCultures).ToList().Where(z => z.LCID.Equals(Convert.ToInt32(c.BaseLangId))).FirstOrDefault().Name.ToString());
                    }
                }
            }
            return Language;
        }

        public Boolean CheckPortalIsMultiLanguage(String PortalId)
        {
            Boolean returnVal = false;

            using (_connection = new SqlConnection(_connectionString))
            {
                using (_sqlDataContext = new ConfigurationSqlDataContext(_connection))
                {
                    var configurations = (from p in _sqlDataContext.p4crm_p4crm_portal_p4crm_configurationBases
                                          where p.p4crm_portalid.Equals(new Guid(PortalId))
                                          select p).ToList();
                    if (configurations.Count > 0)
                    {
                        returnVal = true;
                    }
                }

            }
            return returnVal;
        }
        public ResetPassword GetPortalInformationForPasswordReset(String PortalId)
        {
            ResetPassword ResetPassword = new ResetPassword();

            using (_connection = new SqlConnection(_connectionString))
            {
                using (_sqlDataContext = new ConfigurationSqlDataContext(_connection))
                {
                    ResetPassword = (from p in _sqlDataContext.p4crm_portalBases
                                     where p.p4crm_portalId.Equals(new Guid(PortalId)) && p.statecode.Equals(0) && p.statuscode.Equals(1)
                                     select new ResetPassword
                                     {
                                         EntityName = p.p4crm_entitylogicalname,
                                         UserName = p.p4crm_userlogicalname,
                                         PasswordName = p.p4crm_passwordlogicalname,
                                         EmailValue = p.p4crm_portalemaillogicalname,
                                         EmailAlias = p.p4crm_emailalias,
                                         AdminAddress = p.emailaddress
                                     }).FirstOrDefault<ResetPassword>();
                }
            }
            return ResetPassword;
        }

        public String GetMetadataFromAzureSQL(String metadataID)
        {
            string metadata = string.Empty;

            using (_connection = new SqlConnection(_connectionString))
            {
                // Cache type 1 = metadata cache
                using (_sqlDataContext = new ConfigurationSqlDataContext(_connection))
                {
                    metadata = (from z in _sqlDataContext.p4crm_cacheBases
                                where z.ID.Equals(metadataID) && z.CacheType.Equals(1)
                                select z).FirstOrDefault().Data;
                }
            }

            return metadata;
        }

        public String GetUserSettingFromAzureSQL(String userSettingID)
        {
            string userSetting = string.Empty;

            using (_connection = new SqlConnection(_connectionString))
            {
                // Cache type 1 = metadata cache
                using (_sqlDataContext = new ConfigurationSqlDataContext(_connection))
                {

                    var userSettingQuery = (from z in _sqlDataContext.p4crm_cacheBases
                                           where z.ID.Equals(userSettingID.ToLowerInvariant()) && z.CacheType.Equals(3)
                                           select z).FirstOrDefault();                   
                    userSetting = userSettingQuery.Data;
                }
            }

            return userSetting;
        }

        //public void CreateLog(String ConfigurationId, String EntityName, DateTime? CreateCacheTime, DateTime? RenewCacheTime, Boolean NewCache)
        //{
        //    using (_connection = new SqlConnection(_connectionString))
        //    {
        //        using (_sqlDataContext = new ConfigurationSqlDataContext(_connection))
        //        {
        //            if (NewCache == true)
        //            {
        //                Log L = new Log
        //                {
        //                    EntityName = EntityName,
        //                    InsertCacheTime = CreateCacheTime,
        //                    CacheorNew = "new",
        //                    ConfigurationId = ConfigurationId,
        //                    LogId = Guid.NewGuid()
        //                };
        //                _sqlDataContext.Logs.InsertOnSubmit(L);
        //                _sqlDataContext.SubmitChanges();
        //            }
        //            else
        //            {
        //                Log L = new Log
        //                {
        //                    EntityName = EntityName,
        //                    RenewCacheTime = RenewCacheTime,
        //                    CacheorNew = "existing",
        //                    ConfigurationId = ConfigurationId,
        //                    LogId = Guid.NewGuid()
        //                };
        //                _sqlDataContext.Logs.InsertOnSubmit(L);
        //                _sqlDataContext.SubmitChanges();
        //            }

        //        }

        //    }
        //}

        private RetrieveUserSettingsSystemUserResponse CreateUserSettings(IOrganizationService service, String PortalId, CrmServiceInformation serviceinfo)
        {
            SqlAzureConnection Azure = new SqlAzureConnection();

            RetrieveUserSettingsSystemUserResponse returnResponse = null;

            string userSettingID = string.Format("{0}_{1}_usersettings", PortalId, serviceinfo.ConfigurationId);

            returnResponse = (RetrieveUserSettingsSystemUserResponse)Deserialize(Azure.GetUserSettingFromAzureSQL(userSettingID), typeof(RetrieveUserSettingsSystemUserResponse));

            return returnResponse;

        }

        private static String  ChangeZoneWidth(int value)
        {
            String ReturnValue = String.Empty;
            switch (value)
            {
                case 1:
                    ReturnValue = "16";
                    break;
                case 2:
                    ReturnValue = "25";
                    break;
                case 3:
                    ReturnValue = "33";
                    break;
                case 4:
                    ReturnValue = "50";
                    break;
                case 5:
                    ReturnValue = "66";
                    break;
                case 6:
                    ReturnValue = "75";
                    break;
                case 7:
                    ReturnValue = "84";
                    break;
                case 8:
                    ReturnValue = "100";
                    break;
                default:
                    ReturnValue = "100";
                    break;
            }
            return ReturnValue;
        }
        public static Object Deserialize(string xml, Type toType)
        {
            using (Stream stream = new MemoryStream())
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(xml);
                stream.Write(data, 0, data.Length);
                stream.Position = 0;
                DataContractSerializer deserializer = new DataContractSerializer(toType, null, int.MaxValue, false, false, null, new KnownTypesResolver());
                return deserializer.ReadObject(stream);
            }

        }

    }
}