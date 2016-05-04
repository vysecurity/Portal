using Newtonsoft.Json;
using Portal.Helper;
using Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web.Mvc;
using System.Web.Security;

namespace Portal.Controllers
{

    public class DashboardController : Controller
    {
        private String URL = System.Web.HttpContext.Current.Request.Url.Authority;
        private String LoggedUserGuidValue;
        private String MainLangId;
        private String PortalId;
        private LocalizationModel LanguageModel = new LocalizationModel();
        private List<LanguageModel> Languages = new List<LanguageModel>();

        private PortalService.PortalServiceClient client = null;
        private PortalService.CrmServiceInformation portalserviceinfo = null;

        public DashboardController()
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
                else if (System.Web.HttpContext.Current.Request.Cookies[URL + "usernameguidcookie"] == null)
                {
                    RedirectToAction("Logout", "Dashboard");
                }
                else
                {
                    LoggedUserGuidValue = System.Web.HttpContext.Current.Request.Cookies[URL + "usernameguidcookie"].Values[0].ToString();
                    PortalId = portalserviceinfo.PortalId;
                    MainLangId = portalserviceinfo.PortalMainLanguageCode;
                    //Get Portal LocalizationXML due to CurretnLangId
                    LanguageModel = PortalHelper.ParsePortalLangXml(System.Web.HttpContext.Current.Server.MapPath("~/Helper/Localization.xml"), MainLangId);

                }
            }

        }

        [AllowAnonymous]
        public ActionResult Index()
        {
            //check cookie control
            if (Request.Cookies[URL + "entitynamecookie"] == null)
            {
                return RedirectToAction("Logout", "Dashboard");


            }
            else if (Request.Cookies[URL + "entitynamecookie"].Values[0] == String.Empty)
            {
                return RedirectToAction("Logout", "Dashboard");
            }

            else if (String.IsNullOrEmpty(PortalId))
            {
                return RedirectToAction("Logout", "Dashboard");
            }

            PortalService.Navigation[] navigations = new PortalService.Navigation[] { };
            try
            {
                //FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value)

                List<PortalService.Roles> roles = client.GetLoginUserRoles(portalserviceinfo,
                                                                           PortalId,
                                                                           Request.Cookies[URL + "entitynamecookie"].Values[0],
                                                                           Request.Cookies[URL + "usernamevaluecookie"].Values[0],
                                                                           Request.Cookies[URL + "entityfieldcookie"].Values[0]).ToList();
                if (roles.Count == 0)
                {
                    return RedirectToAction("NoRoles", "Error");
                }
                navigations = client.GetNavigationOfUser(portalserviceinfo, PortalId, roles.ToArray());

                if (navigations.Length == 0)
                {
                    return RedirectToAction("Index", "Error", new { ErrorMessage = "There is no menu item belong that role!", LangId = MainLangId });
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
                        Session[URL + "Picture"] = Convert.ToBase64String(picture as byte[]);
                    }
                }

            }
            catch (Exception ex)
            {
                return RedirectToAction("ExternalErrorPage", "Error", new { Message = ex.Message });
                //throw new Exception(ex.Message);
            }

            navigations = navigations.OrderBy(p => p.Order).ToArray();
            MenuModel model = PortalHelper.ChangeWcfNavigationIntoMenuModel(HttpContext.User.Identity.Name, navigations);

            String Menus = String.Empty;
            ObjectCache cache = MemoryCache.Default;
            if (cache.Contains(PortalId + LoggedUserGuidValue + "menucache"))
                Menus = (String)cache.Get(PortalId + LoggedUserGuidValue + "menucache");
            else
            {
                CacheItemPolicy cacheItemPolicy = new CacheItemPolicy();
                cacheItemPolicy.AbsoluteExpiration = DateTime.Now.AddDays(2);
                cache.Remove(PortalId + LoggedUserGuidValue + "menucache");
                cache.Add(PortalId + LoggedUserGuidValue + "menucache", Server.UrlEncode(JsonConvert.SerializeObject(model.Menus)), cacheItemPolicy);
            }

            var languages = PortalHelper.GetLanguages(portalserviceinfo.PortalId, client);

            return View(new GeneralModel()
            {
                MenuViewModel = new MenuModel()
                {
                    Picture = Session[URL + "picture"].ToString(),
                    LoggedUser = model.LoggedUser,
                    Menus = model.Menus,
                    Languages = languages,
                },
                LocalizationModel = LanguageModel,
                ExtraViewModel = new ExtraModel()
                {
                    PortalLanguage = PortalHelper.GetNameOfLangId(MainLangId)
                },
                PortalLogoURL = portalserviceinfo.PortalLogoURL

            });
        }

        [AllowAnonymous]
        public ActionResult ChangePicture()
        {
            GeneralModel model = new GeneralModel()
            {
                MenuViewModel = new MenuModel()
                {
                    Picture = Session[URL + "Picture"] == null ? String.Empty : Session[URL + "Picture"].ToString(),
                    LoggedUser = HttpContext.User.Identity.Name,

                },
                LocalizationModel = LanguageModel

            };
            return PartialView("PictureUpload", model);
        }

        public ActionResult Logout()
        {
            string[] cookies = Request.Cookies.AllKeys;
            var l = cookies.ToList().Where(p => p.Contains(URL)).ToList();
            foreach (string cookie in l)
            {
                Response.Cookies[cookie].Expires = DateTime.Now.AddDays(-1);
            }

            ObjectCache cache = MemoryCache.Default;
            if (!String.IsNullOrEmpty(Convert.ToString(cache[PortalId + LoggedUserGuidValue + "menucache"])))
            {
                cache.Remove(PortalId + LoggedUserGuidValue + "menucache");
            }

            FormsAuthentication.SignOut();

            return RedirectToAction("index", "login");
        }


    }
}