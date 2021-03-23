using System;
using System.Collections.Generic;
using System.Text;
using Nop.Core.Domain.Orders;
using Mollie.Api.Models.Payment.Response;

namespace Nop.Plugin.Payments.ManualMollie.Models
{
    public class Identifier
    { 

        public Order OrderInfo { get; set; }

        public PaymentResponse MollieInfo { get; set; } 
    }
}
