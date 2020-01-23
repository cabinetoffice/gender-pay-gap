using System;
using System.Collections.Generic;
using GenderPayGap.Core.Interfaces.Downloadable;
using GenderPayGap.Extensions;

namespace GenderPayGap.Core.Classes.Downloadable
{
    public class DownloadableDirectory : IDownloadableDirectory
    {

        private string _name;

        public DownloadableDirectory(string filePath)
        {
            Filepath = filePath;
        }

        public List<IDownloadableItem> DownloadableItems { get; set; }

        public string Filepath { get; set; }

        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_name))
                {
                    _name = Filepath.AfterLastAny("\\/", StringComparison.Ordinal);
                }

                return _name;
            }

            set => _name = value;
        }

        /// <summary>
        ///     Generates the two dots 'parent' special folder for a given directory
        /// </summary>
        /// <param name="directoryPath"></param>
        public static DownloadableDirectory GetSpecialParentFolderInfo(string directoryPath)
        {
            int lastPositionOfSlash = directoryPath.LastIndexOfAny(new[] {'\\', '/'});
            string parentDirectoryPath = directoryPath.Remove(lastPositionOfSlash);

            var specialFolderFileInfo = new DownloadableDirectory(parentDirectoryPath) {Name = ".."};

            return specialFolderFileInfo;
        }

    }
}
