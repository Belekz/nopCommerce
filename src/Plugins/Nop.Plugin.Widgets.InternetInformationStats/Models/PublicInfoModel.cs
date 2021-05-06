using Nop.Web.Framework.Models;
using Nop.Web.Models.ShoppingCart;

namespace Nop.Plugin.Widgets.InternetInformationStats.Models
{
    public record PublicInfoModel : BaseNopModel
    {
        public int NumberOfVisitorsToday { get; set; }
        public int NumberOfOnlineVisitors { get; set; }
        public string Date { get; set; }
    }
}