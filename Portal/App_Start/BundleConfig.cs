using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;

namespace Portal.App_Start
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {

            bundles.Add(new StyleBundle("~/bundles/MetronicMadatoryCss").Include(
                        "~/assets/global/plugins/font-awesome/css/fontapis.css",
                        "~/assets/global/plugins/font-awesome/css/font-awesome.min.css",
                        "~/assets/global/plugins/simple-line-icons/simple-line-icons.min.css",
                        "~/assets/global/plugins/bootstrap/css/bootstrap.min.css",
                        "~/assets/global/plugins/uniform/css/uniform.default.css"));

            bundles.Add(new StyleBundle("~/bundles/ThemeStyles").Include(
                        "~/assets/global/css/components-rounded.css",
                        "~/assets/global/css/plugins.css",
                        "~/assets/admin/layout/css/layout.css",
                        "~/assets/admin/layout/css/themes/default.css",
                        "~/assets/admin/layout/css/custom.css"));

            bundles.Add(new StyleBundle("~/bundles/ThemeStylesIndex").Include(
                       "~/assets/global/css/components-rounded.css",
                       "~/assets/global/css/plugins.css",
                       "~/assets/admin/layout3/css/layout.css",
                       "~/assets/admin/layout3/css/themes/default.css",
                       "~/assets/admin/layout3/css/custom.css"));

            bundles.Add(new StyleBundle("~/bundles/DarkThemeStyle").Include(
                     "~/assets/global/css/components-rounded.css",
                     "~/assets/global/css/plugins.css",
                     "~/assets/admin/layout5/css/layout.css",
                     "~/assets/admin/layout5/css/themes/default.css",
                     "~/assets/admin/layout5/css/custom.css"));

            bundles.Add(new ScriptBundle("~/bundles/JQuery").Include(
                "~/assets/global/plugins/jquery.min.js",            
                "~/assets/global/plugins/bootstrap/js/bootstrap.min.js",
                "~/assets/global/plugins/jquery.blockui.min.js",
                "~/assets/global/plugins/uniform/jquery.uniform.js",
                "~/assets/global/plugins/jquery.cokie.min.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/JQueryIndex").Include(
               "~/Content/Javascript/Json.js",
               "~/assets/global/plugins/jquery-ui/jquery-ui-1.10.3.custom.min.js",
               "~/assets/global/plugins/bootstrap/js/bootstrap.js",
               "~/assets/global/plugins/bootstrap-hover-dropdown/bootstrap-hover-dropdown.js",
               "~/assets/global/plugins/jquery-inputmask/jquery.inputmask.bundle.min.js",
               "~/assets/global/plugins/jquery-slimscroll/jquery.slimscroll.min.js",
               "~/assets/global/plugins/jquery.blockui.min.js",
               "~/assets/global/plugins/jquery.cokie.min.js",
               "~/assets/global/plugins/uniform/jquery.uniform.js"              
               ));
        }
    }
}