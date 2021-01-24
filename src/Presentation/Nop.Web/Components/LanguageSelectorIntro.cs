using Microsoft.AspNetCore.Mvc;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;

namespace Nop.Web.Components
{
    public class LanguageSelectorIntroViewComponent : NopViewComponent
    {
        private readonly ICommonModelFactory _commonModelFactory;

        public LanguageSelectorIntroViewComponent(ICommonModelFactory commonModelFactory)
        {
            _commonModelFactory = commonModelFactory;
        }

        public IViewComponentResult Invoke()
        {
            var model = _commonModelFactory.PrepareLanguageSelectorModel();

            if (model.AvailableLanguages.Count == 1)
                return Content("");

            return View(model);
        }
    }
}
