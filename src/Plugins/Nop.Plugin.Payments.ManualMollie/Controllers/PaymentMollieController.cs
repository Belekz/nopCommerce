using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mollie.Api.Client;
using Mollie.Api.Client.Abstract;
using Mollie.Api.Models.Payment;
using Mollie.Api.Models.Payment.Response;
using Nop.Core;
using Nop.Plugin.Payments.MollieForNop.Models;
using Nop.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Payments.MollieForNop.Controllers
{

    public class PaymentMollieForNopController : BasePaymentController
    {
        #region Fields
        
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IGenericAttributeService _genericAttributeService;

        #endregion

        #region Ctor

        public PaymentMollieForNopController(ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IGenericAttributeService genericAttributeService,
            IStoreContext storeContext)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _genericAttributeService = genericAttributeService;
        }

        #endregion

        #region Methods

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [AutoValidateAntiforgeryToken]

        public async Task<IActionResult> Configure()
        {
            if (! await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var manualPaymentSettings = await _settingService.LoadSettingAsync<MollieForNopPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                ApiKey = manualPaymentSettings.ApiKey,
                SiteURL = manualPaymentSettings.SiteURL,
                ActiveStoreScopeConfiguration = storeScope
            };
            if (storeScope > 0)
            {
                model.Api_OverrideForStore = await _settingService.SettingExistsAsync(manualPaymentSettings, x => x.ApiKey, storeScope);
                model.SiteURL_OverrideForStore = await _settingService.SettingExistsAsync(manualPaymentSettings, x => x.SiteURL, storeScope);
            }

            // When left empty, fill in default URL
            if (model.SiteURL == "")
            {
                var site = await _storeContext.GetCurrentStoreAsync();
                model.SiteURL = site.Url;
            }

            return View("~/Plugins/Payments.MollieForNop/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [AutoValidateAntiforgeryToken]

        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (! await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return await Configure();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var mollieForNopPaymentSettings = await _settingService.LoadSettingAsync<MollieForNopPaymentSettings>(storeScope);

            //save settings
            mollieForNopPaymentSettings.ApiKey = model.ApiKey;
            mollieForNopPaymentSettings.SiteURL = model.SiteURL;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */

            await _settingService.SaveSettingOverridablePerStoreAsync(mollieForNopPaymentSettings, x => x.ApiKey, model.Api_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(mollieForNopPaymentSettings, x => x.SiteURL, model.SiteURL_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        public async Task<IActionResult> Verify()
        {

            // TO DO 

            // 1:  make sure when no base url is present, client gets no error prompted 

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var mollieForNopPaymentSettings = await _settingService.LoadSettingAsync<MollieForNopPaymentSettings>(storeScope);

            // Open client
            IPaymentClient paymentClient = new PaymentClient(mollieForNopPaymentSettings.ApiKey);

            // Set time out count
            int timeout = 0;
            int compare = 999; 

            // Count number of repositories
            int numberOfParallelPayments = Repository.Identifiers.Count();

            // Get first identiefer from repository
            var identifier = Repository.Identifiers.ToArray()[0];

            // Make empty object for result
            Identifier id2 = new Identifier();

            // Check if not more than one orders are being processed at the same time
            if (numberOfParallelPayments > 1)
            {
                // Assign order to the last payed order
                foreach (var i in Repository.Identifiers.ToArray())
                {
                    if (i.MollieForNopInfo.Id != null)
                    {
                        id2 = i;
                        id2.MollieForNopInfo = await paymentClient.GetPaymentAsync(i.MollieForNopInfo.Id);

                        compare = DateTime.Compare(Convert.ToDateTime(identifier.MollieForNopInfo.PaidAt), Convert.ToDateTime(id2.MollieForNopInfo.PaidAt));

                        if (compare == -1)
                        {
                            identifier = i;
                        }

                    }
                    
                }
                
            }

            // Get payment status from MollieForNop
            PaymentResponse result = identifier.MollieForNopInfo;

            // Process status MollieForNop with Nop order
            while (result.Status != Mollie.Api.Models.Payment.PaymentStatus.Paid)
            {
                result = await paymentClient.GetPaymentAsync(identifier.MollieForNopInfo.Id);
                timeout++;

                // Time out
                if (timeout == 10)
                {
                    // Cancel order
                    identifier.OrderInfo.OrderStatus = Core.Domain.Orders.OrderStatus.Cancelled;
                    await _orderService.UpdateOrderAsync(identifier.OrderInfo);

                    // Remove ID from the repository 
                    Repository.Remove(identifier);

                    // Redirect to overview page
                    string url = mollieForNopPaymentSettings.SiteURL + "orderdetails/" + identifier.OrderInfo.Id;
                    return Redirect(url);
                }
            }

            // Redirect to complete or failed page
            if (result.Status == Mollie.Api.Models.Payment.PaymentStatus.Paid)
            {
                identifier.OrderInfo.OrderStatus = Core.Domain.Orders.OrderStatus.Pending;
                identifier.OrderInfo.PaymentStatus = Core.Domain.Payments.PaymentStatus.Paid;
                await _orderService.UpdateOrderAsync(identifier.OrderInfo);

                // Remove ID from the repository 
                Repository.Remove(identifier);

                string url = mollieForNopPaymentSettings.SiteURL + "checkout/completed";
                return Redirect(url);
            }
            else
            {
                // Cancel order
                identifier.OrderInfo.OrderStatus = Core.Domain.Orders.OrderStatus.Cancelled;
                await _orderService.UpdateOrderAsync(identifier.OrderInfo);

                // Remove ID from the repository 
                Repository.Remove(identifier);

                // Redirect to overview page
                string url = mollieForNopPaymentSettings.SiteURL + "orderdetails/" + identifier.OrderInfo.Id;
                return Redirect(url);
            }
        }
         
        #endregion
    }
}