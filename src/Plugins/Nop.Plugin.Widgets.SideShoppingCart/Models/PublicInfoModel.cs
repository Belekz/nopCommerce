using Nop.Web.Framework.Models;

namespace Nop.Plugin.Widgets.SideShoppingCart.Models
{
    public record PublicInfoModel : BaseNopModel
    {
        public string Item { get; set; }
    }
}