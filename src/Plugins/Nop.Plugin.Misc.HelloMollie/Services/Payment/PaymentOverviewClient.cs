using System.Threading.Tasks;
using AutoMapper;
using MollieForNop.Api.Client.Abstract;
using MollieForNop.Api.Models.Payment.Response;
using MollieForNop.WebApplicationCoreExample.Models;
using MollieForNop.WebApplicationCoreExample.Services;

namespace MollieForNop.WebApplicationCoreExample.Services.Payment {
    public class PaymentOverviewClient : OverviewClientBase<PaymentResponse>, IPaymentOverviewClient {
        private readonly IPaymentClient _paymentClient;

        public PaymentOverviewClient(IMapper mapper, IPaymentClient paymentClient) : base(mapper) {
            this._paymentClient = paymentClient;
        }

        public async Task<OverviewModel<PaymentResponse>> GetList() {
            return this.Map(await this._paymentClient.GetPaymentListAsync());
        }

        public async Task<OverviewModel<PaymentResponse>> GetListByUrl(string url) {
            return this.Map(await this._paymentClient.GetPaymentListAsync(this.CreateUrlObject(url)));
        }
    }
}