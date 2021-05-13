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
using System.IO;
using System.Net;

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
            var store = await _storeContext.GetCurrentStoreAsync();
            PublicInfoModel viewModel = new PublicInfoModel { Data = new StatsModel { Week = new WeekModel() } };
            string ownIP = null;
            List<IISLogEvent> logs = new List<IISLogEvent>();
            List<IISLogEvent> logsOfToday = new List<IISLogEvent>();
            List<IISLogEvent> uniqueVisitors = new List<IISLogEvent>();

            #region Get Own IP
            // Get own IP
            try
            {
                if (store.Url.Contains("localhost"))
                {
                    ownIP = "127.0.0.1";
                }
                else
                {
                    IPHostEntry hostEntry = Dns.GetHostEntry(store.Url);
                    ownIP = hostEntry.AddressList[0].ToString();
                }
            }
            catch (Exception)
            {
            }
            #endregion

            try
            {
                // Get files from file location & sort on last modified
                DirectoryInfo info = new DirectoryInfo(internetInformationStatsSettings.FileLocation);
                FileInfo[] files = info.GetFiles().OrderBy(p => p.LastWriteTime).ToArray();
                List<FileInfo> logFiles = new List<FileInfo>();
                List<string> paths = new List<string>();

                // Filter out the log files
                foreach(FileInfo f in files)
                {
                    if (f.Extension == ".log")
                    {
                        //paths.Add(f.DirectoryName+"/"+f.Name);
                        logFiles.Add(f);
                    }
                }

                if (logFiles.Count == 0)
                {
                    viewModel.ErrorMessage = "No .log files found in given directory";
                }

                // Copy the last modified file
                logFiles.LastOrDefault().CopyTo(internetInformationStatsSettings.FileLocation + "//IIStats.log");

                // Read last modified log file
                using (ParserEngine parser = new ParserEngine(internetInformationStatsSettings.FileLocation + "//IIStats.log"))
                {
                    while (parser.MissingRecords)
                    {
                        logs = parser.ParseLog().ToList();
                    }
                }

                // Delete copy of file
                FileInfo[] copyFiles = info.GetFiles().Where(f => f.Name == "IIStats.log").ToArray();
                copyFiles[0].Delete();

                // DEBUGGING PURPOSES
                //var firstDate = logs[0].DateTimeEvent.Date;
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
                        for(int u = 0; u < uniqueVisitors.Count; u++ )
                        {
                            if (l.cIp == uniqueVisitors[u].cIp)
                            {
                                break;
                            }

                            if (l.cIp == "::1")
                            {
                                break;
                            }

                            if (l.cIp == ownIP)
                            {
                                break;
                            }

                            if (u == uniqueVisitors.Count-1)
                            {
                                uniqueVisitors.Add(l);
                            }
                        }

                    }
                }

                foreach (var l in uniqueVisitors)
                {

                    //Day of week
                    switch (l.DateTimeEvent.DayOfWeek.ToString())
                    {
                        case "Sunday":
                            viewModel.Data.Week.Sunday++;
                            break;
                        case "Monday":
                            viewModel.Data.Week.Monday++;
                            break;
                        case "Tuesday":
                            viewModel.Data.Week.Tuesday++;
                            break;
                        case "Wednesday":
                            viewModel.Data.Week.Wednesday++;
                            break;
                        case "Thursday":
                            viewModel.Data.Week.Thursday++;
                            break;
                        case "Friday":
                            viewModel.Data.Week.Friday++;
                            break;
                        case "Saterday":
                            viewModel.Data.Week.Saterday++;
                            break;
                        default:
                            viewModel.ErrorMessage = "day of week not found";
                            break;
                    }
                }
            }
            catch (Exception error)
            {
                if (viewModel.ErrorMessage == null)
                {
                    viewModel.ErrorMessage = error.ToString();
                }

            }

            return View("~/Plugins/Widgets.InternetInformationStats/Views/PublicInfo.cshtml", viewModel);
        }
    }
}
