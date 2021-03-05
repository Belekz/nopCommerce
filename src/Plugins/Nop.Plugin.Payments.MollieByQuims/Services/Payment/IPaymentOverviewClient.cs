using System.Threading.Tasks;
using Mollie.Api.Models.Payment.Response;
using Nop.Plugin.Payments.MollieByQuims.Models;

namespace Nop.Plugin.Payments.MollieByQuims.Services.Payment
{
    public interface IPaymentOverviewClient {
        Task<OverviewModel<PaymentResponse>> GetList();
        Task<OverviewModel<PaymentResponse>> GetListByUrl(string url);
    }
}