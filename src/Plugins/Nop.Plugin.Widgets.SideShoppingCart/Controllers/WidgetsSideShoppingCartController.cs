using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Widgets.SideShoppingCart.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Widgets.SideShoppingCart.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    [AutoValidateAntiforgeryToken]
    public class WidgetsSideShoppingCartController : BasePluginController
    {
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        public WidgetsSideShoppingCartController(ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService, 
            IPictureService pictureService,
            ISettingService settingService,
            IStoreContext storeContext)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _pictureService = pictureService;
            _settingService = settingService;
            _storeContext = storeContext;
        }

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var sideShoppingCartSettings = await _settingService.LoadSettingAsync<SideShoppingCartSettings>(storeScope);
            var model = new ConfigurationModel
            {
                MaximumShoppingCartItems = sideShoppingCartSettings.MaximumShoppingCartItems,
                ShowProductImagesOnShoppingCart = sideShoppingCartSettings.ShowProductImagesOnShoppingCart,
                HideCheckoutButton = sideShoppingCartSettings.HideCheckoutButton,
                ActiveStoreScopeConfiguration = storeScope
            };

            if (storeScope > 0)
            {
                model.MaximumShoppingCartItems_OverrideForStore = await _settingService.SettingExistsAsync(sideShoppingCartSettings, x => x.MaximumShoppingCartItems, storeScope);
                model.ShowProductImagesOnShoppingCart_OverrideForStore = await _settingService.SettingExistsAsync(sideShoppingCartSettings, x => x.ShowProductImagesOnShoppingCart, storeScope);
                model.HideCheckoutButton_OverrideForStore = await _settingService.SettingExistsAsync(sideShoppingCartSettings, x => x.HideCheckoutButton, storeScope);
            }

            return View("~/Plugins/Widgets.SideShoppingCart/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var sideShoppingCartSettings = await _settingService.LoadSettingAsync<SideShoppingCartSettings>(storeScope);

            sideShoppingCartSettings.MaximumShoppingCartItems = model.MaximumShoppingCartItems;
            sideShoppingCartSettings.ShowProductImagesOnShoppingCart = model.ShowProductImagesOnShoppingCart;
            sideShoppingCartSettings.HideCheckoutButton = model.HideCheckoutButton;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            await _settingService.SaveSettingOverridablePerStoreAsync(sideShoppingCartSettings, x => x.MaximumShoppingCartItems, model.MaximumShoppingCartItems_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(sideShoppingCartSettings, x => x.ShowProductImagesOnShoppingCart, model.ShowProductImagesOnShoppingCart_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(sideShoppingCartSettings, x => x.HideCheckoutButton, model.HideCheckoutButton_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));
            
            return await Configure();
        }
    }
}