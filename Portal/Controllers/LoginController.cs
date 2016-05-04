using Newtonsoft.Json;
using Portal.Helper;
using Portal.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Security;

namespace Portal.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {

        String URL = System.Web.HttpContext.Current.Request.Url.Authority;
        String PortalId = String.Empty;
        // GET: /Login/
        PortalService.PortalServiceClient client = new PortalService.PortalServiceClient(PortalHelper.GetBasicHttpBinding(), PortalHelper.GetEndPointAddress(System.Web.HttpContext.Current.Request.Url.Authority));

        [OutputCache(Duration = 3600, VaryByParam = "*")]
        public ActionResult Index(String id)
        {
            MenuModel model = new MenuModel();


            PortalService.CrmServiceInformation portalserviceinfo = client.GetPortalIdAndCreateSourceCrmService(URL);

            PortalHelper.SetCookies(URL, "languagecookie", "language", portalserviceinfo.PortalMainLanguageCode, 60);
            PortalHelper.SetCookies(URL, "themecookie", "theme", portalserviceinfo.ThemeType, 60);
            PortalHelper.SetCookies(URL, "portalcookie", "portalinfo", JsonConvert.SerializeObject(portalserviceinfo), 60);


            if (((HttpContext.User).Identity).IsAuthenticated == true)
            {
                HttpCookie authCookie = new HttpCookie("ASP.NET_SessionId", Session.SessionID);
                if (Request.Cookies[".ASPXAUTH"] != null)
                {
                    authCookie.Expires = FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).IssueDate.AddMinutes(60);
                }
                else
                {
                    authCookie.Expires = FormsAuthentication.Decrypt(Request.Cookies[".AUTH"].Value).IssueDate.AddMinutes(60);
                }
                Response.Cookies.Add(authCookie);

                return RedirectToAction("index", "dashboard");
            }
            else
            {
                if (portalserviceinfo.PortalId == null)
                {
                    return RedirectToAction("ExternalErrorPage", "Error", new { Message = "There is no Portal Listening that URL" });
                }
                else if (portalserviceinfo.PortalBaseLanguage == null)
                {
                    return RedirectToAction("ExternalErrorPage", "Error", new { Message = "There is no Base Language Definition for this Portal" });
                }
                else if (portalserviceinfo.PortalMainLanguageCode == null)
                {
                    return RedirectToAction("ExternalErrorPage", "Error", new { Message = "There is no Language Definition for this Portal" });
                }
                if (!String.IsNullOrEmpty(Request.QueryString["returnurl"]))
                {
                    ViewData[portalserviceinfo.PortalId + "returnurl"] = Request.QueryString["returnurl"];
                }
                PortalId = portalserviceinfo.PortalId;

                PortalService.Navigation[] ExternalNavigations = client.GetExternalNavigation(PortalId);
                model = PortalHelper.ChangeWcfNavigationIntoMenuModel(String.Empty, ExternalNavigations);

                //IT doesnt make sense to pop cookies for the user that not login yet
                //Lost the values after close the browser
                Session[URL + "externalmenu"] = JsonConvert.SerializeObject(model.Menus);

                PortalService.LoginInformation information = client.GetLoginInfo(portalserviceinfo);

                PortalHelper.SetCookies(URL, "entitynamecookie", "entityname", information.EntityName, 60);
                PortalHelper.SetCookies(URL, "entityfieldcookie", "entityfield", information.UserNameField, 60);
                PortalHelper.SetCookies(URL, "usernamevaluecookie", "usernamevalue", String.Empty, 60);
                PortalHelper.SetCookies(URL, "configurationcookie", "configuration", portalserviceinfo.ConfigurationId.ToString(), 60);
            }
            var languages = PortalHelper.GetLanguages(portalserviceinfo.PortalId, client);

