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
using System.Globalization;

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

        // This presumes that weeks start with Monday.
        // Week 1 is the 1st week of the year with a Thursday in it.
        public static int GetIso8601WeekOfYear(DateTime time)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var internetInformationStatsSettings = await _settingService.LoadSettingAsync<InternetInformationStatsSettings>((await _storeContext.GetCurrentStoreAsync()).Id);

            var model = new PublicInfoModel
            {
                VisitorsToday = 0,
                OnlineVisitors = 44
            };

            internetInformationStatsSettings.FileLocation = "C:\\Users\\piete\\Source\\Repos\\NopCommerceMerge\\nopCommerce\\src\\Plugins\\Nop.Plugin.Widgets.InternetInformationStats\\EXAMPLE FILES\\u_ex210505.log";
            List<IISLogEvent> logs = new List<IISLogEvent>();
            List<IISLogEvent> logsOfToday = new List<IISLogEvent>();
            List<IISLogEvent> uniqueVisitors = new List<IISLogEvent>();

            using (ParserEngine parser = new ParserEngine(internetInformationStatsSettings.FileLocation))
            {
                while (parser.MissingRecords)
                {
                    logs = parser.ParseLog().ToList();
                }
            }

            // DEBUGGING PURPOSES
            var firstDate = logs[0].DateTimeEvent.Date;
            // DEBUGGING PURPOSES

            foreach (var l in logs)
            {
                // Add when list is empty
                if (uniqueVisitors.Count == 0)
                {
                    uniqueVisitors.Add(l);
                }
                // Add unique items
                else
                {
                    foreach(var u in uniqueVisitors)
                    {
                        if (u.cIp == l.cIp)
                        {
                            break;
                        }
                        else
                        {
                            uniqueVisitors.Add(l);
                            break;
                        }
                    }
                }

            }

            foreach (var l in uniqueVisitors)
            {
                // Today
                if (l.DateTimeEvent.Date == firstDate)
                {
                    model.OnlineVisitors++;
                }

                // This week
                if (GetIso8601WeekOfYear(l.DateTimeEvent.Date) == GetIso8601WeekOfYear(firstDate))
                {
                    model.VisitorsThisMonth++;
                }

                // write to db 

            }

            return View("~/Plugins/Widgets.InternetInformationStats/Views/PublicInfo.cshtml", model);
        }
    }
}
