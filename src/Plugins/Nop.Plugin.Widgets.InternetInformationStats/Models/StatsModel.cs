using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Models.ShoppingCart;

namespace Nop.Plugin.Widgets.InternetInformationStats.Models
{
    public record StatsModel : BaseNopModel
    {
        public WeekModel Week { get; set; }
        public List<CountryModel> Countries { get; set; } = new List<CountryModel>();
    }
}