using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Services.Configuration;
using Nop.Services.Media;
using Nop.Web.Framework.Components;
using Nop.Plugin.Widgets.InternetInformationStats.Models;
using System;
using System.Threading.Tasks;
using Nop.Services.Security;
using Nop.Core.Domain.Orders;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework.Models.DataTables;
using IISLogParser;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Plugin.Widgets.InternetInformationStats.Components
{
    [ViewComponent(Name = "WidgetsInternetInformationStats")]
    public class CustomViewComponent : NopViewComponent
    {
        private readonly IStoreContext _storeContext;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly ISettingService _settingService;
        private readonly IPictureService _pictureService;
        private readonly IWebHelper _webHelper;
        private readonly IPermissionService _permissionService;
        private readonly ICustomerModelFactory _customerModelFactory;

        public CustomViewComponent(IStoreContext storeContext, 
            IStaticCacheManager staticCacheManager, 
            ISettingService settingService, 
            IPictureService pictureService, 
            IWebHelper webHelper,
            IPermissionService permissionService,
            ICustomerModelFactory customerModelFactory)
        {
            _storeContext = storeContext;
            _staticCacheManager = staticCacheManager;
            _settingService = settingService;
            _pictureService = pictureService;
            _webHelper = webHelper;
            _permissionService = permissionService;
            _customerModelFactory = customerModelFactory;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var internetInformationStatsSettings = await _settingService.LoadSettingAsync<InternetInformationStatsSettings>((await _storeContext.GetCurrentStoreAsync()).Id);

            internetInformationStatsSettings.FileLocation = "C:\\Users\\piete\\Source\\Repos\\NopCommerceMerge\nopCommerce\\src\\Plugins\\Nop.Plugin.Widgets.InternetInformationStats\\EXAMPLE FILES\\u_ex210505.log";
            List<IISLogEvent> logs = new List<IISLogEvent>();
            using (ParserEngine parser = new ParserEngine(internetInformationStatsSettings.FileLocation))
            {
                while (parser.MissingRecords)
                {
                    logs = parser.ParseLog().ToList();
                }
            }

            var model = new PublicInfoModel
            {
                NumberOfVisitorsToday = 2,
                NumberOfOnlineVisitors = 44
            };

            return View("~/Plugins/Widgets.InternetInformationStats/Views/PublicInfo.cshtml", model);
        }
    }
}
