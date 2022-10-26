namespace GenderPayGap.WebUI.Views.Components.PaginationLinks
{
    public class GpgPaginationLinksViewModel
    {
        public GpgPaginationLinkViewModel LeftLink { get; set; }
        public GpgPaginationLinkViewModel RightLink { get; set; }
    }

    public class GpgPaginationLinkViewModel
    {

        /// <summary>
        ///     The direction of the arrow.
        ///     Automatically set by PaginationLinks.cshtml if you are using this as the LeftLink or RightLink on a GpgPaginationLinksViewModel
        /// </summary>
        public ArrowDirection ArrowDirection { get; set; }

        /// <summary>
        ///     The title for the pagination link.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        ///     The text for the pagination link.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        ///     The url for the pagination link.
        /// </summary>
        public string Url { get; set; }

    }
    
    public enum ArrowDirection
    {
        Left = 0,
        Right = 1,
    }
}

