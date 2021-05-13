using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Models.ShoppingCart;

namespace Nop.Plugin.Widgets.InternetInformationStats.Models
{
    public record CountryModel : BaseNopModel
    {
        public string Name { get; set; }
        public string City { get; set; }
        public string Code { get; set; }
        public int Count { get; set; }
    }
}