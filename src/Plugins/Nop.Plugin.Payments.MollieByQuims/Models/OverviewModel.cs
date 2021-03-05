using System.Collections.Generic;
using Mollie.Api.Models;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.MollieByQuims.Models
{
    public class OverviewModel<T> where T : IResponseObject 
    {
        public List<T> Items { get; set; }
        public OverviewNavigationLinksModel Navigation { get; set; }
    }
}