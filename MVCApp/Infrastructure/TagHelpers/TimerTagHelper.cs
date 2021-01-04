using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;

namespace MVCApp.Infrastructure.TagHelpers
{
    //позволяет переопределить поведение tag-хелпера
    //[HtmlTargetElement(TagStructure = TagStructure.NormalOrSelfClosing )]
    public class DateTimeTagHelper : TagHelper
    {
        //атрибуты автоматически привязываются свойствам хелпера
        public string Color { get; set; }
        public string AspSize { get; set; }
        public bool Enabled { get; set; }
        
        [ViewContext]//ссылка на контекст представления
        [HtmlAttributeNotBound]//не осуществлять привязку атрибутов
        public ViewContext ViewContext { get; set; }

        private readonly ILogger<DateTimeTagHelper> _logger;

        public DateTimeTagHelper(ILogger<DateTimeTagHelper> logger)
        {
            _logger = logger;
        }

        //public override void Init(TagHelperContext context)
        //{
        //    base.Init(context);
        //}

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (!Enabled)
            {
                output.SuppressOutput();

                string controller = ViewContext?.RouteData.Values["controller"]?.ToString();
                string action = ViewContext?.RouteData.Values["action"]?.ToString();
                
                _logger.LogInformation($"Tag helper suppressed ({controller}.{action} {nameof(DateTimeTagHelper)})");
                return;
            }


            IEnumerable<string> attrs = context.AllAttributes.Select(x => $"{x.Name}={x.Value} ({x.ValueStyle})");
            string attributes = string.Join(", ", attrs);
            TagHelperContent children = await output.GetChildContentAsync();

            //какой элемент html будет создаваться вместо тега хелпера
            output.TagName = "div";
            
            //формат создаваемого элемента (с одним или с двумя тегами)
            output.TagMode = TagMode.StartTagAndEndTag;

            output.Attributes.RemoveAll("asp-test");
            output.Attributes.Add("asp-inner-attribute", "fooBar");
            output.Attributes.Add("style", $"color: {Color}; font-size: {AspSize};");

            output.PreContent.SetHtmlContent("<i>pre</i>");
            output.PostContent.SetHtmlContent("<i>post</i>");

            //await Task.Delay(500);
            output.Content.SetHtmlContent($"Time now: {DateTime.Now}<br>Attributes: {attributes}<br>");
            output.Content.AppendHtml(children);
        }
    }
}
