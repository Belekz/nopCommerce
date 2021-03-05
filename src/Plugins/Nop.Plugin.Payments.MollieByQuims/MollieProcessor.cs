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
        public ProcessPaymentResult IPaymentMethod.ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            // get ID

            CreatePaymentModel model = new CreatePaymentModel()
            {
                Amount = processPaymentRequest.OrderTotal,
                Description = processPaymentRequest.CustomerId.ToString(),
                // So far all payments only in EUR
                Currency = Currency.EUR 
            };

             _paymentStorageClient.Create(model);
            var list = _paymentOverviewClient.GetList();


            //authorize or capture the order
            var (order, error) = _settings.PaymentType == PaymentType.Capture
                ? _serviceManager.Capture(_settings, orderId.ToString())
                : (_settings.PaymentType == PaymentType.Authorize
                ? _serviceManager.Authorize(_settings, orderId.ToString())
                : (default, default));


            if (!string.IsNullOrEmpty(error))
                return new ProcessPaymentResult { Errors = new[] { error } };

            //request succeeded
            var result = new ProcessPaymentResult();

            var purchaseUnit = order.PurchaseUnits.FirstOrDefault(item => item.ReferenceId.Equals(processPaymentRequest.OrderGuid.ToString()));
            var authorization = purchaseUnit.Payments?.Authorizations?.FirstOrDefault();
            if (authorization != null)
            {
                result.AuthorizationTransactionId = authorization.Id;
                result.AuthorizationTransactionResult = authorization.Status;
                result.NewPaymentStatus = PaymentStatus.Authorized;
            }
            var capture = purchaseUnit.Payments?.Captures?.FirstOrDefault();
            if (capture != null)
            {
                result.CaptureTransactionId = capture.Id;
                result.CaptureTransactionResult = capture.Status;
                result.NewPaymentStatus = PaymentStatus.Paid;
            }

            return result;

        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {

        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            //capture previously authorized payment
            var (capture, error) = _serviceManager
                .CaptureAuthorization(_settings, capturePaymentRequest.Order.AuthorizationTransactionId);

            if (!string.IsNullOrEmpty(error))
                return new CapturePaymentResult { Errors = new[] { error } };

            //request succeeded
            return new CapturePaymentResult
            {
                CaptureTransactionId = capture.Id,
                CaptureTransactionResult = capture.Status,
                NewPaymentStatus = PaymentStatus.Paid
            };
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            //void previously authorized payment
            var (_, error) = _serviceManager.Void(_settings, voidPaymentRequest.Order.AuthorizationTransactionId);

            if (!string.IsNullOrEmpty(error))
                return new VoidPaymentResult { Errors = new[] { error } };

            //request succeeded
            return new VoidPaymentResult { NewPaymentStatus = PaymentStatus.Voided };
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            //refund previously captured payment
            var amount = refundPaymentRequest.AmountToRefund != refundPaymentRequest.Order.OrderTotal
                ? (decimal?)refundPaymentRequest.AmountToRefund
                : null;
            var (refund, error) = _serviceManager.Refund(_settings, refundPaymentRequest.Order.CaptureTransactionId,
                refundPaymentRequest.Order.CustomerCurrencyCode, amount);

            if (!string.IsNullOrEmpty(error))
                return new RefundPaymentResult { Errors = new[] { error } };

            //request succeeded
            var refundIds = _genericAttributeService.GetAttribute<List<string>>(refundPaymentRequest.Order, Defaults.RefundIdAttributeName)
                ?? new List<string>();
            if (!refundIds.Contains(refund.Id))
                refundIds.Add(refund.Id);
            _genericAttributeService.SaveAttribute(refundPaymentRequest.Order, Defaults.RefundIdAttributeName, refundIds);
            return new RefundPaymentResult
            {
                NewPaymentStatus = refundPaymentRequest.IsPartialRefund ? PaymentStatus.PartiallyRefunded : PaymentStatus.Refunded
            };
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
        /// Get payment information
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>Payment info holder</returns>
        public ProcessPaymentRequest GetPaymentInfo(IFormCollection form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            //already set
            return _actionContextAccessor.ActionContext.HttpContext.Session.Get<ProcessPaymentRequest>(Defaults.PaymentRequestSessionKey);
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext).RouteUrl(Defaults.ConfigurationRouteName);
        }

        /// <summary>
        /// Gets a view component for displaying plugin in public store ("payment info" checkout step)
        /// </summary>
        /// <param name="viewComponentName">View component name</param>
        public string GetPublicViewComponentName()
        {
            return Defaults.PAYMENT_INFO_VIEW_COMPONENT_NAME;
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
