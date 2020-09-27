﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MudBlazor.Docs.Compiler
{
    class Program
    {
        const string DocDir = "MudBlazor.Docs";
        const string SnippetsFile = "Snippets.generated.cs";

        static void Main(string[] args)
        {
            var path = Path.GetFullPath(".");
            var src_path = string.Join("/", path.Split('/', '\\').TakeWhile(x => x != "src").Concat(new[] { "src" }));
            var doc_path = Directory.EnumerateDirectories(src_path, DocDir).FirstOrDefault();
            if (doc_path == null)
                throw new InvalidOperationException("Directory not found: " + DocDir);
            var snippets_path = Directory.EnumerateFiles(doc_path, SnippetsFile, SearchOption.AllDirectories).FirstOrDefault();
            if (snippets_path == null)
                throw new InvalidOperationException("File not found: " + SnippetsFile);
            //Console.WriteLine(path);
            //Console.WriteLine(src_path);
            //Console.WriteLine(doc_path);
            using (var f = File.Open(snippets_path, FileMode.Create))
            using(var w = new StreamWriter(f))
            {
                w.WriteLine("// NOTE: this file is autogenerated. Any changes will be overwritten!");
                w.WriteLine(
@"namespace MudBlazor.Docs.Models
{
    public static partial class Snippets
    {
");
                foreach (var entry in Directory.EnumerateFiles(doc_path, "*.razor", SearchOption.AllDirectories))
                {
                    var filename = Path.GetFileName(entry);
                    var component_name = Path.GetFileNameWithoutExtension(filename);
                    if (!filename.Contains("Code"))
                        continue;
                    Console.WriteLine("Found code snippet: " + component_name);
                    w.WriteLine($"public const string {component_name} = @\"```html");
                    w.WriteLine(EscapeComponentSource(entry));
                    w.WriteLine("```\";");
                }
                w.WriteLine(
@"    }
}
");
                w.Flush();
            }
        }

        private static string EscapeComponentSource(string path)
        {
            var source = File.ReadAllText(path, Encoding.UTF8);
            source = Regex.Replace(source, "@using .+?\n", "");
            source = Regex.Replace(source, "@namespace .+?\n", "");
            return source.Replace("\"", "\"\"").Trim();
        }
    }
}
