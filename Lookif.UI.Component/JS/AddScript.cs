using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Lookif.UI.Component.JS
{
    public class AddScript : ComponentBase
    {

        [Parameter]
        public string Source { get; set; }

        [Parameter]
        public string Type { get; set; } = "text/javascript";

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "script");
            builder.AddAttribute(1, "src", Source);
            builder.AddAttribute(2, "type", Type);
            builder.CloseElement();
        }
    }
}
