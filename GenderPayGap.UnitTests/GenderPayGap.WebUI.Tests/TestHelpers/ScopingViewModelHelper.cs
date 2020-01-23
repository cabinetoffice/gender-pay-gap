using GenderPayGap.WebUI.Models.Scope;
using Moq;

namespace GenderPayGap.WebUI.Tests.TestHelpers
{
    public class ScopingViewModelHelper
    {

        public static ScopingViewModel GetScopingViewModel()
        {
            return Mock.Of<ScopingViewModel>();
        }

    }
}
