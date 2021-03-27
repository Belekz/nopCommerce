using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.ManualMollie
{
    /// <summary>
    /// Represents settings of manual payment plugin
    /// </summary>
    public class ManualMolliePaymentSettings : ISettings
    {
        /// <summary>
        /// Gets or sets Mollie API Key (test key or live key)
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Gets or sets site URL
        /// </summary>
        public string SiteURL { get; set; }
    }
}
