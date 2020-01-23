using System.Linq;
using GenderPayGap.WebUI.Classes.Attributes;

namespace GenderPayGap.WebUI.Models.Shared
{

    [Partial("Patterns/Pagination")]
    public class Pagination
    {

        public Pagination(params PaginationLink[] items)
        {
            Items = items.Where(x => x != null).ToArray();
        }

        public PaginationLink[] Items { get; set; }

    }

    public class PaginationLink
    {

        public PaginationLink(string url, string title, string label, bool isPrev = false)
        {
            Url = url;
            Label = label;
            IsPrevious = isPrev;
            if (!string.IsNullOrWhiteSpace(title))
            {
                Title = title;
            }
            else if (isPrev)
            {
                Title = "Previous";
            }
            else
            {
                Title = "Next";
            }
        }

        public bool IsPrevious { get; set; }

        public string Url { get; set; }

        public string Title { get; set; }
        public string Label { get; set; }

    }

}
