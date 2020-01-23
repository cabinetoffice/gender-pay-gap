using System.Collections.Generic;

namespace GenderPayGap.Core.Interfaces.Downloadable
{
    public interface IDownloadableDirectory : IDownloadableItem
    {

        List<IDownloadableItem> DownloadableItems { get; }

    }
}
