using System.ComponentModel.DataAnnotations;
using Mollie.Api.Models;
//using Mollie.WebApplicationCoreExample.Framework.Validators;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.MollieByQuims.Models
{
    public class CreatePaymentModel : BaseNopModel
    
    {
        [Required]
        //[Range(0.01, 1000, ErrorMessage = "Please enter an amount between 0.01 and 1000")]
        //[DecimalPlaces(2)]
        public decimal Amount { get; set; }

        [Required]
        //[StaticStringList(typeof(Currency))]
        public string Currency { get; set; }

        [Required]
        public string Description { get; set; }
    }
}
