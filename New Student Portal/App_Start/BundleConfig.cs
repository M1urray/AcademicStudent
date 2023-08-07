using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;

namespace New_Student_Portal.App_Start
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/CSS/bootstrap.min.css",
                "~/CSS/waves.min.css",
                "~/Feathers/feather.css",
                "~/CSS/style.css"));

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/JS/jquery.min.js",
                "~/JS/jquery-ui.min.js",
                "~/JS/bootstrap.min.js",
                "~/JS/modernizr.js",
                "~/JS/css-scrollbars.js",
                "~/JS/waves.min.js",
                "~/JS/jquery.slimscroll.js",
                "~/JS/pcoded.min.js",
                "~/JS/vertical-layout.min.js",
                "~/JS/script.js"));
        }
    }
}