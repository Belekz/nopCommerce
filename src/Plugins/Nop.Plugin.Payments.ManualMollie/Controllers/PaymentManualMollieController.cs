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
                TransactModeId = Convert.ToInt32(manualPaymentSettings.TransactMode),
                AdditionalFee = manualPaymentSettings.AdditionalFee,
                AdditionalFeePercentage = manualPaymentSettings.AdditionalFeePercentage,
                TransactModeValues = manualPaymentSettings.TransactMode.ToSelectList(),
                ActiveStoreScopeConfiguration = storeScope
            };
            if (storeScope > 0)
            {
                model.TransactModeId_OverrideForStore = _settingService.SettingExists(manualPaymentSettings, x => x.TransactMode, storeScope);
                model.AdditionalFee_OverrideForStore = _settingService.SettingExists(manualPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = _settingService.SettingExists(manualPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
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
            manualMolliePaymentSettings.TransactMode = (TransactMode)model.TransactModeId;
            manualMolliePaymentSettings.AdditionalFee = model.AdditionalFee;
            manualMolliePaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */

            _settingService.SaveSettingOverridablePerStore(manualMolliePaymentSettings, x => x.TransactMode, model.TransactModeId_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(manualMolliePaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(manualMolliePaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            
            //now clear settings cache
            _settingService.ClearCache();

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        public async Task<IActionResult> Verify()
        {

            // TO DO 

            // 2: make client string setting
            // 3: ...


            // Open client
            IPaymentClient paymentClient = new PaymentClient("test_BsMnA5gypddAmp7guP9mAtexVVaC4b");

            // Get first identiefer from repository
            var identifier = Repository.Identifiers.ToArray()[0];

            // Set time out count
            int timeout = 0;

            // Check if not more than one orders are being processed at the same time (this should never happen) 
            if (Repository.Identifiers.Count() > 1)
            {
                foreach (var id in Repository.Identifiers.ToArray())
                {
                    id.OrderInfo.OrderStatus = Core.Domain.Orders.OrderStatus.Cancelled;
                    _orderService.UpdateOrder(id.OrderInfo);
                }

                Repository.Reset();
                return View("~/Plugins/Payments.ManualMollie/Views/Verify.cshtml");
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
                    Repository.Reset();
                    return View("~/Plugins/Payments.ManualMollie/Views/Verify.cshtml");
                }
            }

            // Redirect to complete or failed page
            if (result.Status == Mollie.Api.Models.Payment.PaymentStatus.Paid)
            {
                identifier.OrderInfo.OrderStatus = Core.Domain.Orders.OrderStatus.Pending;
                identifier.OrderInfo.PaymentStatus = Core.Domain.Payments.PaymentStatus.Paid;
                _orderService.UpdateOrder(identifier.OrderInfo);
                Repository.Reset();
                return Redirect("https://localhost:44396/checkout/completed"); 
            }
            else
            {
                identifier.OrderInfo.OrderStatus = Core.Domain.Orders.OrderStatus.Cancelled;
                _orderService.UpdateOrder(identifier.OrderInfo);
                Repository.Reset();
                return View("~/Plugins/Payments.ManualMollie/Views/Verify.cshtml");
            }
        }

        #endregion
    }
}