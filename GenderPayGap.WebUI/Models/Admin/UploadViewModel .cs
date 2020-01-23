using System;
using System.Collections.Generic;
using System.IO;

namespace GenderPayGap.WebUI.Models.Admin
{
    [Serializable]
    public class UploadViewModel
    {

        public List<Upload> Uploads { get; set; } = new List<Upload>();

        [Serializable]
        public class Upload
        {

            public string Type { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }

            public string Filename => Path.GetFileName(Filepath);

            public string Filepath { get; set; }
            public DateTime Modified { get; set; }

            public string Count { get; set; }

        }

    }
}
