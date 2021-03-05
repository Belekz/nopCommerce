using System.ComponentModel.DataAnnotations;
using Mollie.Api.Models;

namespace Nop.Plugin.Misc.HelloMollie.Models
{ 
    public class CreatePaymentModel {
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
    }
}
