using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Nop.Core.Domain.Orders;
using Nop.Services.Cms;
using Nop.Services.Payments;
using Nop.Services.Plugins;

namespace Nop.Plugin.Misc.HelloMollie
{
    public class HelloWorldPlugin : BasePlugin, /*IPaymentMethod,*/ IWidgetPlugin
    {
        #region Widget logic

        /// <summary>
        /// Gets a value indicating whether to hide this plugin on the widget list page in the admin area
        /// </summary>
        public bool HideInWidgetList => false;

        /// <summary>
        /// Gets a name of a view component for displaying widget
        /// </summary>
        /// <param name="widgetZone">Name of the widget zone</param>
        /// <returns>View component name</returns>
        public string GetWidgetViewComponentName(string widgetZone)
        {
            return "HelloMollieWidget";
        }

        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>Widget zones</returns>
        public IList<string> GetWidgetZones()
        {
            return new List<string> { "home_page_before_categories" };
        }

        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            throw new NotImplementedException();
        }

        public override void Install()
        {
            //Logic during installation goes here...

            base.Install();
        }

        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            throw new NotImplementedException();
        }

        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            throw new NotImplementedException();
        }

        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            throw new NotImplementedException();
        }

        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            throw new NotImplementedException();
        }

        public override void Uninstall()
        {
            //Logic during uninstallation goes here...

            base.Uninstall();
        }

        public IList<string> ValidatePaymentForm(IFormCollection form)
        {
            throw new NotImplementedException();
        }

        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Payment Logic

        //public bool SupportCapture => throw new NotImplementedException();

        //public bool SupportPartiallyRefund => throw new NotImplementedException();

        //public bool SupportRefund => throw new NotImplementedException();

        //public bool SupportVoid => throw new NotImplementedException();

        //public RecurringPaymentType RecurringPaymentType => throw new NotImplementedException();

        //public PaymentMethodType PaymentMethodType => throw new NotImplementedException();

        //public bool SkipPaymentInfo => throw new NotImplementedException();

        //public string PaymentMethodDescription => throw new NotImplementedException();

        //public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        //{
        //    throw new NotImplementedException();
        //}

        //public bool CanRePostProcessPayment(Order order)
        //{
        //    throw new NotImplementedException();
        //}

        //public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        //{
        //    throw new NotImplementedException();
        //}

        //public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        //{
        //    throw new NotImplementedException();
        //}

        //public ProcessPaymentRequest GetPaymentInfo(IFormCollection form)
        //{
        //    throw new NotImplementedException();
        //}

        //public string GetPublicViewComponentName()
        //{
        //    throw new NotImplementedException();
        //}

        #endregion

    }
}
