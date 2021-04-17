using System.Threading.Tasks;
using MollieForNop.Api.Models.Payment.Response;
using MollieForNop.WebApplicationCoreExample.Models;

namespace MollieForNop.WebApplicationCoreExample.Services.Payment {
    public interface IPaymentOverviewClient {
        Task<OverviewModel<PaymentResponse>> GetList();
        Task<OverviewModel<PaymentResponse>> GetListByUrl(string url);
    }
}