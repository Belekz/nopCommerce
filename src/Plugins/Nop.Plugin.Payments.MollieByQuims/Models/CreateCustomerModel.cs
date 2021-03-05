using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.MollieByQuims.Models
{
    public class CreateCustomerModel : BaseNopModel

    {
        [Required]
        public string Name { get; set; }

        [EmailAddress]
        public string Email { get; set; }
    }
}