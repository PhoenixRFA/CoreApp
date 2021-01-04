using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace MVCApp.Infrastructure.ViewComponents
{
    public class ShowContextViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(string foo = "bar")
        {
            var sb = new StringBuilder("<h4>ViewComponent Context</h4>");

            sb.Append("<ul>");
            sb.AppendFormat("<li><b>{0}:</b> {1} <i>(from {2})</i></li>", "Session ID", HttpContext.Session.Id, "HttpContext");
            sb.AppendFormat("<li><b>{0}:</b> {1} <i>(from {2})</i></li>", "ModelValid", ModelState.IsValid, "ModelState");
            sb.AppendFormat("<li><b>{0}:</b> {1} <i>(from {2})</i></li>", "Path", Request.Path, "Request");
            sb.AppendFormat("<li><b>{0}:</b> {1} <i>(from {2})</i></li>", "Controller", RouteData.Values["controller"], "RouteData");
            sb.AppendFormat("<li><b>{0}:</b> {1} <i>(from {2})</i></li>", "Link", Url.Action("TagHelpers", "Helpers"), "Url.Action");
            sb.AppendFormat("<li><b>{0}:</b> {1} <i>(from {2})</i></li>", "User", User.Identity?.Name ?? "anonymus", "User");
            sb.AppendFormat("<li><b>{0}:</b> {1} <i>(from {2})</i></li>", "Title", ViewBag.Title, "ViewBag");
            sb.AppendFormat("<li><b>{0}:</b> {1} <i>(from {2})</i></li>", "ViewPath", ViewContext.View.Path, "ViewContext");
            sb.AppendFormat("<li><b>{0}:</b> {1} <i>(from {2})</i></li>", "Foo argument", ViewComponentContext.Arguments["foo"], "ViewComponentContext");
            sb.AppendFormat("<li><b>{0}:</b> {1} <i>(from {2})</i></li>", "Title", ViewData["Title"], "ViewData");
            sb.Append("</ul>");

            var res = new HtmlString(sb.ToString());
            return new HtmlContentViewComponentResult(res);
        }
    }
}
