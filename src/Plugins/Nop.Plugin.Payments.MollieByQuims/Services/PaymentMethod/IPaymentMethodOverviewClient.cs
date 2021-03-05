using System.Threading.Tasks;
using Mollie.Api.Models.PaymentMethod;
using Nop.Plugin.Payments.MollieByQuims.Models;

namespace Nop.Plugin.Payments.MollieByQuims.Services.PaymentMethod
{
    public interface IPaymentMethodOverviewClient {
        Task<OverviewModel<PaymentMethodResponse>> GetList();
    }
}