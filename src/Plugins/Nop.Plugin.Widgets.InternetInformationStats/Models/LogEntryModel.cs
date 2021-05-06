using Nop.Web.Framework.Models;
using Nop.Web.Models.ShoppingCart;

namespace Nop.Plugin.Widgets.InternetInformationStats.Models
{
    public record LogEntryModel : BaseNopModel
    {
        public string Date { get; set; }
        public string Time { get; set; }
        public string IPsource { get; set; }
        public string Type { get; set; }
        public string Page { get; set; }
        public int Port { get; set; }
        public string IPdestination { get; set; }

    }
}