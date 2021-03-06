using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;
using System;

namespace Nop.Plugin.Payments.MollieByQuims.Components
{
    [ViewComponent(Name = "MollieByQuims")]
    public class CustomViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke(int productId)
        {
            return View("~/Plugins/Payments.MollieByQuims/Views/PaymentInfo.cshtml");
        }
    }
}
