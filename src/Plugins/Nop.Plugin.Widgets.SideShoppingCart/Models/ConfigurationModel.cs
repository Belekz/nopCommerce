using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Widgets.SideShoppingCart.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.SideShoppingCart.Fields.MaximumShoppingCartItems")]

        public int MaximumShoppingCartItems { get; set; }
        public bool MaximumShoppingCartItems_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.SideShoppingCart.Fields.ShowProductImagesOnShoppingCart")]

        public bool ShowProductImagesOnShoppingCart { get; set; }
        public bool ShowProductImagesOnShoppingCart_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.SideShoppingCart.Fields.HideCheckoutButton")]

        public bool HideCheckoutButton { get; set; }
        public bool HideCheckoutButton_OverrideForStore { get; set; }

    }
}