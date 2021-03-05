using Microsoft.AspNetCore.Mvc;
using Nop.Services.Cms;
using Nop.Services.Plugins;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;

namespace Nop.Plugin.Misc.Hello2.Components
{
    [ViewComponent(Name = "HelloWorldWidget")]
    public class ExampleWidgetViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke(string widgetZone)
        {
            return Content("Hello World");
        }
    }

}