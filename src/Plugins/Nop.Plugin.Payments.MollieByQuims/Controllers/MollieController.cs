using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Mollie.Api;
using Nop.Plugin.Payments.MollieByQuims.Models;
using Nop.Plugin.Payments.MollieByQuims.Domain;

namespace Nop.Plugin.Payments.MollieByQuims.Controllers
{
    [Area(AreaNames.Admin)]
    [AutoValidateAntiforgeryToken]
    [ValidateIpAddress]
    [AuthorizeAdmin]
    [ValidateVendor]

    class MollieController : BasePaymentController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;
        private readonly ShoppingCartSettings _shoppingCartSettings;

        #endregion

        #region Ctor

        public MollieController(ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext,
            IWebHelper webHelper,
            ShoppingCartSettings shoppingCartSettings)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
            _webHelper = webHelper;
            _shoppingCartSettings = shoppingCartSettings;
        }

        #endregion

        #region Methodes

        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var settings = _settingService.LoadSetting<MollieByQuimsSettings>(storeScope);

            //prepare model
            var model = new ConfigurationModel
            {
                ApiKey = settings.ApiKey,
                PaymentTypeId = (int)settings.PaymentType,
                DisplayButtonsOnShoppingCart = settings.DisplayButtonsOnShoppingCart,
                DisplayButtonsOnProductDetails = settings.DisplayButtonsOnProductDetails,
                DisplayLogoInHeaderLinks = settings.DisplayLogoInHeaderLinks,
                LogoInHeaderLinks = settings.LogoInHeaderLinks,
                DisplayLogoInFooter = settings.DisplayLogoInFooter,
                LogoInFooter = settings.LogoInFooter,
                ActiveStoreScopeConfiguration = storeScope
            };

            if (storeScope > 0)
            {
                model.ApiKey_OverrideForStore = _settingService.SettingExists(settings, setting => setting.ApiKey, storeScope);
                model.PaymentTypeId_OverrideForStore = _settingService.SettingExists(settings, setting => setting.PaymentType, storeScope);
                model.DisplayButtonsOnShoppingCart_OverrideForStore = _settingService.SettingExists(settings, setting => setting.DisplayButtonsOnShoppingCart, storeScope);
                model.DisplayButtonsOnProductDetails_OverrideForStore = _settingService.SettingExists(settings, setting => setting.DisplayButtonsOnProductDetails, storeScope);
                model.DisplayLogoInHeaderLinks_OverrideForStore = _settingService.SettingExists(settings, setting => setting.DisplayLogoInHeaderLinks, storeScope);
                model.LogoInHeaderLinks_OverrideForStore = _settingService.SettingExists(settings, setting => setting.LogoInHeaderLinks, storeScope);
                model.DisplayLogoInFooter_OverrideForStore = _settingService.SettingExists(settings, setting => setting.DisplayLogoInFooter, storeScope);
                model.LogoInFooter_OverrideForStore = _settingService.SettingExists(settings, setting => setting.LogoInFooter, storeScope);
            }

            //prepare available payment types
            model.PaymentTypes = PaymentType.Payment.ToSelectList(false)
                .Select(item => new SelectListItem(item.Text, item.Value))
                .ToList();

            //prices and total aren't rounded, so display warning
            if (!_shoppingCartSettings.RoundPricesDuringCalculation)
            {
                var url = Url.Action("AllSettings", "Setting", new { settingName = nameof(ShoppingCartSettings.RoundPricesDuringCalculation) });
                var warning = string.Format(_localizationService.GetResource("Plugins.Payments.MollieByQuims.RoundingWarning"), url);
                _notificationService.WarningNotification(warning, false);
            }

            return View("~/Plugins/Payments.MollieByQuims/Views/Configure.cshtml", model);
        }

        #endregion
    }
}
