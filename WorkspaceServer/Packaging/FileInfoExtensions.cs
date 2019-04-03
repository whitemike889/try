using System;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;

namespace WorkspaceServer.Packaging
{
    internal static class FileInfoExtensions
    {
        public static  string GetTargetFramework(this FileInfo project)
        {
            if (project.Exists)
            {
                var dom = XElement.Parse(File.ReadAllText(project.FullName));
                var targetFramework = dom.XPathSelectElement("//TargetFramework");
                return targetFramework?.Value ?? string.Empty;
            }
            return string.Empty;
        }

        public static string GetLanguageVersion(this FileInfo project)
        {
            if (project.Exists)
            {
                var dom = XElement.Parse(File.ReadAllText(project.FullName));
                var languageVersion = dom.XPathSelectElement("//LangVersion");
                return languageVersion?.Value ?? "7.3";
            }
            return string.Empty;
        }

        public static void SetLanguageVersion(this FileInfo project, string version)
        {
            var dom = XElement.Parse(File.ReadAllText(project.FullName));
            var langElement = dom.XPathSelectElement("//LangVersion");

            if (langElement != null)
            {
                langElement.Value = version;
            }
            else
            {
                var propertyGroup = dom.XPathSelectElement("//PropertyGroup");
                propertyGroup?.Add(new XElement("LangVersion", version));
            }

            File.WriteAllText(project.FullName, dom.ToString());
        }

        public static void TrySetLanguageVersion(this FileInfo project, string version)
        {
            var supported = CSharpLanguageSelector.GetCSharpLanguageVersion(project.GetTargetFramework());

            var canSet = StringComparer.OrdinalIgnoreCase.Equals( supported ,version);
            if (canSet)
            {

                var dom = XElement.Parse(File.ReadAllText(project.FullName));
                var langElement = dom.XPathSelectElement("//LangVersion");

                if (langElement != null)
                {
                    langElement.Value = version;
                }
                else
                {
                    var propertyGroup = dom.XPathSelectElement("//PropertyGroup");
                    propertyGroup?.Add(new XElement("LangVersion", version));
                }

                File.WriteAllText(project.FullName, dom.ToString());
            }
        }
    }
}