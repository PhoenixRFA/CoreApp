using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;

namespace MVCApp.App_code
{
    public static class ListHelper
    {
        /// <summary> Example of simple HtmlHelper </summary>
        public static HtmlString CreateList(this IHtmlHelper helper, ICollection<string> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            var sb = new StringBuilder("<ul>");

            foreach (string item in items)
            {
                sb.AppendFormat("<li>{0}</li>", item);
            }

            sb.Append("</ul>");

            return new HtmlString(sb.ToString());
        }

        public static HtmlString CreateListOnTagBuilders(this IHtmlHelper helper, ICollection<string> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            var ul = new TagBuilder("ul");

            foreach (string item in items)
            {
                var li = new TagBuilder("li");
                li.InnerHtml.Append(item);
                li.AddCssClass("list-group-item");

                ul.InnerHtml.AppendHtml(li);
            }

            ul.Attributes.Add("class", "list-group");

            var writer = new StringWriter();
            ul.WriteTo(writer, HtmlEncoder.Default);
            
            return new HtmlString(writer.ToString());
        }
    }
}
