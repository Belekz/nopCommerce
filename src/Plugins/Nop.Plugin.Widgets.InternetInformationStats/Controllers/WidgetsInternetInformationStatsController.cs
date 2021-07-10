using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Widgets.InternetInformationStats.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Widgets.InternetInformationStats.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    [AutoValidateAntiforgeryToken]
    public class WidgetsInternetInformationStatsController : BasePluginController
    {
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        public WidgetsInternetInformationStatsController(ILocalizationService localizationService,
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
            var internetInformationStatsSettings = await _settingService.LoadSettingAsync<InternetInformationStatsSettings>(storeScope);
            var model = new ConfigurationModel
            {
                FileLocation = internetInformationStatsSettings.FileLocation,
                CultureInfo = internetInformationStatsSettings.CultureInfo,
                HideCheckoutButton = internetInformationStatsSettings.HideCheckoutButton,
                ActiveStoreScopeConfiguration = storeScope
            };

            if (storeScope > 0)
            {
                model.FileLocation_OverrideForStore = await _settingService.SettingExistsAsync(internetInformationStatsSettings, x => x.FileLocation, storeScope);
                model.CultureInfo_OverrideForStore = await _settingService.SettingExistsAsync(internetInformationStatsSettings, x => x.CultureInfo, storeScope);
                model.HideCheckoutButton_OverrideForStore = await _settingService.SettingExistsAsync(internetInformationStatsSettings, x => x.HideCheckoutButton, storeScope);
            }

            return View("~/Plugins/Widgets.InternetInformationStats/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var internetInformationStatsSettings = await _settingService.LoadSettingAsync<InternetInformationStatsSettings>(storeScope);

            internetInformationStatsSettings.FileLocation = model.FileLocation;
            internetInformationStatsSettings.CultureInfo = model.CultureInfo;
            internetInformationStatsSettings.HideCheckoutButton = model.HideCheckoutButton;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            await _settingService.SaveSettingOverridablePerStoreAsync(internetInformationStatsSettings, x => x.FileLocation, model.FileLocation_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(internetInformationStatsSettings, x => x.CultureInfo, model.CultureInfo_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(internetInformationStatsSettings, x => x.HideCheckoutButton, model.HideCheckoutButton_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));
            
            return await Configure();
        }
    }
}