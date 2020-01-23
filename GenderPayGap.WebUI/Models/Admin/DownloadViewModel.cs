using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Xml.Serialization;
using GenderPayGap.Extensions;
using Newtonsoft.Json;

namespace GenderPayGap.WebUI.Models.Admin
{
    [Serializable]
    public class DownloadViewModel
    {

        public List<Download> Downloads { get; set; } = new List<Download>();

        [Serializable]
        public class Download
        {

            public string Type { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }

            public string Filename => Path.GetFileName(Filepath);

            public string Filepath { get; set; }
            public string EhrcAccessKey { get; set; }

            public DateTime? Modified { get; set; }
            public string Count { get; set; }

            public string ContentType
            {
                get
                {
                    if (Filepath.EndsWithI(".csv"))
                    {
                        return "text/csv";
                    }

                    if (Filepath.EndsWithI(".txt", ".log"))
                    {
                        return "text/plain";
                    }

                    return FileSystem.GetMimeMapping(Filename);
                }
            }

            public string Content { get; set; }

            [XmlIgnore]
            [JsonIgnore]
            public DataTable Datatable { get; set; }

            public bool ShowUpdateButton { get; set; }

        }

    }
}
