﻿using System.Collections.Generic;
using Mollie.Api.Models;

namespace Nop.Plugin.Misc.HelloMollie.Models
{
    public class OverviewModel<T> where T : IResponseObject {
        public List<T> Items { get; set; }
        public OverviewNavigationLinksModel Navigation { get; set; }
    }
}