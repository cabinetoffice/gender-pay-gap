using System;

namespace GenderPayGap.WebUI.Classes.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PartialAttribute : Attribute
    {

        public PartialAttribute(string partialPath)
        {
            PartialPath = partialPath;
        }

        public string PartialPath { get; set; }

    }

}
