using Microsoft.AspNetCore.Mvc;
using Nop.Services.Cms;
using Nop.Services.Plugins;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using Nop.Plugin.Misc.HelloMollie.Models;

namespace Nop.Plugin.Misc.HelloMollie.Components
{

    [ViewComponent(Name = "HelloMollieWidget")]
    public class ExampleWidgetViewComponent : NopViewComponent
    {
        public CreatePaymentModel NewPayment = new CreatePaymentModel();

        public IViewComponentResult Invoke(string widgetZone)
        {
            //return Content("Hello Mollie");
            return View("~/Plugins/Misc.HelloMollie/Views/Create.cshtml", NewPayment);
        }

        
    }

}