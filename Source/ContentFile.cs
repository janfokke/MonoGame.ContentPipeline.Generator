namespace MonoGame.ContentPipeline.Generator
{
    internal class ContentFile
    {
        public string Name { get; }
        public string FullPath { get; }

        public ContentFile(string name, string fullPath)
        {
            Name = name;
            FullPath = fullPath;
        }
    }
}