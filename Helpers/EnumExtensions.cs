using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace DemoApp.Web.Helpers
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum value)
        {
            var memberInfo = value.GetType().GetMember(value.ToString());
            if (memberInfo.Length == 0) return value.ToString();

            var displayAttribute = memberInfo[0].GetCustomAttribute<DisplayAttribute>();
            return displayAttribute?.GetName() ?? value.ToString();
        }

        public static List<SelectListItem> ToSelectList<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T))
                       .Cast<T>()
                       .Select(e => new SelectListItem
                       {
                           Value = e.ToString(),
                           Text = (e as Enum).GetDisplayName()
                       })
                       .ToList();
        }
    }
}
