using System.Threading.Tasks;
using Nop.Plugin.Payments.MollieByQuims.Models;

namespace Nop.Plugin.Payments.MollieByQuims.Services.Payment
{
    public interface IPaymentStorageClient {
        Task Create(CreatePaymentModel model);
    }
}