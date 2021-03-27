using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mollie.Api.Client;
using Mollie.Api.Client.Abstract;
using Mollie.Api.Models.Payment;
using Mollie.Api.Models.Payment.Response;
using Nop.Core;
using Nop.Plugin.Payments.ManualMollie.Models;
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

namespace Nop.Plugin.Payments.ManualMollie.Controllers
{

    public class PaymentManualMollieController : BasePaymentController
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

        public PaymentManualMollieController(ILocalizationService localizationService,
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

        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var manualPaymentSettings = _settingService.LoadSetting<ManualMolliePaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                ApiKey = manualPaymentSettings.ApiKey,
                SiteURL = manualPaymentSettings.SiteURL,
                ActiveStoreScopeConfiguration = storeScope
            };
            if (storeScope > 0)
            {
                model.Api_OverrideForStore = _settingService.SettingExists(manualPaymentSettings, x => x.ApiKey, storeScope);
                model.SiteURL_OverrideForStore = _settingService.SettingExists(manualPaymentSettings, x => x.SiteURL, storeScope);
            }

            // When left empty, fill in default URL
            if (model.SiteURL == "")
            {
                model.SiteURL = _storeContext.CurrentStore.Url;
            }

            return View("~/Plugins/Payments.ManualMollie/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [AutoValidateAntiforgeryToken]

        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var manualMolliePaymentSettings = _settingService.LoadSetting<ManualMolliePaymentSettings>(storeScope);

            //save settings
            manualMolliePaymentSettings.ApiKey = model.ApiKey;
            manualMolliePaymentSettings.SiteURL = model.SiteURL;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */

            _settingService.SaveSettingOverridablePerStore(manualMolliePaymentSettings, x => x.ApiKey, model.Api_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(manualMolliePaymentSettings, x => x.SiteURL, model.SiteURL_OverrideForStore, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        public async Task<IActionResult> Verify()
        {

            // TO DO 

            // 1:  make sure when no base url is present, client gets no error prompted 

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var manualMolliePaymentSettings = _settingService.LoadSetting<ManualMolliePaymentSettings>(storeScope);

            // Open client
            IPaymentClient paymentClient = new PaymentClient(manualMolliePaymentSettings.ApiKey);

            // Set time out count
            int timeout = 0;

            // Count number of repositories
            int numberOfParallelPayments = Repository.Identifiers.Count();

            // Get first identiefer from repository
            var identifier = Repository.Identifiers.ToArray()[0];

            // Make empty object for result
            PaymentResponse id1 = paymentClient.GetPaymentAsync(identifier.MollieInfo.Id).GetAwaiter().GetResult();
            PaymentResponse id2 = new PaymentResponse();

            // Check if not more than one orders are being processed at the same time
            if (numberOfParallelPayments > 1)
            {
                // Assign order to the last payed order
                foreach (var id in Repository.Identifiers.ToArray())
                {
                    id2 = await paymentClient.GetPaymentAsync(id.MollieInfo.Id);
                    int compare = DateTime.Compare(Convert.ToDateTime(id1.PaidAt), Convert.ToDateTime(id2.PaidAt));

                    if (compare >0)
                    {
                        identifier = id;
                    }
                }
                

            }

            // Get payment status from Mollie
            PaymentResponse result = await paymentClient.GetPaymentAsync(identifier.MollieInfo.Id);

            // Process status Mollie with Nop order
            while (result.Status != Mollie.Api.Models.Payment.PaymentStatus.Paid)
            {
                result = await paymentClient.GetPaymentAsync(identifier.MollieInfo.Id);
                timeout++;

                // Time out
                if (timeout == 10)
                {
                    identifier.OrderInfo.OrderStatus = Core.Domain.Orders.OrderStatus.Cancelled;
                    _orderService.UpdateOrder(identifier.OrderInfo);

                    // Clean up repository if only one ID was stored
                    if (Repository.Identifiers.Count() == 1)
                    {
                        Repository.Reset();
                    }

                    return View("~/Plugins/Payments.ManualMollie/Views/Verify.cshtml");
                }
            }

            // Redirect to complete or failed page
            if (result.Status == Mollie.Api.Models.Payment.PaymentStatus.Paid)
            {
                identifier.OrderInfo.OrderStatus = Core.Domain.Orders.OrderStatus.Pending;
                identifier.OrderInfo.PaymentStatus = Core.Domain.Payments.PaymentStatus.Paid;
                _orderService.UpdateOrder(identifier.OrderInfo);

                // Clean up repository if only one ID was stored
                if (Repository.Identifiers.Count() == 1)
                {
                    Repository.Reset();
                }

                string url = manualMolliePaymentSettings.SiteURL + "checkout/completed";
                return Redirect(url);
            }
            else
            {
                identifier.OrderInfo.OrderStatus = Core.Domain.Orders.OrderStatus.Cancelled;
                _orderService.UpdateOrder(identifier.OrderInfo);

                // Clean up repository if only one ID was stored
                if (Repository.Identifiers.Count() == 1)
                {
                    Repository.Reset();
                }

                return View("~/Plugins/Payments.ManualMollie/Views/Verify.cshtml");
            }
        }

        #endregion
    }
}