using System;
using GovUkDesignSystem.GovUkDesignSystemComponents.SubComponents;

namespace GovUkDesignSystem.GovUkDesignSystemComponents
{
    public class HtmlTextViewModel : IHtmlText
    {
        public Func<object, object> Html { get; set; }
        
        public string Text { get; set; }

    }
}
