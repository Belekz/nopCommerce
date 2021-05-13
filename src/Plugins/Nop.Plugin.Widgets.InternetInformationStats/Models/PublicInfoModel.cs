using Nop.Web.Framework.Models;
using Nop.Web.Models.ShoppingCart;

namespace Nop.Plugin.Widgets.InternetInformationStats.Models
{
    public record PublicInfoModel : BaseNopModel
    {
        public StatsModel Data { get; set; }
        public string ErrorMessage { get; set; }
    }
}