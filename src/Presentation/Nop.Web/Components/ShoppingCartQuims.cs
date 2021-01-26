using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Web.Components
{
    public class ShoppingCartQuimsViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke()
        {

            return View();
        }
    }
}
