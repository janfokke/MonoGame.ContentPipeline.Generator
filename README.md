# MonoGame.ContentPipeline.Generator
A utility to generate code with the paths to content files.

# Usage

## Run utility

Run `MonoGame.ContentPipeline.Generator` with the following arguments

`--output "\<Full path to output cs file\>"`

`--content "\<Full path to Content.mgcb\>"`

`--namespace "\<The namespace of the content file\>"`

I recommend to run the tool from [VisualStudio external tools](https://docs.microsoft.com/en-us/visualstudio/ide/managing-external-tools?view=vs-2019)

## Example output
```cs
namespace YourNameSpace
{
    public static class ContentPaths
    {
        public static class fonts
        {
            public static string default_font => "fonts/someFont";
        }

        public static class sprites
        {
            public static string player => "sprites/player";
        }
    }
}
```

## Load content
```cs
contentManager.Load<SpriteFont>(ContentPaths.fonts.default_font);
```