            return View("LoginScreen", new GeneralModel()
            {
                MenuViewModel = new MenuModel()
                {
                    LoggedUser = model.LoggedUser,
                    Menus = model.Menus,
                    ComingFrom = Enums.MenuSource.External.ToString(),
                    Languages = languages,
                },
                ExtraViewModel = new ExtraModel()
                {
                    PortalId = portalserviceinfo.PortalId,
                    PortalLanguage = PortalHelper.GetNameOfLangId(portalserviceinfo.PortalMainLanguageCode)
                },
                PortalLogoURL = portalserviceinfo.PortalLogoURL
            });

        }

        [HttpPost]
        public ActionResult AttempLogin(LoginModel model)
        {
            PortalService.CrmServiceInformation info = new PortalService.CrmServiceInformation();

            if (Request.Cookies[URL + "portalcookie"] == null)
            {
                info = client.GetPortalIdAndCreateSourceCrmService(URL);
            }
            else
            {
                info = JsonConvert.DeserializeObject<PortalService.CrmServiceInformation>(Request.Cookies[URL + "portalcookie"].Values[0]);
            }
            PortalService.LoginInformation information = client.GetLoginInfo(info);
            PortalId = info.PortalId;

            PortalService.Login login = new PortalService.Login();
            login.UserNameValue = model.UserName;
            login.PasswordValue = model.Password;

            Object[] Result = client.AttemptLogin(info, PortalId, information, login);
            if ((Boolean)Result[0] == true)
            {
                byte[] picture = client.GetPicture(info, PortalId, information.EntityName, information.UserNameField, model.UserName);
                //pictures can not insert into cookies due to their sizes!
                Session[URL + "Picture"] = Convert.ToBase64String(picture);

                PortalHelper.SetCookies(URL, "entitynamecookie", "entityname", information.EntityName, 60);
                PortalHelper.SetCookies(URL, "entityfieldcookie", "entityfield", information.UserNameField, 60);
                PortalHelper.SetCookies(URL, "usernamevaluecookie", "usernamevalue", model.UserName, 60);
                PortalHelper.SetCookies(URL, "usernameguidcookie", "usernameguid", Result[1].ToString(), 60);

                //SetAuthenticationCookie
                FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(1,
                                                                                    model.UserName,
                                                                                    DateTime.Now,
                                                                                    DateTime.Now.AddMinutes(60),
                                                                                    true,
                                                                                    string.Empty);
                string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                System.Web.HttpCookie authCookie = new System.Web.HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                if (authTicket.IsPersistent)
                {
                    authCookie.Expires = authTicket.Expiration;
                }
                System.Web.HttpContext.Current.Response.Cookies.Add(authCookie);
            }
            return Json(Result);
        }

        [HttpPost]
        public ActionResult ClearCache()
        {
            //if user not use logout back , use back button of the browser we need to clear cache values
            if (Request.Cookies[URL + "usernameguidcookie"] != null && Request.Cookies[URL + "portalcookie"] != null)
            {
                PortalService.CrmServiceInformation portalserviceinfo = JsonConvert.DeserializeObject<PortalService.CrmServiceInformation>(Request.Cookies[URL + "portalcookie"].Values[0]);
                String UserNameGuid = Request.Cookies[URL + "usernameguidcookie"].Values[0].ToString();
                //clear cache               
                ObjectCache cache = MemoryCache.Default;
                if (!String.IsNullOrEmpty(Convert.ToString(cache[portalserviceinfo.PortalId + UserNameGuid + "menucache"])))
                {
                    cache.Remove(portalserviceinfo.PortalId + Request.Cookies[URL + "usernameguidcookie"].Values[0] + "menucache");
                }
                if (((HttpContext.User).Identity).IsAuthenticated == true)
                {
                    Response.Cookies[URL + "usernameguidcookie"].Expires = DateTime.Now.AddDays(-1);
                }
            }


            return Json("");
        }

        public ActionResult ResetPassword(String Value, String LangId)
        {
            PortalService.CrmServiceInformation portalserviceinfo = JsonConvert.DeserializeObject<PortalService.CrmServiceInformation>(Request.Cookies[URL + "portalcookie"].Values[0]);

            String html = String.Empty;
            if (System.IO.File.Exists(HttpContext.Server.MapPath("~/Helper/EmailTemplates/" + portalserviceinfo.PortalId + "_" + LangId + ".html")))
            {
                html = System.IO.File.ReadAllText(HttpContext.Server.MapPath("~/Helper/EmailTemplates/" + portalserviceinfo.PortalId + "_" + LangId + ".html"));
            }
            else
            {
                html = System.IO.File.ReadAllText(HttpContext.Server.MapPath("~/Helper/EmailTemplates/default.html"));
            }
            String Result = client.ResetPassword(portalserviceinfo.PortalId, Value, html, portalserviceinfo);
            return Json(Result);
        }



    }
}