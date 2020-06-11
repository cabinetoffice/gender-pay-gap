using System;
using System.Collections.Generic;

namespace GenderPayGap.WebUI.Models.Search
{
    [Serializable]
    public class DownloadViewModel
    {

        public List<Download> Downloads { get; set; }

        public class Download
        {

            public string Title { get; set; }
            public string Url { get; set; }

        }

    }
}
