using Nop.Web.Framework.Models;
using Nop.Web.Models.ShoppingCart;

namespace Nop.Plugin.Widgets.SideShoppingCart.Models
{
    public record PublicInfoModel : BaseNopModel
    {
        public MiniShoppingCartModel ShoppingCartModel { get; set; }

        #region Settings
        public int MaximumShoppingCartItems { get; set; }
        public bool ShowProductImagesOnShoppingCart { get; set; }
        public bool HideCheckoutButton { get; set; }
        #endregion
    }
}