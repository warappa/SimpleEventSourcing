using System.Web.Optimization;

namespace Shop.UI.Web
{
    public class BundleConfig
    {
        // Weitere Informationen zu Bundling finden Sie unter "http://go.microsoft.com/fwlink/?LinkId=301862".
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/requirejs").Include(
                        "~/Scripts/require.js",
                        "~/Scripts/text.js"));

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/metro-ui/jquery.ui.widget.js",
                        "~/Scripts/metro-ui/metro.js"));

            // Verwenden Sie die Entwicklungsversion von Modernizr zum Entwickeln und Erweitern Ihrer Kenntnisse. Wenn Sie dann
            // für die Produktion bereit sind, verwenden Sie das Buildtool unter "http://modernizr.com", um nur die benötigten Tests auszuwählen.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/metroui").Include(

                      ));

            bundles.Add(new StyleBundle("~/bundles/metrouicss").Include(
                      "~/Content/metro-ui/css/*.css"
                      ));

            bundles.Add(new ScriptBundle("~/bundles/knockout").Include(
                      "~/Scripts/knockout-{version}.js",
                      "~/Scripts/ko.bindinghandlers.js",
                      "~/Scripts/knockout.punches.js",
                      "~/Scripts/linq.js",
                      "~/Scripts/signals.js",
                      "~/Scripts/hasher.js",
                      "~/Scripts/crossroads.js",
                      "~/Scripts/moment.js"));

            bundles.Add(new ScriptBundle("~/bundles/app").IncludeDirectory(
                      "~/app", "*.js", true));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css",
                      "~/Content/font-awesome.min.css"));

            var viewBundle = new Bundle("~/Content/views").IncludeDirectory(
                      "~/app", "*.html", true);
            viewBundle.EnableFileExtensionReplacements = true;

            bundles.Add(viewBundle);

            // Festlegen von EnableOptimizations auf false für Debugzwecke. Weitere Informationen
            // finden Sie unter http://go.microsoft.com/fwlink/?LinkId=301862
            BundleTable.EnableOptimizations = false;
        }
    }
}
