using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Services.Configuration;
using Nop.Services.Media;
using Nop.Web.Framework.Components;
using Nop.Plugin.Widgets.SideShoppingCart.Models;
using System;
using System.Threading.Tasks;

namespace Nop.Plugin.Widgets.SideShoppingCart.Components
{
    [ViewComponent(Name = "WidgetsSideShoppingCart")]
    public class CustomViewComponent : NopViewComponent
    {
        private readonly IStoreContext _storeContext;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly ISettingService _settingService;
        private readonly IPictureService _pictureService;
        private readonly IWebHelper _webHelper;

        public CustomViewComponent(IStoreContext storeContext, IStaticCacheManager staticCacheManager, 
            ISettingService settingService, IPictureService pictureService, IWebHelper webHelper)
        {
            _storeContext = storeContext;
            _staticCacheManager = staticCacheManager;
            _settingService = settingService;
            _pictureService = pictureService;
            _webHelper = webHelper;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var nivoSliderSettings = await _settingService.LoadSettingAsync<SideShoppingCartSettings>((await _storeContext.GetCurrentStoreAsync()).Id);

            var model = new PublicInfoModel
            {
                Item = "test item 1"
            };

            return View("~/Plugins/Widgets.SideShoppingCart/Views/PublicInfo.cshtml", model);
        }
    }
}
