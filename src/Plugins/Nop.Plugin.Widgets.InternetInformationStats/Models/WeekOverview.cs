using Nop.Web.Framework.Models;
using Nop.Web.Models.ShoppingCart;
using System;
using System.Linq;

namespace Nop.Plugin.Widgets.InternetInformationStats.Models
{
    public record WeekModel : BaseNopModel
    {
        public int Monday { get; set; }
        public int Tuesday { get; set; }
        public int Wednesday { get; set; }
        public int Thursday { get; set; }
        public int Friday { get; set; }
        public int Saterday { get; set; }
        public int Sunday { get; set; }

        // Functions
        public int GetVisitorsToday()
        {
            switch (DateTime.Today.DayOfWeek.ToString())
            {
                case "Sunday":
                    return Sunday;
                case "Monday":
                    return Monday;
                case "Tuesday":
                    return Tuesday;
                case "Wednesday":
                    return Wednesday;
                case "Thursday":
                    return Thursday;
                case "Friday":
                    return Friday;
                case "Saterday":
                    return Saterday;
                default:
                    return 0;
            }
        }
        public int GetVisitorsThisWeek()
        {
            return Sunday + Monday + Tuesday + Wednesday + Thursday + Friday + Saterday;
        }
    }

}