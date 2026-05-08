using Cyrena.Coding.Extensions;
using Cyrena.Coding.Models;

namespace Cyrena.Website.Extensions
{
    public static class DevelopPlanExtensions
    {
        public static void IndexStaticWebsiteDefaultPlan(this DevelopPlan plan)
        {
            plan.IndexFiles("html", "html_");
            plan.IndexFiles("ico", "ico_");

            var css = plan.GetOrCreateFolder("css", "css");
            plan.IndexFiles(css, "css", "css_");

            var js = plan.GetOrCreateFolder("js", "js");
            plan.IndexFiles(js, "js", "js_");

            var images = plan.GetOrCreateFolder("images", "images");
            plan.IndexFiles(images, "png", "image_");
            plan.IndexFiles(images, "jpg", "image_");
            plan.IndexFiles(images, "jpeg", "image_");
            plan.IndexFiles(images, "svg", "image_");
            plan.IndexFiles(images, "ico", "image_");
            plan.IndexFiles(images, "webp", "image_");

            var assets = plan.GetOrCreateFolder("assets", "assets");
            plan.IndexFiles(assets, "pdf", "asset_");
            plan.IndexFiles(assets, "zip", "asset_");
            plan.IndexFiles(assets, "webmanifest", "asset_");

            var fonts = plan.GetOrCreateFolder("fonts", "fonts");
            plan.IndexFiles(fonts, "woff", "font_", true);
            plan.IndexFiles(fonts, "woff2", "font_", true);
            plan.IndexFiles(fonts, "ttf", "font_", true);

            plan.IndexFiles("json", "json_");
            plan.IndexFiles("xml", "xml_");
            plan.IndexFiles("txt", "txt_");
        }
    }
}
