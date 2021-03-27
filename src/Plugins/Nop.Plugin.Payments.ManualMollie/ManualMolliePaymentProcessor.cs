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
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.ManualMollie.Models;
using Nop.Plugin.Payments.ManualMollie.Validators;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Services.Plugins;

namespace Nop.Plugin.Payments.ManualMollie
{
    /// <summary>
    /// Manual payment processor
    /// </summary>
    public class ManualMolliePaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IPaymentService _paymentService;
        private readonly ISettingService _settingService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHelper _webHelper;
        private readonly ManualMolliePaymentSettings _manualMolliePaymentSettings;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public ManualMolliePaymentProcessor(ILocalizationService localizationService,
            IPaymentService paymentService,
            ISettingService settingService,
            IWebHelper webHelper,
            IHttpContextAccessor httpContextAccessor,
            IGenericAttributeService genericAttributeService,
            IStoreContext storeContext,
            ManualMolliePaymentSettings manualMolliePaymentSettings)
        {
            _localizationService = localizationService;
            _paymentService = paymentService;
            _settingService = settingService;
            _webHelper = webHelper;
            _manualMolliePaymentSettings = manualMolliePaymentSettings;
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
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {

            return new ProcessPaymentResult();

        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            string total = string.Format("{0:0.00}", postProcessPaymentRequest.Order.OrderSubtotalInclTax);
            string url = _manualMolliePaymentSettings.SiteURL + "PaymentManualMollie/Verify";

            IPaymentClient paymentClient = new PaymentClient(_manualMolliePaymentSettings.ApiKey);
            PaymentRequest paymentRequest = new PaymentRequest()
            {
                Amount = new Amount(Currency.EUR, total),
                Description = $"{_storeContext.CurrentStore.Name} - ID: {postProcessPaymentRequest.Order.Id.ToString()}",
                RedirectUrl = url
            };

            PaymentResponse paymentResponse = paymentClient.CreatePaymentAsync(paymentRequest).GetAwaiter().GetResult();

            // Write info to repository 
            Identifier identifier = new Identifier();
            identifier.MollieInfo = paymentResponse;
            identifier.OrderInfo = postProcessPaymentRequest.Order;
            Repository.AddInfo(identifier);

            // Redirect to Mollie
            _httpContextAccessor.HttpContext.Response.Redirect(paymentResponse.Links.Checkout.Href);

            return;
        } 

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return false;
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            // No fee 
            return _paymentService.CalculateAdditionalFee(cart,
                0, false);
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            return new CapturePaymentResult { Errors = new[] { "Capture method not supported" } };
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            return new RefundPaymentResult { Errors = new[] { "Refund method not supported" } };
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            return new VoidPaymentResult { Errors = new[] { "Void method not supported" } };
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            //var result = new ProcessPaymentResult
            //{
            //    AllowStoringCreditCardNumber = true
            //};
            //switch (_manualMolliePaymentSettings.TransactMode)
            //{
            //    case TransactMode.Pending:
            //        result.NewPaymentStatus = PaymentStatus.Pending;
            //        break;
            //    case TransactMode.Authorize:
            //        result.NewPaymentStatus = PaymentStatus.Authorized;
            //        break;
            //    case TransactMode.AuthorizeAndCapture:
            //        result.NewPaymentStatus = PaymentStatus.Paid;
            //        break;
            //    default:
            //        result.AddError("Not supported transaction type");
            //        break;
            //}

            //return result;

            return new ProcessPaymentResult { Errors = new[] { "Recurring payment not supported" } };
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            ////always success
            //return new CancelRecurringPaymentResult();

            return new CancelRecurringPaymentResult { Errors = new[] { "Recurring payment not supported" } };
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            //if (order == null)
            //    throw new ArgumentNullException(nameof(order));

            ////it's not a redirection payment method. So we always return false
            //return false;

            if (order == null)
                throw new ArgumentNullException(nameof(order));

            //let's ensure that at least 5 seconds passed after order is placed
            //P.S. there's no any particular reason for that. we just do it
            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalSeconds < 5)
                return false;

            return true;
        }

        /// <summary>
        /// Validate payment form
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>List of validating errors</returns>
        public IList<string> ValidatePaymentForm(IFormCollection form)
        {
            //var warnings = new List<string>();

            ////validate
            //var validator = new PaymentInfoValidator(_localizationService);
            //var model = new PaymentInfoModel
            //{
            //    CardholderName = form["CardholderName"],
            //    CardNumber = form["CardNumber"],
            //    CardCode = form["CardCode"],
            //    ExpireMonth = form["ExpireMonth"],
            //    ExpireYear = form["ExpireYear"]
            //};
            //var validationResult = validator.Validate(model);
            //if (!validationResult.IsValid)
            //    warnings.AddRange(validationResult.Errors.Select(error => error.ErrorMessage));

            //return warnings;

            return new List<string>();
        }

        /// <summary>
        /// Get payment information
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>Payment info holder</returns>
        public ProcessPaymentRequest GetPaymentInfo(IFormCollection form)
        {
            //return new ProcessPaymentRequest
            //{
            //    CreditCardType = form["CreditCardType"],
            //    CreditCardName = form["CardholderName"],
            //    CreditCardNumber = form["CardNumber"],
            //    CreditCardExpireMonth = int.Parse(form["ExpireMonth"]),
            //    CreditCardExpireYear = int.Parse(form["ExpireYear"]),
            //    CreditCardCvv2 = form["CardCode"]
            //};

            return new ProcessPaymentRequest();
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentManualMollie/Configure";
        }

        /// <summary>
        /// Gets a name of a view component for displaying plugin in public store ("payment info" checkout step)
        /// </summary>
        /// <returns>View component name</returns>
        public string GetPublicViewComponentName()
        {
            return "PaymentManualMollie";
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            //settings
            var settings = new ManualMolliePaymentSettings
            {
                // nothing
            };
            _settingService.SaveSetting(settings);

            //locales
            _localizationService.AddLocaleResource(new Dictionary<string, string>
            {
                ["Plugins.Payments.ManualMollie.Instructions"] = "This payment method stores credit card information in database (it's not sent to any third-party processor). In order to store credit card information, you must be PCI compliant.",
                ["Plugins.Payments.ManualMollie.Fields.AdditionalFee"] = "Additional fee",
                ["Plugins.Payments.ManualMollie.Fields.AdditionalFee.Hint"] = "Enter additional fee to charge your customers.",
                ["Plugins.Payments.ManualMollie.Fields.AdditionalFeePercentage"] = "Additional fee. Use percentage",
                ["Plugins.Payments.ManualMollie.Fields.AdditionalFeePercentage.Hint"] = "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.",
                ["Plugins.Payments.ManualMollie.Fields.TransactMode"] = "After checkout mark payment as",
                ["Plugins.Payments.ManualMollie.Fields.TransactMode.Hint"] = "Specify transaction mode.",
                ["Plugins.Payments.ManualMollie.PaymentMethodDescription"] = "Pay by credit / debit card"
            });

            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<ManualMolliePaymentSettings>();

            //locales
            _localizationService.DeleteLocaleResources("Plugins.Payments.ManualMollie");

            base.Uninstall();
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
        public string PaymentMethodDescription => _localizationService.GetResource("Plugins.Payments.ManualMollie.PaymentMethodDescription");

        #endregion

    }
}