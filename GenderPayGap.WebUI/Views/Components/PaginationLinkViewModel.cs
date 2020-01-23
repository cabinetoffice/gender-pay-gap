namespace GenderPayGap.WebUI.Views.Components
{
    public class PaginationLinkViewModel
    {

        /// <summary>
        ///     The direction of the arrow.
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

