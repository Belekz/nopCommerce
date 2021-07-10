using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Widgets.InternetInformationStats.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.InternetInformationStats.Fields.FileLocation")]

        public string FileLocation { get; set; }
        public bool FileLocation_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.InternetInformationStats.Fields.CultureInfo")]

        public string CultureInfo { get; set; }
        public bool CultureInfo_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.InternetInformationStats.Fields.HideCheckoutButton")]

        public bool HideCheckoutButton { get; set; }
        public bool HideCheckoutButton_OverrideForStore { get; set; }

    }
}