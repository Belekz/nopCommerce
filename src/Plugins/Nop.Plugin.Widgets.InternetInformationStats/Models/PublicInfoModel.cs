using Nop.Web.Framework.Models;
using Nop.Web.Models.ShoppingCart;

namespace Nop.Plugin.Widgets.InternetInformationStats.Models
{
    public record PublicInfoModel : BaseNopModel
    {
        public int VisitorsToday { get; set; }
        public int VisitorsThisMonth { get; set; }
        public int OnlineVisitors { get; set; }
        public string Date { get; set; }
    }
}