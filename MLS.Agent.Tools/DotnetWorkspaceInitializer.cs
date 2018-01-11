using System;
using System.IO;
using System.Threading.Tasks;

namespace MLS.Agent.Tools
{
    public class DotnetWorkspaceInitializer : IWorkspaceInitializer
    {
        public string Template { get; }

        public string Name { get; }

        public DotnetWorkspaceInitializer(string template, string name)
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(template));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            }

            Template = template;

            Name = name;
        }

        public Task Initialize(DirectoryInfo directory)
        {
            var dotnet = new Dotnet(directory);
            dotnet
                .New(Template, args: $"--name \"{Name}\" --output \"{directory.FullName}\"")
                .ThrowOnFailure();

            return Task.CompletedTask;
        }
    }
}
