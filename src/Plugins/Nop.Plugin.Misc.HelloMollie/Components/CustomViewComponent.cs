using Microsoft.AspNetCore.Mvc;
using Nop.Services.Cms;
using Nop.Services.Plugins;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using Nop.Plugin.Misc.HelloMollieForNop.Models;

namespace Nop.Plugin.Misc.HelloMollieForNop.Components
{

    [ViewComponent(Name = "HelloMollieForNopWidget")]
    public class ExampleWidgetViewComponent : NopViewComponent
    {
        public CreatePaymentModel NewPayment = new CreatePaymentModel();

        public IViewComponentResult Invoke(string widgetZone)
        {
            //return Content("Hello MollieForNop");
            return View("~/Plugins/Misc.HelloMollieForNop/Views/Create.cshtml", NewPayment);
        }

        
    }

}