using Mollie.Api.Models.Url;

namespace Nop.Plugin.Misc.HelloMollie.Models
{
    public class OverviewNavigationLinksModel {
        public UrlLink Previous { get; set; }
        public UrlLink Next { get; set; }

        public OverviewNavigationLinksModel(UrlLink previous, UrlLink next) {
            this.Previous = previous;
            this.Next = next;
        }
    }
}