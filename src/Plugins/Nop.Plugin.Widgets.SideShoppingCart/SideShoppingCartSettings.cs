using Nop.Core.Configuration;

namespace Nop.Plugin.Widgets.SideShoppingCart
{
    /// <summary>
    /// Represents settings of manual payment plugin
    /// </summary>
    public class SideShoppingCartSettings : ISettings
    {
        /// <summary>
        /// Gets or sets a value indicating maximum number of items in the shopping cart
        /// </summary>
        public int MaximumShoppingCartItems { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show product image on shopping cart page
        /// </summary>
        public bool ShowProductImagesOnShoppingCart { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to hide the check-out button
        /// </summary>
        public bool HideCheckoutButton { get; set; }

    }
}
