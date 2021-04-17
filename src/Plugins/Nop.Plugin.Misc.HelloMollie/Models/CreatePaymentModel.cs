using System.ComponentModel.DataAnnotations;
using MollieForNop.Api.Models;

namespace Nop.Plugin.Misc.HelloMollieForNop.Models
{ 
    public class CreatePaymentModel {
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
    }
}
