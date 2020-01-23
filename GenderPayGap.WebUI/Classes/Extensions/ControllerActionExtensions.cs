using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace GenderPayGap.WebUI.Classes
{

    public static class ControllerActionExtensions
    {

        #region RedirectToAction Helpers

        public static IActionResult RedirectToAction<TDestController>(this Controller controller, string actionName)
            where TDestController : Controller
        {
            RedirectToActionResult result = controller.RedirectToAction(actionName, GetControllerFriendlyName<TDestController>());

            AreaAttribute areaAttr = GetControllerArea<TDestController>();
            if (areaAttr != null)
            {
                result.RouteValues = new RouteValueDictionary();
                result.RouteValues.Add(areaAttr.RouteKey, areaAttr.RouteValue);
            }

            return result;
        }

        #endregion

        private static string GetControllerFriendlyName<TController>() where TController : Controller
        {
            Type controllerType = typeof(TController);
            string controllerName = controllerType.Name;
            return controllerName.Remove(controllerName.LastIndexOf(nameof(Controller)));
        }

        private static AreaAttribute GetControllerArea<TController>() where TController : Controller
        {
            return typeof(TController)
                .GetCustomAttributes(typeof(AreaAttribute), false)
                .FirstOrDefault() as AreaAttribute;
        }

        #region Action Helpers

        public static string Action<TDestController>(this IUrlHelper helper, string action, object values, string protocol)
            where TDestController : Controller
        {
            var routeValues = new RouteValueDictionary(values);
            AreaAttribute areaAttr = GetControllerArea<TDestController>();
            if (areaAttr != null)
            {
                routeValues.Add(areaAttr.RouteKey, areaAttr.RouteValue);
            }

            return helper.Action(action, GetControllerFriendlyName<TDestController>(), routeValues, protocol);
        }

        public static string Action<TDestController>(this IUrlHelper helper, string action, object values)
            where TDestController : Controller
        {
            return helper.Action<TDestController>(action, new RouteValueDictionary(values), "https");
        }

        public static string Action<TDestController>(this IUrlHelper helper, string action)
            where TDestController : Controller
        {
            return helper.Action<TDestController>(action, null);
        }

        #endregion

    }

}
