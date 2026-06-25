using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace NPO_Workflow.TagHelpers
{
    [HtmlTargetElement("a", Attributes ="asp-controller")]
    public class ActiveLinkTagHelper : TagHelper
    {
        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var currentController = ViewContext.RouteData.Values["controller"]?.ToString();
            var targetController = context.AllAttributes["asp-controller"]?.Value?.ToString();

            if (string.Equals(currentController, targetController, StringComparison.OrdinalIgnoreCase))
            {
                var existingClasses = output.Attributes["class"]?.Value?.ToString() ?? "";
                output.Attributes.SetAttribute("class", $"{existingClasses} active");
            } 
        }
    }
}
