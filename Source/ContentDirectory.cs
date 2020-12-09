using System;
using System.Collections.Generic;

namespace MonoGame.ContentPipeline.Generator
{
    internal class ContentDirectory
    {
        public Dictionary<string, ContentDirectory> Directories { get; } =
            new Dictionary<string, ContentDirectory>();
        public List<ContentFile> Files { get; } = new List<ContentFile>();

        public void Insert(Span<string> path, ContentFile contentFile)
        {
            if (path.Length == 0)
            {
                Files.Add(contentFile);
            }
            else
            {
                var directoryName = path[0];
                if (!Directories.TryGetValue(directoryName, out ContentDirectory directory))
                {
                    Directories.Add(directoryName, directory = new ContentDirectory());
                }

                directory.Insert(path[1..], contentFile);
            }
        }
    }
}