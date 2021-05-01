using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Mollie.Api.Client;
using Mollie.Api.Client.Abstract;
using Mollie.Api.Models;
using Mollie.Api.Models.Order;
using Mollie.Api.Models.Payment;
using Mollie.Api.Models.Payment.Request;
using Mollie.Api.Models.Payment.Response;
using Nop.Core;
using Nop.Core.Domain.Orders;
using System.Threading.Tasks;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.MollieForNop.Models;
using Nop.Plugin.Payments.MollieForNop.Validators;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Services.Plugins;

namespace Nop.Plugin.Payments.MollieForNop
{
    /// <summary>
    /// Manual payment processor
    /// </summary>
    public class MollieForNopPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IPaymentService _paymentService;
        private readonly ISettingService _settingService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHelper _webHelper;
        private readonly MollieForNopPaymentSettings _MollieForNopPaymentSettings;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public MollieForNopPaymentProcessor(ILocalizationService localizationService,
            IPaymentService paymentService,
            ISettingService settingService,
            IWebHelper webHelper,
            IHttpContextAccessor httpContextAccessor,
            IGenericAttributeService genericAttributeService,
            IStoreContext storeContext,
            MollieForNopPaymentSettings MollieForNopPaymentSettings)
        {
            _localizationService = localizationService;
            _paymentService = paymentService;
            _settingService = settingService;
            _webHelper = webHelper;
            _MollieForNopPaymentSettings = MollieForNopPaymentSettings;
            _httpContextAccessor = httpContextAccessor;
            _genericAttributeService = genericAttributeService;
            _storeContext = storeContext;
        }

        #endregion


        #region Methods

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return Task.FromResult(new ProcessPaymentResult());
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public async Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            string total = string.Format("{0:0.00}", postProcessPaymentRequest.Order.OrderSubtotalInclTax);
            total = total.Replace(",", "."); // Bug fix for use of comma 
            string url = _MollieForNopPaymentSettings.SiteURL + "PaymentMollieForNop/Verify";

            var store = await _storeContext.GetCurrentStoreAsync();

            IPaymentClient paymentClient = new PaymentClient(_MollieForNopPaymentSettings.ApiKey);
            PaymentRequest paymentRequest = new PaymentRequest()
            {
                Amount = new Amount(Currency.EUR, total),
                Description = $"{store.Name} - ID: {postProcessPaymentRequest.Order.Id.ToString()}",
                RedirectUrl = url
            };

            PaymentResponse paymentResponse = paymentClient.CreatePaymentAsync(paymentRequest).GetAwaiter().GetResult();

            // Write info to repository 
            Identifier identifier = new Identifier();
            identifier.MollieForNopInfo = paymentResponse;
            identifier.OrderInfo = postProcessPaymentRequest.Order;
            Repository.AddInfo(identifier);

            // Redirect to MollieForNop
            _httpContextAccessor.HttpContext.Response.Redirect(paymentResponse.Links.Checkout.Href);

            return;
        }

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return Task.FromResult(false);
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <returns>Additional handling fee</returns>
        public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            // No fee 
            return await _paymentService.CalculateAdditionalFeeAsync(cart,0, false);
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            return Task.FromResult(new CapturePaymentResult { Errors = new[] { "Capture method not supported" } });
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            return Task.FromResult(new RefundPaymentResult { Errors = new[] { "Refund method not supported" } });
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            return Task.FromResult(new VoidPaymentResult { Errors = new[] { "Void method not supported" } });
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return Task.FromResult(new ProcessPaymentResult { Errors = new[] { "Recurring payment not supported" } });
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            return Task.FromResult(new CancelRecurringPaymentResult { Errors = new[] { "Recurring payment not supported" } });
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public Task<bool> CanRePostProcessPaymentAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            //let's ensure that at least 5 seconds passed after order is placed
            //P.S. there's no any particular reason for that. we just do it
            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalSeconds < 5)
                return Task.FromResult(false);

            return Task.FromResult(true);
        }

        /// <summary>
        /// Validate payment form
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>List of validating errors</returns>
        public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            return Task.FromResult<IList<string>>(new List<string>());
        }

        /// <summary>
        /// Get payment information
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>Payment info holder</returns>
        public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            return Task.FromResult(new ProcessPaymentRequest());
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentMollieForNop/Configure";
        }

        /// <summary>
        /// Gets a name of a view component for displaying plugin in public store ("payment info" checkout step)
        /// </summary>
        /// <returns>View component name</returns>
        public string GetPublicViewComponentName()
        {
            return "PaymentMollieForNop";
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override async Task InstallAsync()
        {
            //settings
            var settings = new MollieForNopPaymentSettings { };
            await _settingService.SaveSettingAsync(settings);

            //locales
            await _localizationService.AddLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.Payments.MollieForNop.Paymentmethoddescription"] = "Continue your payment via https://www.Mollie.com/",
                ["Plugins.Payments.MollieForNop.Instructions"] = "Create an account at Mollie.com. Add payment methods to your Mollie account. Enter your Mollie API Key bellow, this can be either a test key or a live key. Next correct the URL when the automatic filled in URL doesn't match your site.",
                ["Plugins.Payments.MollieForNop.Fields.RedirectionTip"] = "Continue your payment with Mollie",
                ["Plugins.Payments.MollieForNop.Fields.PaymentHasFailed"] = "Payment via MollieForNop has failed. Please try again.",
                ["Plugins.Payments.MollieForNop.Fields.ApiKey"] = "Mollie API key:",
                ["Plugins.Payments.MollieForNop.Fields.SiteURL"] = "Website base URL:",
            });

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<MollieForNopPaymentSettings>();

            //locales
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.Payments.MollieForNop");

            await base.UninstallAsync();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture => false;

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund => false;

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund => false;

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid => false;

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        //public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.Manual;
        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        //public PaymentMethodType PaymentMethodType => PaymentMethodType.Standard;
        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo => true;

        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        /// <remarks>
        /// return description of this payment method to be display on "payment method" checkout step. good practice is to make it localizable
        /// for example, for a redirection payment method, description may be like this: "You will be redirected to PayPal site to complete the payment"
        /// </remarks>
        public async Task<string> GetPaymentMethodDescriptionAsync() => await _localizationService.GetResourceAsync("Plugins.Payments.MollieForNop.PaymentMethodDescription");

        #endregion

    }
}