using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.ManualMollie.Infrastructure
{
    public partial class RouteProvider : IRouteProvider
    {
        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="endpointRouteBuilder">Route builder</param>
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            //PDT  [EXAMPLE]
            //endpointRouteBuilder.MapControllerRoute("Plugin.Payments.PayPalStandard.PDTHandler", "Plugins/PaymentPayPalStandard/PDTHandler",
            //     new { controller = "PaymentPayPalStandard", action = "PDTHandler" });

            //endpointRouteBuilder.MapControllerRoute("Plugin.Payments.ManualMollie.webhook", "Plugins/PaymentManualMollie/webhook",
            //     new { controller = "ManualMolliePaymentProcessor", action = "webhook" });

        }

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => -1;
    }
}