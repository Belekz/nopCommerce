using Nop.Core.Configuration;

namespace Nop.Plugin.Widgets.InternetInformationStats
{
    /// <summary>
    /// Represents settings of manual payment plugin
    /// </summary>
    public class InternetInformationStatsSettings : ISettings
    {
        /// <summary>
        /// Gets or sets the location for the log-file
        /// </summary>
        public string FileLocation { get; set; }

        /// <summary>
        /// Gets or sets own IP address, to ignore in log files
        /// </summary>
        public string CultureInfo { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to hide the check-out button
        /// </summary>
        public bool HideCheckoutButton { get; set; }

    }
}
