using Newtonsoft.Json;
using Portal.Helper;
using Portal.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Portal.Controllers
{
    public class ExternalController : Controller
    {
        [Authorize]
        public ActionResult WithLogin(String PageId, String LangId)
        {
            List<QueryStrings> QueryStringList = new List<QueryStrings>();

            var QueryStrings = Request.QueryString;
            foreach (var item in QueryStrings)
            {
                QueryStrings q = new Models.QueryStrings();
                q.Key = item.ToString();
                q.Value = QueryStrings[item.ToString()];
                QueryStringList.Add(q);
            }
            ExternalModel Model = new ExternalModel
            {
                LangId = LangId,
                PageId = PageId,
                QueryStrings = QueryStringList
            };
            
            return RedirectToAction("BuildPage", "Page", new { NavigationId = PageId, LangId = LangId, IsComingFromExternal = "1", QueryStrings = JsonConvert.SerializeObject(QueryStringList) });

        }

        [AllowAnonymous]
        public ActionResult WithoutLogin()
        {
            return View();
        }
    }
}