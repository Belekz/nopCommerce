using Mollie.Api.Models.Url;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.MollieByQuims.Models
{
    public class OverviewNavigationLinksModel : BaseNopModel
    {
        public UrlLink Previous { get; set; }
        public UrlLink Next { get; set; }

        public OverviewNavigationLinksModel(UrlLink previous, UrlLink next) {
            this.Previous = previous;
            this.Next = next;
        }
    }
}