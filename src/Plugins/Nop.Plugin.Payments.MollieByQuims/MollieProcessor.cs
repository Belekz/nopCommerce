using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Http.Extensions;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Stores;
using Nop.Web.Framework.Infrastructure;
using Mollie.Api;
using Nop.Plugin.Payments.MollieByQuims.Models;
using Nop.Plugin.Payments.MollieByQuims.Services.Payment;
using Mollie.Api.Models;
using Mollie.Api.Models.Payment.Response;
using Mollie.Api.Client;
using Mollie.Api.Client.Abstract;
using Mollie.Api.Models.Payment.Request;

namespace Nop.Plugin.Payments.MollieByQuims
{
    /// <summary>
    /// Represents a payment method implementation
    /// </summary>
    public class MollieProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IStoreService _storeService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly MollieByQuimsSettings _settings;
        private readonly WidgetSettings _widgetSettings;

        // Mollie
        private readonly IPaymentOverviewClient _paymentOverviewClient;
        private readonly IPaymentStorageClient _paymentStorageClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        // Mollie

        #endregion

        #region Ctor

        public MollieProcessor(IActionContextAccessor actionContextAccessor,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            ISettingService settingService,
            IStoreService storeService,
            IUrlHelperFactory urlHelperFactory,
            MollieByQuimsSettings settings,
            WidgetSettings widgetSettings,

            // Mollie
            IPaymentOverviewClient paymentOverviewClient,
            IPaymentStorageClient paymentStorageClient)
            // Mollie
        {
            _actionContextAccessor = actionContextAccessor;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _settingService = settingService;
            _storeService = storeService;
            _urlHelperFactory = urlHelperFactory;
            _settings = settings;
            _widgetSettings = widgetSettings;

            // Mollie
            _paymentOverviewClient = paymentOverviewClient;
            _paymentStorageClient = paymentStorageClient;

        }

        #endregion

        #region Methods

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            ////settings
            //_settingService.SaveSetting(new MollieByQuimsPaymentSettings
            //{
            //    UseSandbox = true
            //});

            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            //_settingService.DeleteSetting<PayPalStandardPaymentSettings>();

            //locales
            //_localizationService.DeleteLocaleResources("Plugins.Payments.PayPalStandard");

            base.Uninstall();
        }


        /// <summary>
        /// Validate payment form
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>List of validating errors</returns>
        public IList<string> ValidatePaymentForm(IFormCollection form)
        {
            return new List<string>();
        }

        /// <summary>
        /// Get payment info
        /// </summary>
        public ProcessPaymentRequest GetPaymentInfo(IFormCollection form)
        {
            return new ProcessPaymentRequest();
        }

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            return new ProcessPaymentResult();
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public async void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            IPaymentClient paymentClient = new PaymentClient("test_BsMnA5gypddAmp7guP9mAtexVVaC4b");
            PaymentRequest paymentRequest = new PaymentRequest()
            {
                //Amount = new Amount(Currency.EUR, 100.00m),
                Amount = new Amount(Currency.EUR, postProcessPaymentRequest.Order.OrderSubTotalDiscountInclTax),
                //Description = "Test payment of the example project",
                Description = postProcessPaymentRequest.Order.OrderGuid.ToString(),
                //RedirectUrl = "http://google.com"
                RedirectUrl = "https://quims.be/checkout/completed/"
            };

            PaymentResponse paymentResponse = await paymentClient.CreatePaymentAsync(paymentRequest);

            _httpContextAccessor.HttpContext.Response.Redirect(paymentResponse.RedirectUrl);
            return;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            return new CapturePaymentResult();
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            return new VoidPaymentResult();
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            return new RefundPaymentResult();
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            return new ProcessPaymentResult { Errors = new[] { "Recurring payment not supported" } };
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            return new CancelRecurringPaymentResult { Errors = new[] { "Recurring payment not supported" } };
        }

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            return false;
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return decimal.Zero;
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            return false;
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            //return $"{_webHelper.GetStoreLocation()}Admin/MollieByQuims/Configure";
            return "https://www.Quims.be/MollieByQuims/Configure";
        }

        /// <summary>
        /// Gets a view component for displaying plugin in public store ("payment info" checkout step)
        /// </summary>
        /// <param name="viewComponentName">View component name</param>
        public string GetPublicViewComponentName()
        {
            //return Defaults.PAYMENT_INFO_VIEW_COMPONENT_NAME;
            return "MollieByQuims";
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        bool IPaymentMethod.SupportCapture => true;

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        bool IPaymentMethod.SupportVoid => true;

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        bool IPaymentMethod.SupportRefund => true;

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        bool IPaymentMethod.SupportPartiallyRefund => true;

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        RecurringPaymentType IPaymentMethod.RecurringPaymentType => RecurringPaymentType.NotSupported;

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        PaymentMethodType IPaymentMethod.PaymentMethodType => PaymentMethodType.Standard;

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        bool IPaymentMethod.SkipPaymentInfo => false;

        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        string IPaymentMethod.PaymentMethodDescription => _localizationService.GetResource("Plugins.Payments.MollieByQuims.PaymentMethodDescription");

        /// <summary>
        /// Gets a value indicating whether to hide this plugin on the widget list page in the admin area
        /// </summary>
        public bool HideInWidgetList => true;

        #endregion
    }
}
