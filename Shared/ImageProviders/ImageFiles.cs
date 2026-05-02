using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ClaudiaIDE.ImageProviders
{
    public class ImageFiles : IEnumerable<string>
    {
        public string Extensions { get; set; }
        public string ImageDirectoryPath { get; set; }
        public bool Shuffle { get; set; }
        public bool IncludeSubdirectories { get; set; }

        private static DateTime LastEnumerationTime = DateTime.MinValue;
        private static List<string> LastImages;

        public IEnumerator<string> GetEnumerator()
        {
            // to avoid multiple, repeated enumerations of the images due to settings changes or other events, check
            // whether this method has been called in the last 2 seconds
            if (LastImages != null && DateTime.Now - LastEnumerationTime < TimeSpan.FromSeconds(2))
            {
                return new ImageFilesEnumerator(LastImages);
            }
            LastEnumerationTime = DateTime.Now;

            List<string> imageFilePaths;

            if (!string.IsNullOrEmpty(Extensions) && !string.IsNullOrEmpty(ImageDirectoryPath) && Directory.Exists(ImageDirectoryPath))
            {
                var extensions = Extensions
                    .Split(new[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.ToUpper());

                var searchOption = IncludeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

                imageFilePaths = Directory
                    .EnumerateFiles(new DirectoryInfo(ImageDirectoryPath).FullName, "*.*", searchOption)
                    .Where(x => extensions.Contains(Path.GetExtension(x).ToUpper())).OrderBy(x => x).ToList();

                if (imageFilePaths.Any())
                {
                    if (Shuffle) imageFilePaths.Shuffle();
                }
            }
            else
            {
                imageFilePaths = new List<string>();
            }

            LastImages = imageFilePaths;
            return new ImageFilesEnumerator(LastImages);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class ImageFilesEnumerator : IEnumerator<string>
    {
        private readonly List<string> imageFilePaths;
        private int position;

        public ImageFilesEnumerator(List<string> imageFilePaths)
        {
            this.imageFilePaths = imageFilePaths;
            position = -1;
        }

        public string Current => position < 0 || position > imageFilePaths.Count ? null : imageFilePaths[position];

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            position++;
            return position < imageFilePaths.Count;
        }

        public void Reset()
        {
            position = -1;
        }
    }

}
