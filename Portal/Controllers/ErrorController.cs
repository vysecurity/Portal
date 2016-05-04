using Newtonsoft.Json;
using Portal.Helper;
using Portal.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Portal.Controllers
{

    public class ErrorController : Controller
    {
        private String URL = System.Web.HttpContext.Current.Request.Url.Authority;
        private String LoggedUserGuidValue;
        private String MainLangId;
        private String PortalId;
        private LocalizationModel LanguageModel = new LocalizationModel();
        private List<LanguageModel> Languages = new List<LanguageModel>();
        ObjectCache cache = MemoryCache.Default;
        
        PortalService.CrmServiceInformation portalserviceinfo = new PortalService.CrmServiceInformation();
        private PortalService.PortalServiceClient client = null;

        public ErrorController()
        {
            client = new PortalService.PortalServiceClient(PortalHelper.GetBasicHttpBinding(), PortalHelper.GetEndPointAddress(URL));
            if (System.Web.HttpContext.Current.Request.Cookies[URL + "portalcookie"] != null)
            {
                portalserviceinfo = JsonConvert.DeserializeObject<PortalService.CrmServiceInformation>(System.Web.HttpContext.Current.Request.Cookies[URL + "portalcookie"].Values[0]);
                if (portalserviceinfo == null)
                {
                    RedirectToAction("Logout", "Dashboard");
                }
                else
                {
                    if (!System.Web.HttpContext.Current.Request.RequestContext.RouteData.Values.Values.Contains("ExternalErrorPage"))
                    {
                        if (System.Web.HttpContext.Current.Request.Cookies[URL + "usernameguidcookie"] != null)
                        {
                            LoggedUserGuidValue = System.Web.HttpContext.Current.Request.Cookies[URL + "usernameguidcookie"].Values[0].ToString();
                        }
                        PortalId = portalserviceinfo.PortalId;
                        System.Web.HttpContext.Current.Application[URL + "PortalIds"] = PortalId;
                        //Get Localization
                        Languages = PortalHelper.GetLanguages(PortalId, client);
                        //GET Portal Current LangId
                        MainLangId = System.Web.HttpContext.Current.Request.Cookies[URL + "languagecookie"].Values[0];
                        //Get Portal LocalizationXML due to CurretnLangId
                        LanguageModel = Portal.Helper.PortalHelper.ParsePortalLangXml(System.Web.HttpContext.Current.Server.MapPath("~/Helper/Localization.xml"), MainLangId);

                    }

                }
            }

        }
        [AllowAnonymous]
        public ActionResult Index(String ErrorMessage)
        {
            if (Request.Cookies[URL + "portalcookie"] != null)
            {
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
            }
            List<Menu> Menu = new List<Menu>();

            if (cache.Contains(PortalId + LoggedUserGuidValue + "menucache"))
            {
                String DecodedValues = Server.UrlDecode(cache[PortalId + LoggedUserGuidValue + "menucache"].ToString());

                Menu = JsonConvert.DeserializeObject<List<Menu>>(DecodedValues);
            }
            else
            {
                Menu = null;
            }

            String ParentScript = String.Empty;
            if(!String.IsNullOrEmpty(ParentScript))
                ParentScript = client.GetDynamicScriptForEntity(PortalId);

            return View(new GeneralModel()
            {
                MenuViewModel = new MenuModel()
                {
                    Picture = Session[URL + "Picture"] == null ? String.Empty : Session[URL + "Picture"].ToString(),
                    LoggedUser = HttpContext.User.Identity.Name,
                    Menus = Menu,
                    Scripts = ParentScript,
                    Languages = Languages,
                    ComingFrom = Request.Cookies[URL + "usernameguidcookie"] == null ? Enums.MenuSource.External.ToString() : Enums.MenuSource.Internal.ToString()
                },
                ExtraViewModel = new ExtraModel()
                {
                    ErrorMessage = ErrorMessage,
                    PortalLanguage = PortalHelper.GetNameOfLangId(MainLangId),
                    IsComingFromErrorPage = "1",

                },
                LocalizationModel = LanguageModel,

            });
        }

        public ActionResult NotFound(String LangId)
        {
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
            String ParentScript = client.GetDynamicScriptForEntity(PortalId);

            List<Menu> Menu = new List<Menu>();


            if (cache.Contains(PortalId + LoggedUserGuidValue + "menucache"))
            {
                String DecodedValues = Server.UrlDecode(cache[PortalId + LoggedUserGuidValue + "menucache"].ToString());

                Menu = JsonConvert.DeserializeObject<List<Menu>>(DecodedValues);
            }
            else
            {
                Menu = null;
            }

            return View(new GeneralModel()
            {
                MenuViewModel = new MenuModel()
                   {
                       Picture = Session[URL + "Picture"].ToString(),
                       LoggedUser = HttpContext.User.Identity.Name,
                       Menus = Menu,
                       Scripts = ParentScript,
                       Languages = Languages
                   },
                LocalizationModel = LanguageModel,
                ExtraViewModel = new ExtraModel()
                {
                    LoggedUserGuid = LoggedUserGuidValue,
                    PortalLanguage = PortalHelper.GetLangIdOfCultureName(LangId) == String.Empty ? MainLangId : LangId,
                    IsComingFromErrorPage = "1"
                }
            });
        }

        public ActionResult NoRoles()
        {
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
            String ParentScript = client.GetDynamicScriptForEntity(PortalId);
            return View(new GeneralModel()
            {
                MenuViewModel = new MenuModel()
                {
                    Picture = Session[URL + "Picture"].ToString(),
                    LoggedUser = HttpContext.User.Identity.Name,
                    Menus = null,
                    Scripts = ParentScript,
                    Languages = Languages
                },
                LocalizationModel = LanguageModel,
                ExtraViewModel = new ExtraModel()
                {
                    PortalLanguage = MainLangId,
                    IsComingFromErrorPage = "1",
                    IsEditFormInNewWindow = "1"
                }
            });
        }

        [AllowAnonymous]
        public ActionResult ExternalErrorPage(String Message)
        {
            GeneralModel MenuViewModel = new GeneralModel()
              {

                  ExtraViewModel = new ExtraModel
                  {
                      ErrorMessage = Message,
                      IsComingFromErrorPage = "1"
                  }
              };
            return View(MenuViewModel);
        }

        [AllowAnonymous]
        public ActionResult GeneralError()
        {
            GeneralModel MenuViewModel = new GeneralModel()
            {

                ExtraViewModel = new ExtraModel
                {
                    ErrorMessage = "Something Goes Wrong",
                    IsComingFromErrorPage = "1"
                }
            };
            return View(MenuViewModel);
        }


    }
}