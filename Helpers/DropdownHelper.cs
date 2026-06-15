using Microsoft.AspNetCore.Mvc.Rendering;

namespace DemoApp.Web.Helpers
{
    public static class DropdownHelper
    {
        public static async Task<List<SelectListItem>> GetSelectListWithOptionalInactiveAsync<TDto>(
            Func<Task<List<TDto>>> getActiveFunc,
            Func<Task<TDto?>> getByIdFunc,
            Func<TDto, int> getId,
            Func<TDto, string> getName,
            int? selectedId = null)
            where TDto : class
        {
            var activeItems = await getActiveFunc.Invoke();
            var selectList = activeItems
                .Select(item => new SelectListItem
                {
                    Value = getId(item).ToString(),
                    Text = getName(item)
                })
                .ToList();

            if (selectedId.HasValue && !selectList.Any(i => i.Value == selectedId.Value.ToString()))
            {
                var selectedItem = await getByIdFunc.Invoke();
                if (selectedItem != null)
                {
                    selectList.Add(new SelectListItem
                    {
                        Value = getId(selectedItem).ToString(),
                        Text = getName(selectedItem)
                    });
                }
            }

            return selectList.OrderBy(x => x.Text).ToList();
        }
    }
}