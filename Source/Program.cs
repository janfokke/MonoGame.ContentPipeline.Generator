using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MonoGame.ContentPipeline.Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            var rootCommand = new RootCommand();
            rootCommand.AddOption(new Option<string>("--output")
                {IsRequired = true, Description = "Path of the output csharp file"});
            rootCommand.AddOption(new Option<string>("--content")
                {IsRequired = true, Description = "Path of the content root folder"});
            rootCommand.AddOption(new Option<string>("--namespace")
                {IsRequired = true, Description = "Root namespace of the generated content file"});
            rootCommand.Handler = CommandHandler.Create<string, string, string>(Run);
            rootCommand.Invoke(args);
        }

        private static void Run(string output, string content, string @namespace)
        {
            var outputName = Path.GetFileNameWithoutExtension(output);
            var contentProject = File.ReadAllText(content);
            var root = new ContentDirectory();

            MatchCollection matches = Regex.Matches(contentProject, "#begin (?<path>.+)\\..+\\n");
            foreach (Match match in matches)
            {
                var fullPath = match.Groups["path"].Value;
                string[] split = fullPath.Split("/");
                var file = split[^1];
                string[] directories = split[..^1];
                root.Insert(directories, new ContentFile(file, fullPath));
            }

            ClassDeclarationSyntax rootClass = SyntaxFactory.ClassDeclaration(outputName)
                .WithModifiers(
                    SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                        SyntaxFactory.Token(SyntaxKind.StaticKeyword)));

            ClassDeclarationSyntax Recurse(ContentDirectory directory, ClassDeclarationSyntax nestedClass)
            {
                foreach (KeyValuePair<string, ContentDirectory> keyValuePair in directory.Directories)
                {
                    ClassDeclarationSyntax memberDeclarationSyntax = SyntaxFactory.ClassDeclaration(keyValuePair.Key)
                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                            SyntaxFactory.Token(SyntaxKind.StaticKeyword)));
                    nestedClass =
                        nestedClass.AddMembers(Recurse(keyValuePair.Value, memberDeclarationSyntax));
                }

                foreach (ContentFile file in directory.Files)
                {
                    nestedClass = nestedClass.AddMembers(SyntaxFactory.PropertyDeclaration(
                            SyntaxFactory.PredefinedType(
                                SyntaxFactory.Token(SyntaxKind.StringKeyword)),
                            SyntaxFactory.Identifier(file.Name))
                        .WithModifiers(
                            SyntaxFactory.TokenList(
                                SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                                    SyntaxFactory.Token(SyntaxKind.StaticKeyword))))
                        .WithExpressionBody(
                            SyntaxFactory.ArrowExpressionClause(
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    SyntaxFactory.Literal(file.FullPath))))
                        .WithSemicolonToken(
                            SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                        .NormalizeWhitespace());
                }

                return nestedClass;
            }

            rootClass = Recurse(root, rootClass);

            NamespaceDeclarationSyntax nameSpace = SyntaxFactory
                .NamespaceDeclaration(SyntaxFactory.ParseName(@namespace))
                .AddMembers(
                    rootClass);

            File.WriteAllText(output, nameSpace.NormalizeWhitespace().ToFullString());
            Console.WriteLine("Successfully generated content file");
        }
    }
}