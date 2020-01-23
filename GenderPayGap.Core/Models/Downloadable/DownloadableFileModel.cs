using System.Data;
using System.IO;
using System.Text;
using GenderPayGap.Core.Classes;
using GenderPayGap.Extensions;

namespace GenderPayGap.Core.Models.Downloadable
{
    public class DownloadableFileModel
    {

        public DownloadableFileModel(string filePath)
        {
            Filepath = filePath;
        }

        public string Filename => Path.GetFileName(Filepath);

        public string Filepath { get; internal set; }

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

        public byte[] ByteArrayContent => Encoding.ASCII.GetBytes(CsvContent);

        private string CsvContent => DataTable.ToCSV();

        public DataTable DataTable { get; set; }
        public string Name { get; set; }

    }
}
