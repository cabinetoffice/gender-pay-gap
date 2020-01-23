using System.Linq;
using GenderPayGap.WebUI.Classes.Attributes;

namespace GenderPayGap.WebUI.Models.Shared
{

    [Partial("Patterns/CheckYourAnswers")]
    public class CheckYourAnswers
    {

        public CheckYourAnswers(params CheckYourAnswer[] items)
        {
            Items = items.Where(x => x != null).ToArray();
        }

        public CheckYourAnswer[] Items { get; set; }

    }

    public class CheckYourAnswer
    {

        public CheckYourAnswer(string name, string value, string valueId = "")
        {
            Name = name;
            Value = value;
            ValueId = valueId;
        }

        public string Name { get; set; }
        public string Value { get; set; }
        public string ValueId { get; set; }

    }

}
