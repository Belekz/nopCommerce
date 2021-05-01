using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.MollieForNop.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Payments.MollieForNop.Fields.ApiKey")]

        public string ApiKey { get; set; }
        public bool Api_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.MollieForNop.Fields.SiteURL")]

        public string SiteURL { get; set; }
        public bool SiteURL_OverrideForStore { get; set; }
    }
}