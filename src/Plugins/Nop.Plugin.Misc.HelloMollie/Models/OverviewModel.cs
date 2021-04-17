using System.Collections.Generic;
using MollieForNop.Api.Models;

namespace Nop.Plugin.Misc.HelloMollieForNop.Models
{
    public class OverviewModel<T> where T : IResponseObject {
        public List<T> Items { get; set; }
        public OverviewNavigationLinksModel Navigation { get; set; }
    }
}