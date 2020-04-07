namespace GenderPayGap.WebUI.Views.Components.PaginationLinks
{
    public class PaginationLinksViewModel
    {
        public PaginationLinkViewModel LeftLink { get; set; }
        public PaginationLinkViewModel RightLink { get; set; }
    }

    public class PaginationLinkViewModel
    {

        /// <summary>
        ///     The direction of the arrow.
        ///     Automatically set by PaginationLinks.cshtml if you are using this as the LeftLink or RightLink on a PaginationLinksViewModel
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

