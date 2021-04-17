using System.Threading.Tasks;
using MollieForNop.WebApplicationCoreExample.Models;

namespace MollieForNop.WebApplicationCoreExample.Services.Payment {
    public interface IPaymentStorageClient {
        Task Create(CreatePaymentModel model);
    }
}